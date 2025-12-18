using Fusion;
using Fusion.Sockets;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkRunnerHandler : MonoBehaviour, INetworkRunnerCallbacks
{
    [SerializeField] private NetworkObject networkGameStatePrefab;
    private NetworkRunner networkRunner;
    private NetworkGameState networkGameState;
    private bool sceneLoaded = false;

    async void Start()
    {
        networkRunner = gameObject.AddComponent<NetworkRunner>();
        networkRunner.AddCallbacks(this);
        DontDestroyOnLoad(gameObject);

        await networkRunner.StartGame(new StartGameArgs
        {
            GameMode = ConnectionConfig.Mode,
            SessionName = ConnectionConfig.RoomName,
            Scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex),
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>(),
        });
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        if (runner.IsServer && networkGameState == null)
        {
            NetworkObject obj = runner.Spawn(networkGameStatePrefab);
            networkGameState = obj.GetComponent<NetworkGameState>();
        }
    }

    void Update()
    {
        if (networkRunner != null && networkRunner.IsServer && networkGameState != null)
        {
            if (networkGameState.FirstPlayerReady && networkGameState.SecondPlayerReady && !sceneLoaded)
            {
                sceneLoaded = true;
                Debug.Log("[Handler] Both ready → LoadScene('PlayingScene')");
                int index = SceneUtility.GetBuildIndexByScenePath("Assets/Scenes/PlayingScene.unity");
                networkRunner.LoadScene(SceneRef.FromIndex(index));
            }
        }
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"OnPlayerJoined: PlayerId={player.PlayerId}, IsServer={runner.IsServer}");
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"OnPlayerLeft: PlayerId={player.PlayerId}");
    }

    public NetworkRunner Runner => networkRunner;

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
}