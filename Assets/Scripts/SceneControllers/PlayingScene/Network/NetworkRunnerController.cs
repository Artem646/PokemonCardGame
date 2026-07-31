using System.Threading.Tasks;
using Fusion;
using UnityEngine;

public class NetworkRunnerController : MonoBehaviour
{
    public static NetworkRunnerController Instance { get; private set; }
    public NetworkRunner NetworkRunner { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void InitializeRunnerIfNeeded()
    {
        if (NetworkRunner == null)
        {
            GameObject networkRunnerObject = new("NetworkRunnerObject");
            NetworkRunner = networkRunnerObject.AddComponent<NetworkRunner>();
            NetworkRunner.ProvideInput = true;
        }
    }

    public async Task JoinLobby()
    {
        InitializeRunnerIfNeeded();
        await NetworkRunner.JoinSessionLobby(SessionLobby.ClientServer);
    }

    public async Task<StartGameResult> JoinRoom(string roomName)
    {
        Debug.Log($"Подключение к комнате: {roomName}...");

        InitializeRunnerIfNeeded();

        INetworkSceneManager sceneManager = gameObject.GetComponent<INetworkSceneManager>() ?? gameObject.AddComponent<NetworkSceneManagerDefault>();
        return await NetworkRunner.StartGame(new StartGameArgs()
        {
            GameMode = ConnectionConfig.Mode,
            SessionName = roomName,
            SceneManager = sceneManager
        });
    }

    public async Task DisconnectAndRejoinLobby()
    {
        if (NetworkRunner != null)
        {
            await NetworkRunner.Shutdown();
            NetworkRunner = null;
        }

        await JoinLobby();
    }
}