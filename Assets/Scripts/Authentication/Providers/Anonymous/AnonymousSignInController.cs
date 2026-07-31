using Firebase.Auth;
using System;
using System.Threading.Tasks;
using UnityEngine;

public class AnonymousSignInController
{
    private readonly FirebaseAuth auth;

    public AnonymousSignInController()
    {
        auth = FirebaseAuthService.Instance.GetAuth();
    }

    public Task<AuthResult> SignInWithAnonymousAsync()
    {
        return auth.SignInAnonymouslyAsync();
    }

    public void SignOut()
    {
        try
        {
            auth.CurrentUser.DeleteAsync();
            auth.SignOut();
        }
        catch (Exception ex)
        {
            Debug.Log($"[P][AnonymousProvider.cs] Ошибка при выходе: {ex.Message}");
        }
    }

    public bool IsSignedIn => auth?.CurrentUser != null;
    public FirebaseUser CurrentUser => auth?.CurrentUser;
}
