using Firebase.Auth;
using System;
using System.Threading.Tasks;
using UnityEngine;

public class AnonymousSingInController
{
    private readonly FirebaseAuth auth;

    public AnonymousSingInController()
    {
        auth = FirebaseAuthService.Instance.GetAuth();
        Debug.Log("[P][AnonymousSingInController] AnonymousSingInController успешно создан.");
    }

    public Task<AuthResult> SignInWithAnonymousAsync()
    {
        return auth.SignInAnonymouslyAsync();
    }

    public void SignOut()
    {
        try
        {
            auth.SignOut();
            Debug.Log("[P]AnonymousAuthService] Пользователь вышел из Firebase.");
        }
        catch (Exception ex)
        {
            Debug.Log($"[P][AnonymousProvider.cs] Ошибка при выходе: {ex.Message}");
        }
    }

    public bool IsSignedIn => auth?.CurrentUser != null;
    public FirebaseUser CurrentUser => auth?.CurrentUser;
}
