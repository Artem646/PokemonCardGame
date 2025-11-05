using System.Collections.Generic;
using Firebase.Auth;
using UnityEngine;

public class AuthManager : MonoBehaviour
{
    public static AuthManager Instance { get; private set; }
    private IAuthFactory factory;
    private IAuthProvider currentProvider;
    private FirebaseAuth auth;
    private bool IsRestored = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;
            factory = new AuthFactory();
            DontDestroyOnLoad(gameObject);
            Debug.Log("[P][AuthManager] AuthManager успешно создан.");
        }
    }

    public void Initialize()
    {
        auth = FirebaseService.Instance.GetAuth();

        if (auth.CurrentUser != null && currentProvider == null && !IsRestored)
        {
            RestoreSession();
        }
    }

    private void RestoreSession()
    {
        Debug.Log("[P][AuthManager] Восстановление сессии...");
        AuthType type = GetAuthTypeFromProviderData(auth.CurrentUser);
        if (type != AuthType.Unknown)
        {
            currentProvider = factory.CreateAuthProvider(type);
            Debug.Log($"[P][AuthManager] Сессия восстановлена как {type}: {auth.CurrentUser.UserId}");
        }
        else
        {
            Debug.LogError("[P][AuthManager] Не удалось определить тип провайдера.");
        }

        IsRestored = true;
    }

    private AuthType GetAuthTypeFromProviderData(FirebaseUser user)
    {
        if (user.IsAnonymous)
            return AuthType.Anonymous;

        var providerDataList = new List<IUserInfo>(user.ProviderData);
        foreach (var provider in providerDataList)
        {
            if (provider.ProviderId == "google.com")
            {
                return AuthType.Google;
            }
        }

        return AuthType.Unknown;
    }

    public void SignIn(AuthType type)
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            Debug.Log("[P] Нет подключения к интернету");
            NotificationManager.ShowNotification("Нет подключения к интернету", true);
            return;
        }

        currentProvider = factory.CreateAuthProvider(type);
        if (currentProvider == null)
        {
            Debug.LogError($"[P][AuthManager] Не удалось создать провайдер для типа {type}");
            return;
        }
        currentProvider.SignIn();
        Debug.Log($"[P][AuthManager] Вход через {type} запущен.");
    }

    public void SignOut()
    {
        currentProvider.SignOut();
        currentProvider = null;
        Debug.Log("[P][AuthManager] Выход выполнен.");
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}