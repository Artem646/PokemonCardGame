using System;
using System.Threading.Tasks;
using Firebase.Auth;
using UnityEngine;

public class AnonymousProvider : IAuthProvider
{
    private readonly AnonymousSingInController anonymousController;

    public AnonymousProvider()
    {
        anonymousController = new AnonymousSingInController();
        Debug.Log("[P][AnonymousProvider] AnonymousProvider успешно создан.");
    }

    public void SignIn()
    {
        try
        {
            var anonymousSignInTask = anonymousController.SignInWithAnonymousAsync();
            anonymousSignInTask.ContinueWith(taskResult =>
            {
                AuthResponseHandler.HandleAnonymousFirebaseResult(taskResult, OnAnonymousSuccess);
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }
        catch (Exception ex)
        {
            ShowError("Ошибка при запуске входа: " + ex.Message);
            Debug.Log($"[P][AnonymousProvider] Критическая ошибка в методе SignIn: {ex.Message}");
        }
    }

    private void OnAnonymousSuccess(AuthResult result)
    {
        FirebaseUser user = result.User;
        ShowSuccess($"Добро пожаловать, {user.DisplayName}!");
    }

    public void SignOut() => anonymousController.SignOut();

    private void ShowError(string message) => NotificationManager.ShowNotification(message, true);
    private void ShowSuccess(string message) => NotificationManager.ShowNotification(message, false);

    public void SignUp() { }

    public bool IsSignedIn() => anonymousController.IsSignedIn;
    public string GetUserId() => anonymousController.CurrentUser?.UserId;
}
