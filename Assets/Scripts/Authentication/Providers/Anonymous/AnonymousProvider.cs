using System;
using System.Threading.Tasks;
using Firebase.Auth;
using UnityEngine;

public class AnonymousProvider : IAuthProvider
{
    private readonly AnonymousSignInController anonymousController;

    public AnonymousProvider()
    {
        anonymousController = new AnonymousSignInController();
    }

    public void SignIn()
    {
        try
        {
            Task<AuthResult> anonymousSignInTask = anonymousController.SignInWithAnonymousAsync();
            anonymousSignInTask.ContinueWith(taskResult =>
            {
                AuthResponseHandler.HandleAnonymousFirebaseResult(taskResult, OnAnonymousSuccess);
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }
        catch (Exception ex)
        {
            Localizer.LocalizeNotification(NotificationKey.SingOutError, NotificationType.Error, ex.Message);
            Debug.Log($"[P][AnonymousProvider] Критическая ошибка в методе SignIn: {ex.Message}");
        }
    }

    private void OnAnonymousSuccess()
    {
        Localizer.LocalizeNotification(NotificationKey.AnonymousSingInSuccess, NotificationType.Success);
    }

    public void SignOut() => anonymousController.SignOut();

    public bool IsSignedIn() => anonymousController.IsSignedIn;
    public string GetUserId() => anonymousController.CurrentUser?.UserId;
}
