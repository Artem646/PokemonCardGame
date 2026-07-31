using Firebase.Auth;
using System;
using System.Threading.Tasks;
using UnityEngine;

public class EmailSignInController
{
    private readonly FirebaseAuth auth;

    public EmailSignInController()
    {
        auth = FirebaseAuthService.Instance.GetAuth();
    }

    public Task<AuthResult> AuthenticateAsync(string email, string password, bool isLogin)
    {
        if (isLogin) return auth.SignInWithEmailAndPasswordAsync(email, password);
        else return auth.CreateUserWithEmailAndPasswordAsync(email, password);
    }

    public void SignOut()
    {
        try
        {
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
