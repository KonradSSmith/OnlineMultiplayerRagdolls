using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System.Threading.Tasks;
using System;
using UnityEngine.SceneManagement;
using System.Linq;

public class NetworkRunnerHandler : MonoBehaviour
{
    [SerializeField] NetworkRunner networkRunnerPrefab;
    NetworkRunner networkRunner;


    private void Awake()
    {
        networkRunner = FindFirstObjectByType<NetworkRunner>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (networkRunner == null)
        {
            networkRunner = Instantiate(networkRunnerPrefab);
            networkRunner.name = "Network Runner";
        }

        var clientTask = InitializeNetworkRunner(networkRunner, GameMode.AutoHostOrClient, "Test Session", NetAddress.Any(), SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex), null);

        Utils.DebugLog("InitializeNetworkRunner called");


    }

    INetworkSceneManager GetSceneManager(NetworkRunner runner)
    {
        INetworkSceneManager sceneManager;

        if (runner.GetComponents(typeof(MonoBehaviour)).OfType<INetworkSceneManager>().FirstOrDefault()  != null)
        {
            sceneManager = runner.GetComponents(typeof(MonoBehaviour)).OfType<INetworkSceneManager>().FirstOrDefault();
        }
        else
        {
            sceneManager = runner.gameObject.AddComponent<NetworkSceneManagerDefault>();
        }

        return sceneManager;
    }

    protected virtual Task InitializeNetworkRunner(NetworkRunner networkRunner, GameMode gameMode, string sessionName, NetAddress address, SceneRef scene, Action<NetworkRunner> initialized)
    {
        INetworkSceneManager sceneManager = GetSceneManager(networkRunner);

        networkRunner.ProvideInput = true;

        return networkRunner.StartGame(new StartGameArgs
        {
            GameMode = gameMode,
            Address = address,
            Scene = scene,
            SessionName = sessionName,
            CustomLobbyName = "OurLobbyID",
            SceneManager = sceneManager
        });
    }
}
