using System;
using System.Collections.Generic;
using Firebase.Extensions;
using Firebase.Auth;
using UnityEngine;
using UnityEngine.SceneManagement;
using Firebase.Firestore;

public class FirebaseService
{
    private static FirebaseService _instance;
    public static FirebaseService Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new FirebaseService();
            }
            return _instance;
        }
    }

    private FirebaseAuth auth;
    private FirebaseFirestore firestore;

    public FirebaseAuth GetAuth() => auth;
    public FirebaseFirestore GetFirestore() => firestore;

    private Dictionary<string, FirebaseUser> userByAuth = new();
    private readonly bool fetchingToken = false;
    private bool isProcessingStateChange = false;
    private bool isAuthInitialized = false;
    private bool isFirestoreInitialized = false;
    private string lastToken = null;
    private HashSet<string> loggedProviderIds = new HashSet<string>();

    private FirebaseService() { }

    public void InitializeFirebaseAuth()
    {
        if (isAuthInitialized) return;

        auth = FirebaseAuth.DefaultInstance;
        if (auth == null)
        {
            Debug.LogError("[P][FirebaseService] Ошибка: Firebase Auth не проинициализирован.");
            return;
        }

        auth.StateChanged += AuthStateChanged;
        auth.IdTokenChanged += IdTokenChanged;
        AuthStateChanged(this, null);

        isAuthInitialized = true;

        Debug.Log("[P][FirebaseService] Firebase Auth успешно инициализирован.");
    }

    public void InitializeFirebaseFirestore()
    {
        if (isFirestoreInitialized) return;

        firestore = FirebaseFirestore.DefaultInstance;
        if (firestore == null)
        {
            Debug.LogError("[P][FirebaseService] Ошибка: Firebase Firestore не проинициализирован.");
            return;
        }

        isFirestoreInitialized = true;

        Debug.Log("[P][FirebaseService] Firebase Firestore успешно инициализирован.");
    }

    private void AuthStateChanged(object sender, EventArgs eventArgs)
    {
        if (isProcessingStateChange) return;

        isProcessingStateChange = true;
        FirebaseAuth senderAuth = sender as FirebaseAuth;
        FirebaseUser user = null;

        if (senderAuth != null) userByAuth.TryGetValue(senderAuth.App.Name, out user);
        if (senderAuth == auth && senderAuth.CurrentUser != user)
        {
            bool signedIn = user != senderAuth.CurrentUser && senderAuth.CurrentUser != null && auth.CurrentUser.IsValid();
            if (!signedIn && user != null)
            {
                Debug.Log("[P][FirebaseService] Signed out " + user.UserId);
                SceneManager.LoadScene("SignInScene");
            }

            user = senderAuth.CurrentUser;
            userByAuth[senderAuth.App.Name] = user;

            if (signedIn)
            {
                Debug.Log("[P][FirebaseService] Signed in " + user.UserId);
                DisplayDetailedUserInfo(user, 1);

                CreateUserDocumentInFirestore(user.UserId);

                SceneManager.LoadScene("UploadingScene");
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
        if (senderAuth == auth && senderAuth.CurrentUser != null && !fetchingToken)
        {
            senderAuth.CurrentUser.TokenAsync(false).ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError("[P][FirebaseService] Token retrieval failed: " + task.Exception);
                }
                else if (task.Result != lastToken)
                {
                    lastToken = task.Result;
                    Debug.Log($"[P][FirebaseService] Token[0:8] = {task.Result.Substring(0, 8)}");
                }
            });
        }
    }

    private void DisplayDetailedUserInfo(FirebaseUser user, int indentLevel)
    {
        string indent = new string(' ', indentLevel * 2);

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
        string indent = new string(' ', indentLevel * 2);
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

    private async void CreateUserDocumentInFirestore(string userId)
    {
        DocumentReference userDocument = firestore.Collection("users").Document(userId);
        DocumentSnapshot snapshot = await userDocument.GetSnapshotAsync();
        if (!snapshot.Exists)
        {
            Debug.Log($"[P][FirebaseService] Документ пользователя {userId} создан.");

            Dictionary<string, object> userData = new()
            {
                { "cardsInCollection", new List<string>() }
            };

            // Dictionary<string, object> userData = new()
            // {
            //     { "uid", user.UserId },
            //     { "email", user.Email ?? "" },
            //     { "displayName", user.DisplayName ?? "" },
            //     { "authType", authType },
            //     { "createdAt", Timestamp.GetCurrentTimestamp() }
            // };

            await userDocument.SetAsync(userData);
        }
        else
        {
            Debug.Log($"[P][FirebaseService] Документ пользователя {userId} уже существует");
        }
    }
    public void Dispose()
    {
        if (auth != null)
        {
            auth.StateChanged -= AuthStateChanged;
            auth.IdTokenChanged -= IdTokenChanged;
            auth = null;
        }

        if (firestore != null)
        {
            firestore = null;
        }

        _instance = null;
    }
}