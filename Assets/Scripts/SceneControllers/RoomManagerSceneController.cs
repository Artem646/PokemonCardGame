using UnityEngine;
using UnityEngine.UIElements;
using Fusion;
using System.Collections.Generic;
using Fusion.Sockets;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

public class RoomManagerSceneController : MonoBehaviour, INetworkRunnerCallbacks
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private RoomController roomController;
    [SerializeField] private DeckSelectionDialogController deckSelectionDialogController;
    [SerializeField] private RoomCreationDialogController roomCreationDialogController;
    [SerializeField] private FriendSelectionDialogController friendSelectionDialogController;

    public string SelectedRoomName { get; private set; }

    private VisualElement root;
    private VisualElement roomRoot;
    private ScrollView roomList;
    private VisualElement selectedRoomElement = null;
    private Button joinRoomButton;
    private Button joinFriendButton;
    private Button createRoomButton;
    private Button selectDeckButton;
    private Button spectateRoomButton;
    private Button backButton;
    private VisualElement loadingOverlay;

    private Dictionary<string, SessionInfo> availableSessions = new();

    private async void Start()
    {
        InitializeUI();

        joinRoomButton.SetEnabled(false);
        spectateRoomButton.SetEnabled(false);

        loadingOverlay.style.display = DisplayStyle.Flex;
        await NetworkRunnerController.Instance.JoinLobby();
        NetworkRunnerController.Instance.NetworkRunner.AddCallbacks(this);
        loadingOverlay.style.display = DisplayStyle.None;

        RegisterCallbacks();

        SelectedDeckManager.Clear();
    }

    private void InitializeUI()
    {
        root = uiDocument.rootVisualElement;
        roomList = root.Q<ScrollView>("roomList");
        joinRoomButton = root.Q<Button>("joinRoomButton");
        joinFriendButton = root.Q<Button>("joinFriendButton");
        createRoomButton = root.Q<Button>("createRoomButton");
        selectDeckButton = root.Q<Button>("selectDeckButton");
        spectateRoomButton = root.Q<Button>("spectateRoomButton");
        backButton = root.Q<Button>("backButton");
        loadingOverlay = root.Q<VisualElement>("loadingOverlay");
    }

    private void RegisterCallbacks()
    {
        joinRoomButton.RegisterCallback<ClickEvent>(async evt =>
        {
            if (string.IsNullOrEmpty(SelectedRoomName))
            {
                NotificationManager.ShowNotification("Комната не выбрана!", NotificationType.Info);
                return;
            }

            if (SelectedDeckManager.SelectedDeck == null)
            {
                NotificationManager.ShowNotification("Колода не выбрана!", NotificationType.Info);
                return;
            }

            if (availableSessions.TryGetValue(SelectedRoomName, out SessionInfo session))
            {
                if (session.Properties != null && session.Properties.TryGetValue("IsRoomFull", out SessionProperty property))
                {
                    if (property.IsInt && (int)property.PropertyValue == 0)
                    {
                        ConnectionConfig.IsSpectator = false;
                        await ConnectToSelectedRoom();
                    }
                    else
                        NotificationManager.ShowNotification("Комната уже полная. Можно войти как зритель, понаблюдать!", NotificationType.Info);
                }
            }
        });

        joinFriendButton.RegisterCallback<ClickEvent>(evt =>
        {
            friendSelectionDialogController.OpenFriendSelectionDialog();
        });

        createRoomButton.RegisterCallback<ClickEvent>(evt =>
        {
            ConnectionConfig.IsSpectator = false;
            roomCreationDialogController.OpenRoomCreationDialog();
        });

        selectDeckButton.RegisterCallback<ClickEvent>(evt =>
        {
            deckSelectionDialogController.OpenDeckSelectionDialog();
        });

        spectateRoomButton.RegisterCallback<ClickEvent>(async evt =>
        {
            if (string.IsNullOrEmpty(SelectedRoomName))
            {
                NotificationManager.ShowNotification("Комната не выбрана!", NotificationType.Info);
                return;
            }

            ConnectionConfig.IsSpectator = true;
            bool isGameAlreadyRunning = false;

            if (availableSessions.TryGetValue(SelectedRoomName, out SessionInfo session))
            {
                if (session.Properties != null && session.Properties.TryGetValue("IsStarted", out SessionProperty property))
                {
                    if (property.IsInt && (int)property.PropertyValue == 1)
                        isGameAlreadyRunning = true;
                }
            }

            if (isGameAlreadyRunning)
            {
                Debug.Log("[Spectator] Игра уже идет. Загружаем GameLoadingScene...");
                ConnectionConfig.RoomName = SelectedRoomName;
                ConnectionConfig.IsLateSpectator = true;

                AsyncOperation operation = SceneManager.LoadSceneAsync("GameLoadingScene", LoadSceneMode.Additive);
                while (!operation.isDone) await Task.Yield();

                await ConnectToSelectedRoom();
            }
            else
            {
                Debug.Log("[Spectator] Игра еще в лобби. Просто подключаемся.");
                ConnectionConfig.IsLateSpectator = false;
                await ConnectToSelectedRoom();
            }
        });

        backButton.RegisterCallback<ClickEvent>(evt =>
        {
            SceneManager.LoadScene("StartPlayScene");
        });
    }

    private async Task ConnectToSelectedRoom()
    {
        loadingOverlay.style.display = DisplayStyle.Flex;
        ConnectionConfig.RoomName = SelectedRoomName;

        StartGameResult result = await NetworkRunnerController.Instance.JoinRoom(ConnectionConfig.RoomName);
        if (result.Ok)
            Debug.Log("Успешное подключение к комнате!");
        else
        {
            Debug.LogError($"Ошибка подключения: {result.ShutdownReason}");
            NotificationManager.ShowNotification("Не удалось подключиться к комнате", NotificationType.Error);
            loadingOverlay.style.display = DisplayStyle.None;
        }
    }

    public async void RefreshRooms() => await NetworkRunnerController.Instance.JoinLobby();

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        roomList.Clear();
        availableSessions.Clear();
        selectedRoomElement = null;
        SelectedRoomName = null;
        joinRoomButton.SetEnabled(false);

        foreach (SessionInfo session in sessionList)
        {
            bool roomToShow = false;

            if (session.Properties != null && session.Properties.TryGetValue("IsLobbySceneLoaded", out SessionProperty propertyFirst))
            {
                if (propertyFirst.IsInt && (int)propertyFirst.PropertyValue == 1)
                    roomToShow = true;
            }

            if (roomToShow)
            {
                availableSessions[session.Name] = session;
                roomRoot = roomController.InstantiateRoomElement(session.Name, OnRoomElementClicked);
                roomList.Add(roomRoot);
            }
        }
    }

    private void OnRoomElementClicked(VisualElement clickedRoomElement)
    {
        if (selectedRoomElement == clickedRoomElement)
        {
            roomController.ToggleCheckmark(false);
            selectedRoomElement = null;
            SelectedRoomName = null;
            joinRoomButton.SetEnabled(false);
            spectateRoomButton.SetEnabled(false);
        }
        else
        {
            if (selectedRoomElement != null)
                roomController.ToggleCheckmark(false);

            selectedRoomElement = clickedRoomElement;
            SelectedRoomName = (string)clickedRoomElement.userData;

            roomController.ToggleCheckmark(true);
            joinRoomButton.SetEnabled(true);
            spectateRoomButton.SetEnabled(true);
        }
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, System.ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }

    private void OnDestroy()
    {
        if (NetworkRunnerController.Instance.NetworkRunner != null)
            NetworkRunnerController.Instance.NetworkRunner.RemoveCallbacks(this);
    }
}
