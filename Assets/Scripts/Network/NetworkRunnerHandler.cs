using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkRunnerHandler : MonoBehaviour, INetworkRunnerCallbacks
{
    [SerializeField] private NetworkObject networkGameStatePrefab;
    private NetworkRunner networkRunner;
    private NetworkGameState networkGameState;

    async void Start()
    {
        networkRunner = gameObject.AddComponent<NetworkRunner>();
        networkRunner.AddCallbacks(this);
        DontDestroyOnLoad(gameObject);

        await networkRunner.JoinSessionLobby(SessionLobby.ClientServer);

        Debug.Log($"[NetworkRunnerHandler] Starting game: Mode={ConnectionConfig.Mode}, Room={ConnectionConfig.RoomName}");

        await networkRunner.StartGame(new StartGameArgs
        {
            GameMode = ConnectionConfig.Mode,
            SessionName = ConnectionConfig.RoomName,
            Scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex),
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>(),
            PlayerCount = 2,
            IsVisible = true,
            IsOpen = true
        });
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        if (networkGameState == null && runner.IsSharedModeMasterClient)
        {
            NetworkObject obj = runner.Spawn(networkGameStatePrefab, flags: NetworkSpawnFlags.DontDestroyOnLoad);
            networkGameState = obj.GetComponent<NetworkGameState>();
        }

        if (networkGameState == null)
            networkGameState = FindAnyObjectByType<NetworkGameState>();

        GameManagerScript gameManager = FindAnyObjectByType<GameManagerScript>();
        if (gameManager != null && networkGameState != null)
        {
            networkGameState.BindGameManager(gameManager);
            gameManager.Init(networkGameState);
        }
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"OnPlayerJoined: PlayerId={player.PlayerId}");

        if (runner.IsSharedModeMasterClient && networkGameState != null)
            networkGameState.AssignPlayerRole(player);
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"OnPlayerLeft: PlayerId={player.PlayerId}");
    }

    public async void ShutdownRunner()
    {
        if (networkRunner != null)
        {
            Debug.Log("[NetworkRunnerHandler] Shutting down runner...");

            if (networkRunner.IsSharedModeMasterClient)
                await networkRunner.Shutdown(true);
            else
                await networkRunner.Shutdown();

            networkGameState = null;
            Destroy(networkRunner);
        }
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
}