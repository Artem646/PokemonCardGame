using UnityEngine;
using Firebase.Extensions;

public class FirebaseInitializer
{
    private static FirebaseInitializer _instance;
    public static FirebaseInitializer Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new FirebaseInitializer();
            }
            return _instance;
        }
    }

    private bool isInitialized = false;
    private Firebase.DependencyStatus dependencyStatus = Firebase.DependencyStatus.UnavailableOther;
    private FirebaseInitializer() { }

    public async void Initialize()
    {
        if (isInitialized)
        {
            return;
        }

        Debug.Log("[P][FirebaseInitializer] Starting Firebase dependency check...");

        await Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                try
                {
                    FirebaseService.Instance.InitializeFirebaseAuth();
                    FirebaseService.Instance.InitializeFirebaseFirestore();
                    Debug.Log("[P][FirebaseInitializer] FirebaseInitializer initialized successfully.");

                    AuthManager.Instance.Initialize();
                    isInitialized = true;
                }
                catch (System.Exception e)
                {
                    Debug.Log("[P][FirebaseInitializer] Failed to initialize FirebaseInitializer: " + e.Message);
                }
            }
            else
            {
                Debug.Log("[P][FirebaseInitializer] Could not resolve all Firebase dependencies: " + dependencyStatus);
            }
        });
    }
}