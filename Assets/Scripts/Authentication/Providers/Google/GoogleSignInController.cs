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
        }
        catch (Exception e)
        {
            Debug.Log($"[P][GoogleSignInController] Ошибка при выходе: {e.Message}");
        }
    }
}
