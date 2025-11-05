using Google;
using System;
using System.Threading.Tasks;
using UnityEngine;

public class GoogleSignInController
{
    private readonly GoogleSignInConfiguration config;
    private bool isInitialized = false;

    public GoogleSignInController(string webClientId)
    {
        config = new GoogleSignInConfiguration
        {
            WebClientId = webClientId,
            RequestIdToken = true,
            RequestEmail = true
        };
        Debug.Log("[P][GoogleSignInController] GoogleSignInController успешно создан.");
    }

    public void Initialize(bool silent = false)
    {
        if (isInitialized) return;

        if (GoogleSignIn.Configuration == null)
        {
            GoogleSignIn.Configuration = config;
            GoogleSignIn.Configuration.UseGameSignIn = false;
            GoogleSignIn.Configuration.RequestIdToken = true;
            if (!silent)
            {
                GoogleSignIn.Configuration.RequestEmail = true;
            }
        }

        isInitialized = true;

        Debug.Log("[P][GoogleSignInController] Google Sign-In успешна инициализирована.");
    }

    public Task<GoogleSignInUser> SignIn() => GoogleSignIn.DefaultInstance.SignIn();

    public Task<GoogleSignInUser> SignInSilently() => GoogleSignIn.DefaultInstance.SignInSilently();

    public void SignOut()
    {
        try
        {
            GoogleSignIn.DefaultInstance.SignOut();
            isInitialized = false;
            GoogleSignIn.Configuration = null;
            Debug.Log("[P][GoogleSignInController] Пользователь вышел из Google аккаунта.");
        }
        catch (Exception e)
        {
            Debug.Log($"[P][GoogleSignInController] Ошибка при выходе: {e.Message}");
        }
    }
}
