using System;
using System.Collections.Generic;
using Firebase.Extensions;
using Firebase.Auth;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using Google;

public class FirebaseAuthService
{
    private static FirebaseAuthService _instance;
    public static FirebaseAuthService Instance => _instance ??= new FirebaseAuthService();

    private FirebaseAuth auth;
    public FirebaseAuth GetAuth() => auth;

    private readonly Dictionary<string, FirebaseUser> userByAuth = new();
    private bool isProcessingStateChange = false;
    private bool isAuthInitialized = false;
    private string lastToken = null;
    private readonly HashSet<string> loggedProviderIds = new();
    private bool isAnonymous;

    private FirebaseAuthService() { }

    public void InitializeFirebaseAuth()
    {
        if (isAuthInitialized) return;

        auth = FirebaseAuth.DefaultInstance;
        if (auth == null)
        {
            Debug.LogError("[P][FirebaseService] Ошибка: Firebase Auth не проинициализирован.");
            return;
        }

        auth.StateChanged += async (s, e) => await AuthStateChanged(s, e);
        auth.IdTokenChanged += IdTokenChanged;

        isAuthInitialized = true;

        FirebaseFirestoreService.Instance.InitializeFirebaseFirestore();

        Debug.Log("[P][FirebaseService] Firebase Auth успешно инициализирован.");
    }

    private async Task AuthStateChanged(object sender, EventArgs eventArgs)
    {
        if (isProcessingStateChange) return;

        isProcessingStateChange = true;
        FirebaseAuth senderAuth = sender as FirebaseAuth;
        FirebaseUser previousUser = null;

        if (senderAuth != null) userByAuth.TryGetValue(senderAuth.App.Name, out previousUser);
        if (senderAuth == auth && senderAuth.CurrentUser != previousUser)
        {
            bool signedIn = previousUser != senderAuth.CurrentUser && senderAuth.CurrentUser != null && auth.CurrentUser.IsValid();

            if (!signedIn && previousUser != null)
            {
                Debug.Log("[P][FirebaseService] Signed out " + previousUser.UserId);

                if (isAnonymous && UserSession.Instance.ActiveUser != null)
                {
                    await FirebaseFirestoreService.Instance.DeleteAnonymousUserDocument(UserSession.Instance.ActiveUser);
                    UserSession.Instance.ClearActiveUser();
                }

                userByAuth[senderAuth.App.Name] = null;
                SceneManager.LoadScene("SignInScene");
            }

            previousUser = senderAuth.CurrentUser;
            userByAuth[senderAuth.App.Name] = previousUser;

            if (signedIn)
            {
                Debug.Log("[P][FirebaseService] Signed in " + previousUser.UserId);
                DisplayDetailedUserInfo(previousUser, 1);
                SceneManager.LoadScene("UploadingScene");
                isAnonymous = previousUser.IsAnonymous;
            }
        }
        else
        {
            SceneManager.LoadScene("SignInScene");
        }

        isProcessingStateChange = false;
    }

    private void IdTokenChanged(object sender, EventArgs eventArgs)
    {
        FirebaseAuth senderAuth = sender as FirebaseAuth;
        FirebaseUser currentUser = senderAuth.CurrentUser;

        if (senderAuth == auth && currentUser != null)
        {
            currentUser.TokenAsync(false).ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError("[P][FirebaseService] Token retrieval failed: " + task.Exception);
                    return;
                }

                string token = task.Result;

                if (token != lastToken)
                {
                    lastToken = token;
                    Debug.Log($"[P][FirebaseService] Новый токен: {token.Substring(0, 8)}...");
                }
            });
        }
    }

    private void DisplayDetailedUserInfo(FirebaseUser user, int indentLevel)
    {
        string indent = new(' ', indentLevel * 2);

        DisplayUserInfo(user, indentLevel);

        Debug.Log(string.Format("[P][FirebaseService] {0}Anonymous: {1}", indent, user.IsAnonymous));
        Debug.Log(string.Format("[P][FirebaseService] {0}Email Verified: {1}", indent, user.IsEmailVerified));

        var providerDataList = new List<IUserInfo>(user.ProviderData);
        var numberOfProviders = providerDataList.Count;

        if (numberOfProviders > 0)
        {
            for (int i = 0; i < numberOfProviders; ++i)
            {
                var provider = providerDataList[i];
                if (loggedProviderIds.Add(provider.ProviderId))
                {
                    Debug.Log(string.Format("[P][FirebaseService] {0}Provider Data: {1}", indent, i));
                    DisplayUserInfo(provider, indentLevel + 2);
                }
            }
        }

        loggedProviderIds.Clear();
    }

    private void DisplayUserInfo(IUserInfo userInfo, int indentLevel)
    {
        string indent = new(' ', indentLevel * 2);
        var userProperties = new Dictionary<string, string>
        {
            {"Display Name", userInfo.DisplayName},
            {"Email", userInfo.Email},
            {"Photo URL", userInfo.PhotoUrl != null ? userInfo.PhotoUrl.ToString() : null},
            {"Provider ID", userInfo.ProviderId},
            {"User ID", userInfo.UserId}
        };

        foreach (var property in userProperties)
        {
            if (!string.IsNullOrEmpty(property.Value))
            {
                Debug.Log(string.Format("[P][FirebaseService] {0}{1}: {2}", indent, property.Key, property.Value));
            }
        }
    }

    // public async Task LinkAnonymousToGoogle(string idToken)
    // {
    //     if (auth.CurrentUser == null || !auth.CurrentUser.IsAnonymous)
    //     {
    //         Debug.LogError("[AuthService] Текущий пользователь не анонимный, линк невозможен.");
    //         NotificationManager.ShowNotification("Привязка доступна только для анонимных аккаунтов.");
    //         return;
    //     }

    //     try
    //     {
    //         Credential credential = GoogleAuthProvider.GetCredential(idToken, null);
    //         AuthResult result = await auth.CurrentUser.LinkWithCredentialAsync(credential);
    //         FirebaseUser linkedUser = result.User;

    //         await linkedUser.ReloadAsync();

    //         bool exists = await FirebaseFirestoreService.Instance.UserDocumentExists(linkedUser.UserId);
    //         if (exists)
    //         {
    //             Debug.LogError("[AuthService] У Google-аккаунта уже есть запись, линк запрещён.");
    //             NotificationManager.ShowNotification("Этот Google-аккаунт уже зарегистрирован. Привязка невозможна."); return;
    //         }

    //         Debug.Log($"[AuthService] Анонимный аккаунт {auth.CurrentUser.UserId} привязан к Google {linkedUser.UserId}");

    //         await FirebaseFirestoreService.Instance.CreateOrUpdateUserDocument(linkedUser);
    //         NotificationManager.ShowNotification("Аккаунт успешно привязан к Google.");
    //     }
    //     catch (Exception e)
    //     {
    //         Debug.LogError($"[AuthService] Ошибка линка анонимного аккаунта: {e.Message}");
    //     }
    // }

    public void Dispose()
    {
        if (auth != null)
        {
            auth.StateChanged -= async (s, e) => await AuthStateChanged(s, e);
            auth.IdTokenChanged -= IdTokenChanged;
            auth = null;
        }

        _instance = null;
    }
}