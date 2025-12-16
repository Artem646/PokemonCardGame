using System.Collections.Generic;
using Firebase.Auth;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        auth = FirebaseAuthService.Instance.GetAuth();

        if (auth.CurrentUser != null && currentProvider == null && !IsRestored)
        {
            RestoreSession();
        }
    }

    private async void RestoreSession()
    {
        Debug.Log("[P][AuthManager] Попытка восстановления сессии...");

        FirebaseUser currentUser = auth.CurrentUser;
        if (currentUser == null)
        {
            Debug.Log("[P][AuthManager] Нет локального пользователя — переход к SignInScene.");
            SceneManager.LoadScene("SignInScene");
            return;
        }

        try
        {
            var token = await currentUser.TokenAsync(true);
            AuthType type = GetAuthTypeFromProviderData(currentUser);
            if (type != AuthType.Unknown)
            {
                currentProvider = factory.CreateAuthProvider(type);
                IsRestored = true;
                Debug.Log($"[P][AuthManager] Сессия восстановлена как {type}: {currentUser.UserId}");
            }
            else
            {
                Debug.LogWarning("[P][AuthManager] Не удалось определить тип провайдера.");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[P][AuthManager] Ошибка восстановления токена: {e.Message}");
        }
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