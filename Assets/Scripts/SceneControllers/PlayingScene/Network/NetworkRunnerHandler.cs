using Fusion;
using Fusion.Sockets;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NetworkRunnerHandler : MonoBehaviour, INetworkRunnerCallbacks
{
    [SerializeField] private NetworkPrefabRef presencePrefab;

    [SerializeField] private TextMeshProUGUI firstPlayerName;
    [SerializeField] private Image firstPlayerImage;
    [SerializeField] private TextMeshProUGUI secondPlayerName;
    [SerializeField] private Image secondPlayerImage;
    [SerializeField] private TextMeshProUGUI spectatorsCountText;

    private NetworkRunner networkRunner;
    private bool isLocalSharedModeMasterClient;

    private NetworkGameController networkGameController;

    private async void Start()
    {
        if (GameTypeConfig.CurrentType == GameType.Multiplayer)
        {
            networkRunner = NetworkRunnerController.Instance.NetworkRunner;
            isLocalSharedModeMasterClient = networkRunner.IsSharedModeMasterClient;
            networkRunner.AddCallbacks(this);

            firstPlayerName.text = "...";
            firstPlayerImage.sprite = null;
            secondPlayerName.text = "...";
            secondPlayerImage.sprite = null;

            PlayerPresence.OnPresenceChanged += UpdateInfoPanel;

            while (NetworkAvatarSyncer.Instance == null) await Task.Yield();

            NetworkAvatarSyncer.Instance.OnAvatarReceived += HandleAvatarReceived;

            UpdateInfoPanel();

            NetworkAvatarSyncer.Instance.RequestAvatars();
        }
    }

    public void UpdateInfoPanel()
    {
        PlayerPresence[] allPresences = FindObjectsByType<PlayerPresence>(FindObjectsInactive.Exclude);
        int spectatorsCount = allPresences.Count(p => p.IsSpectator);
        spectatorsCountText.text = $"{spectatorsCount}";

        firstPlayerName.text = NetworkGamePlayersInfoCache.HostName;
        secondPlayerName.text = NetworkGamePlayersInfoCache.ClientName;

        SetAvatar(NetworkGamePlayersInfoCache.HostPlayerId, firstPlayerImage);
        SetAvatar(NetworkGamePlayersInfoCache.ClientPlayerId, secondPlayerImage);
    }

    private void SetAvatar(int playerId, Image playerImage)
    {
        Texture2D texture = AvatarStorage.GetAvatar(playerId);
        if (texture != null)
        {
            playerImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            playerImage.color = Color.white;
        }
        else
        {
            playerImage.sprite = null;
            playerImage.color = new Color(0, 0, 0, 0.2f);
        }
    }

    private void HandleAvatarReceived(int playerId, Texture2D texture) => UpdateInfoPanel();

    public async void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (player == runner.LocalPlayer)
        {
            PlayerPresence[] allPresence = FindObjectsByType<PlayerPresence>(FindObjectsInactive.Exclude);
            if (!allPresence.Any(p => p.Object != null && p.Object.InputAuthority == player))
            {
                Debug.Log("Спавн визитки для позднего подключения...");
                runner.Spawn(presencePrefab, inputAuthority: player);
            }

            // PlayerPresence existingPresence = allPresence.FirstOrDefault(p => p.Object.InputAuthority == player);
            // if (existingPresence == null)
            // {
            //     Debug.Log("Спавн визитки для позднего подключения...");
            //     runner.Spawn(presencePrefab, inputAuthority: player);
            // }
        }

        await Task.Delay(800);
        NetworkAvatarSyncer.Instance.RequestAvatars();

        StartCoroutine(DelayedUpdateInfoPanel());

        if (isLocalSharedModeMasterClient)
            StartCoroutine(CheckIntruderRoutine(player));
    }

    private IEnumerator DelayedUpdateInfoPanel()
    {
        yield return null;
        UpdateInfoPanel();
    }

    private IEnumerator CheckIntruderRoutine(PlayerRef newPlayer)
    {
        yield return new WaitForSeconds(0.5f);
        if (networkGameController == null) yield break;

        PlayerPresence[] allPresence = FindObjectsByType<PlayerPresence>(FindObjectsInactive.Exclude);
        PlayerPresence newPresence = allPresence.FirstOrDefault(p => p.Object.InputAuthority == newPlayer);

        if (newPresence != null && !newPresence.IsSpectator && networkGameController.IsGameStarted)
        {
            Debug.LogWarning($"Игрок {newPlayer.PlayerId} попытался зайти в идущий матч. Выгоняем.");
            networkGameController.RpcKickForStartedGame(newPlayer.PlayerId);
        }
    }

    public async void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (player == runner.LocalPlayer) return;

        if (networkGameController == null)
            networkGameController = FindAnyObjectByType<NetworkGameController>();

        bool isFirstPlayerLeft = player == networkGameController.FirstPlayerRef;
        bool isSecondPlayerLeft = player == networkGameController.SecondPlayerRef;

        if (isFirstPlayerLeft)
        {
            NotificationManager.ShowNotification("Создатель матча покинул игру. Матч прерван.", NotificationType.Error);
            await ExitToMenu();
            return;
        }

        if (isSecondPlayerLeft)
        {
            NotificationManager.ShowNotification("Оппонент покинул игру. Вы победили!", NotificationType.Info);
            await ExitToMenu();
            return;
        }

        Debug.Log($"Зритель {player.PlayerId} покинул игру.");
        StartCoroutine(DelayedUpdateInfoPanel());
    }

    private async Task ExitToMenu()
    {
        AvatarStorage.Clear();
        await NetworkRunnerController.Instance.DisconnectAndRejoinLobby();
        SceneManager.LoadScene("RoomManagerScene");
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        networkGameController = FindAnyObjectByType<NetworkGameController>();
        UpdateInfoPanel();
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) => AvatarStorage.Clear();
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) => AvatarStorage.Clear();
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, System.ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }

    private void OnDestroy()
    {
        if (networkRunner != null) networkRunner.RemoveCallbacks(this);
        PlayerPresence.OnPresenceChanged -= UpdateInfoPanel;
        if (NetworkAvatarSyncer.Instance != null) NetworkAvatarSyncer.Instance.OnAvatarReceived -= HandleAvatarReceived;
    }
}