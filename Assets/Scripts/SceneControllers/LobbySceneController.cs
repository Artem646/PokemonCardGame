using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

static class NetworkGamePlayersInfoCache
{
    static public string HostName { get; set; }
    static public int HostPlayerId { get; set; }
    static public string ClientName { get; set; }
    static public int ClientPlayerId { get; set; }
}

public class LobbySceneController : MonoBehaviour, INetworkRunnerCallbacks
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private LobbyRoomState lobbyRoomState;
    [SerializeField] private NetworkPrefabRef avatarSyncerPrefab;
    [SerializeField] private NetworkPrefabRef presencePrefab;

    private NetworkRunner networkRunner;
    private PlayerRef? playerToKick = null;
    private bool isKicked = false;
    private bool isHostLeft = false;
    private bool isLocalSharedModeMasterClient;
    private PlayerRef? hostPlayerRef = null;

    private VisualElement root;
    private Label roomNameLabel;
    private Label hostNameLabel;
    private VisualElement hostImage;
    private Label playerNameLabel;
    private VisualElement playerImage;
    private Label spectatorsCountLabel;
    private Button startGameButton;
    private Button kickPlayerButton;
    private Button exitButton;
    private VisualElement playersInfoSection;
    private VisualElement hostNameSection;
    private VisualElement playerNameSection;
    private VisualElement hostActionButtonsSection;

    private async void Start()
    {
        InitializeUI();

        networkRunner = NetworkRunnerController.Instance.NetworkRunner;
        isLocalSharedModeMasterClient = networkRunner.IsSharedModeMasterClient;
        networkRunner.AddCallbacks(this);

        roomNameLabel.text = ConnectionConfig.RoomName;
        hostNameLabel.text = "Ожидание хоста...";
        hostImage.style.backgroundImage = null;
        playerNameLabel.text = "Ожидание игрока...";
        playerImage.style.backgroundImage = null;

        SetHostActionButtonsSectionEnable(false);

        if (isLocalSharedModeMasterClient && NetworkAvatarSyncer.Instance == null)
            await networkRunner.SpawnAsync(avatarSyncerPrefab);
        while (NetworkAvatarSyncer.Instance == null) await Task.Yield();

        LobbyRoomState.OnNamesChanged += UpdateUI;
        PlayerPresence.OnPresenceChanged += UpdateUI;
        NetworkAvatarSyncer.Instance.OnAvatarReceived += HandleAvatarReceived;

        await Task.Delay(500);

        if (!ConnectionConfig.IsSpectator)
        {
            Texture2D myTextureImage = BytesToTexture(UserSession.Instance.ActiveUser.userData.profilePhotoData);
            if (myTextureImage != null) NetworkAvatarSyncer.Instance.PrepareUserImage(myTextureImage);
        }

        UpdateUI();
        RegisterCallbacks();

        NetworkAvatarSyncer.Instance.RequestAvatars();

        if (isLocalSharedModeMasterClient)
        {
            Dictionary<string, SessionProperty> properties = new() { { "IsLobbySceneLoaded", 1 } };
            networkRunner.SessionInfo.UpdateCustomProperties(properties);
        }
    }

    private Texture2D BytesToTexture(byte[] bytes)
    {
        Texture2D texture = new(2, 2);
        if (texture.LoadImage(bytes)) return texture;
        return null;
    }

    private void InitializeUI()
    {
        root = uiDocument.rootVisualElement;
        roomNameLabel = root.Q<Label>("roomNameLabel");
        hostNameLabel = root.Q<Label>("hostNameLabel");
        hostImage = root.Q<VisualElement>("hostImage");
        playerNameLabel = root.Q<Label>("playerNameLabel");
        playerImage = root.Q<VisualElement>("playerImage");
        spectatorsCountLabel = root.Q<Label>("spectatorsCountLabel");
        startGameButton = root.Q<Button>("startGameButton");
        kickPlayerButton = root.Q<Button>("kickPlayerButton");
        exitButton = root.Q<Button>("exitButton");
        playersInfoSection = root.Q<VisualElement>("playersInfoSection");
        hostNameSection = root.Q<VisualElement>("hostNameSection");
        playerNameSection = root.Q<VisualElement>("playerNameSection");
        hostActionButtonsSection = root.Q<VisualElement>("hostActionButtonsSection");
    }

    private void RegisterCallbacks()
    {
        startGameButton.RegisterCallback<ClickEvent>(evt =>
        {
            SetUIEnabled(false);

            Dictionary<string, SessionProperty> properties = new() { { "IsStarted", 1 } };
            networkRunner.SessionInfo.UpdateCustomProperties(properties);

            int loadingGameSceneIndex = SceneUtility.GetBuildIndexByScenePath("Assets/Scenes/GameLoadingScene.unity");
            networkRunner.LoadScene(SceneRef.FromIndex(loadingGameSceneIndex));
        });

        kickPlayerButton.RegisterCallback<ClickEvent>(evt =>
        {
            SetUIEnabled(false);
            if (playerToKick != null) lobbyRoomState.RpcKickPlayer(playerToKick.Value.PlayerId);
        });

        exitButton.RegisterCallback<ClickEvent>(async evt =>
        {
            SetUIEnabled(false);
            AvatarStorage.Clear();
            await NetworkRunnerController.Instance.DisconnectAndRejoinLobby();
            SceneManager.LoadScene("RoomManagerScene");
        });
    }

    private void UpdateUI()
    {
        if (isKicked) return;
        if (lobbyRoomState == null || lobbyRoomState.Object == null || !lobbyRoomState.Object.IsValid)
            return;

        hostPlayerRef = lobbyRoomState.Object.StateAuthority;

        PlayerPresence[] allPresences = FindObjectsByType<PlayerPresence>(FindObjectsInactive.Exclude);
        List<PlayerPresence> realPlayers = allPresences.Where(p => !p.IsSpectator).OrderBy(p => p.Object.InputAuthority.PlayerId).ToList();

        bool isIntruder = false;
        int myIndex = realPlayers.FindIndex(p => p.Object.InputAuthority == networkRunner.LocalPlayer);
        if (myIndex >= 2) isIntruder = true;

        if (isIntruder)
        {
            hostNameLabel.text = "Неизвестно";
            hostImage.style.backgroundImage = null;
            playerNameLabel.text = "Неизвестно";
            playerImage.style.backgroundImage = null;
        }

        if (isLocalSharedModeMasterClient)
        {
            if (realPlayers.Count > 2)
            {
                for (int i = 2; i < realPlayers.Count; i++)
                {
                    int intruderId = realPlayers[i].Object.InputAuthority.PlayerId;
                    Debug.LogWarning($"[Lobby] Обнаружен лишний игрок ID: {intruderId}. Отправка RPC кика...");
                    lobbyRoomState.RpcKickForLimit(intruderId);
                }
            }
        }

        if (!isIntruder)
        {
            string hostName = lobbyRoomState.HostName.ToString();
            string clientName = lobbyRoomState.ClientName.ToString();

            hostNameLabel.text = !string.IsNullOrEmpty(hostName) ? hostName : "Ожидание хоста...";
            playerNameLabel.text = !string.IsNullOrEmpty(clientName) ? clientName : "Ожидание игрока...";

            NetworkGamePlayersInfoCache.HostName = hostName;
            NetworkGamePlayersInfoCache.ClientName = clientName;

            if (realPlayers.Count > 0)
            {
                int hostId = realPlayers[0].Object.InputAuthority.PlayerId;
                NetworkGamePlayersInfoCache.HostPlayerId = hostId;
                Texture2D hostTexture = AvatarStorage.GetAvatar(hostId);
                hostImage.style.backgroundImage = hostTexture != null ? new StyleBackground(hostTexture) : null;
            }

            if (realPlayers.Count > 1)
            {
                int clientId = realPlayers[1].Object.InputAuthority.PlayerId;
                NetworkGamePlayersInfoCache.ClientPlayerId = clientId;
                Texture2D clientTexture = AvatarStorage.GetAvatar(clientId);
                playerImage.style.backgroundImage = clientTexture != null ? new StyleBackground(clientTexture) : null;
            }
            else
            {
                playerImage.style.backgroundImage = null;
                NetworkGamePlayersInfoCache.ClientPlayerId = 0;
            }

            int spectatorsCount = allPresences.Count(p => p.IsSpectator);
            spectatorsCountLabel.text = $"{spectatorsCount}";

            if (realPlayers.Count > 1 && isLocalSharedModeMasterClient)
            {
                playerToKick = realPlayers[1].Object.InputAuthority;
                SetHostActionButtonsSectionEnable(true);
            }
            else
            {
                playerToKick = null;
                SetHostActionButtonsSectionEnable(false);
            }
        }
    }

    private void HandleAvatarReceived(int playerId, Texture2D texture) => UpdateUI();

    public async void HandleKicked(string reasonText)
    {
        isKicked = true;
        SetUIEnabled(false);
        playerNameLabel.text = reasonText;

        AvatarStorage.Clear();

        await Task.Delay(2000);

        await NetworkRunnerController.Instance.DisconnectAndRejoinLobby();
        SceneManager.LoadScene("RoomManagerScene");
    }

    public async void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (player == runner.LocalPlayer)
            runner.Spawn(presencePrefab, inputAuthority: player);

        await Task.Delay(800);
        NetworkAvatarSyncer.Instance.RequestAvatars();

        StartCoroutine(DelayedUpdateUI());
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (player == runner.LocalPlayer) return;

        if (hostPlayerRef.HasValue && hostPlayerRef.Value == player)
            isHostLeft = true;

        else if (lobbyRoomState != null && lobbyRoomState.Object != null && lobbyRoomState.Object.IsValid)
        {
            if (lobbyRoomState.Object.StateAuthority == player)
                isHostLeft = true;
        }

        if (isHostLeft)
        {
            AvatarStorage.Clear();
            NotificationManager.ShowNotification("Создатель комнаты покинул игру. Комната закрыта.", NotificationType.Error);
            Debug.Log("Хост вышел. Покидаем комнату...");
            HandleKicked("Хост покинул комнату. Комната закрыта");
            return;
        }

        AvatarStorage.RemoveAvatar(player.PlayerId);
        Debug.Log($"[Lobby] Удален аватар ушедшего игрока ID: {player.PlayerId}");

        // Если мы Хост, проверяем, кто именно вышел (Клиент или Зритель)
        if (isLocalSharedModeMasterClient)
            StartCoroutine(CheckWhoLeftRoutine(player));
        else
        {
            // Если мы Клиент или Зритель, просто обновляем счетчики
            StartCoroutine(DelayedUpdateUI());
        }
    }

    private IEnumerator DelayedUpdateUI()
    {
        yield return null;
        UpdateUI();
    }

    private IEnumerator CheckWhoLeftRoutine(PlayerRef leftPlayer)
    {
        // Ждем 1 кадр, чтобы Fusion успел уничтожить PlayerPresence вышедшего игрока
        yield return DelayedUpdateUI();

        // Проверяем, осталась ли визитка Клиента. Если нет - значит вышел именно он.
        if (lobbyRoomState.FindPresenceForClient() == null)
        {
            string leftPlayerName = lobbyRoomState.ClientName.ToString();
            if (!string.IsNullOrEmpty(leftPlayerName))
                NotificationManager.ShowNotification($@"Игрок ""{leftPlayerName}"" покинул лобби", NotificationType.Info);
            lobbyRoomState.ClearClientName();

            AvatarStorage.RemoveAvatar(leftPlayer.PlayerId);
            Debug.Log($"[Lobby] Удален аватар ушедшего игрока ID: {leftPlayer.PlayerId}");
        }
    }

    private void SetHostActionButtonsSectionEnable(bool isEnabled)
    {
        if (isEnabled)
        {
            hostActionButtonsSection.style.display = DisplayStyle.Flex;
            playersInfoSection.style.width = new StyleLength(new Length(80, LengthUnit.Percent));
            hostNameSection.style.borderRightWidth = 3f;
            playerNameSection.style.borderRightWidth = 3f;

            SetUIEnabled(true);

            Dictionary<string, SessionProperty> properties = new() { { "IsRoomFull", 1 } };
            networkRunner.SessionInfo.UpdateCustomProperties(properties);
        }
        else
        {
            hostActionButtonsSection.style.display = DisplayStyle.None;
            playersInfoSection.style.width = new StyleLength(new Length(100, LengthUnit.Percent));
            hostNameSection.style.borderRightWidth = 0;
            playerNameSection.style.borderRightWidth = 0;

            if (isLocalSharedModeMasterClient)
            {
                Dictionary<string, SessionProperty> properties = new() { { "IsRoomFull", 0 } };
                networkRunner.SessionInfo.UpdateCustomProperties(properties);
            }
        }
    }

    private void SetUIEnabled(bool isEnabled)
    {
        startGameButton.SetEnabled(isEnabled);
        kickPlayerButton.SetEnabled(isEnabled);
        exitButton.SetEnabled(isEnabled);
    }

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
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }

    private void OnDestroy()
    {
        if (networkRunner != null) networkRunner.RemoveCallbacks(this);
        LobbyRoomState.OnNamesChanged -= UpdateUI;
        PlayerPresence.OnPresenceChanged -= UpdateUI;
        NetworkAvatarSyncer.Instance.OnAvatarReceived -= HandleAvatarReceived;
    }
}
