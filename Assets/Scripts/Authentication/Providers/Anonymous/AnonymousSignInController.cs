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
            auth.CurrentUser.DeleteAsync();
            auth.SignOut();
            Debug.Log("[P]AnonymousAuthService] Анонимный пользователь вышел из Firebase и удалил аккаунт.");
        }
        catch (Exception ex)
        {
            Debug.Log($"[P][AnonymousProvider.cs] Ошибка при выходе: {ex.Message}");
        }
    }

    public bool IsSignedIn => auth?.CurrentUser != null;
    public FirebaseUser CurrentUser => auth?.CurrentUser;
}
