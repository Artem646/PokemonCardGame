// Start вызывается перед первым обновлением фрейма
// Когда приложение запустится, проверьте, есть ли у нас
// необходимые зависимости для использования Firebase, и если нет,
// добавьте их, если это возможно.

using UnityEngine;
using Firebase.Extensions;

public class FirebaseInitializer : MonoBehaviour
{
    private Firebase.DependencyStatus dependencyStatus = Firebase.DependencyStatus.UnavailableOther;

    private async void Start()
    {
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