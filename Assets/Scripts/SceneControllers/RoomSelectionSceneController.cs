using UnityEngine;
using UnityEngine.UIElements;
using Fusion;
using System.Collections.Generic;
using Fusion.Sockets;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

public class RoomSelectionSceneController : MonoBehaviour, INetworkRunnerCallbacks
{
    [SerializeField] private UIDocument uiDocument;
    private NetworkRunner runner;
    private NetworkRunnerHandler networkRunnerHandler;
    private VisualElement root;
    private ScrollView roomList;
    private TextField roomNameField;
    private Button createRoomButton;
    private Button refreshButton;
    private Button backButton;
    private VisualElement loadingOverlay;

    private async void Start()
    {
        InitializeUI();
        loadingOverlay.style.display = DisplayStyle.Flex;
        await InitializeNetworkRunner();
        await Task.Delay(700);
        loadingOverlay.style.display = DisplayStyle.None;
        RegisterEvents();
    }

    private void InitializeUI()
    {
        root = uiDocument.rootVisualElement;
        roomList = root.Q<ScrollView>("roomList");
        roomNameField = root.Q<TextField>("roomNameField");
        createRoomButton = root.Q<Button>("createRoomButton");
        refreshButton = root.Q<Button>("refreshButton");
        backButton = root.Q<Button>("backButton");
        loadingOverlay = root.Q<VisualElement>("loadingOverlay");
    }

    private void RegisterEvents()
    {
        createRoomButton.clicked += CreateRoom;
        refreshButton.clicked += RefreshRooms;
        backButton.clicked += BackToDeckSelection;
    }

    private async Task InitializeNetworkRunner()
    {
        runner = gameObject.AddComponent<NetworkRunner>();
        runner.AddCallbacks(this);
        networkRunnerHandler = FindAnyObjectByType<NetworkRunnerHandler>();
        await runner.JoinSessionLobby(SessionLobby.ClientServer);
    }

    private void CreateRoom()
    {
        ConnectionConfig.RoomName = string.IsNullOrEmpty(roomNameField.value) ? "DefaultRoom" : roomNameField.value;
        SceneManager.LoadScene("TestConnScene");
    }

    private void ConnectToRoom(string roomName)
    {
        ConnectionConfig.RoomName = roomName;
        SceneManager.LoadScene("TestConnScene");
    }

    private async void RefreshRooms()
    {
        Debug.Log("Запрос обновления списка комнат...");
        await runner.JoinSessionLobby(SessionLobby.ClientServer);
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        roomList.Clear();

        foreach (var session in sessionList)
        {
            Button button = new() { text = $"{session.Name}" };
            button.style.fontSize = 30;
            button.clicked += () => ConnectToRoom(session.Name);
            roomList.Add(button);
        }
    }

    private void BackToDeckSelection()
    {
        if (networkRunnerHandler != null)
            networkRunnerHandler.ShutdownRunner();
        SceneManager.LoadScene("DeckSelectionScene");
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
        if (networkRunnerHandler != null)
        {
            networkRunnerHandler.ShutdownRunner();
        }
    }
}
