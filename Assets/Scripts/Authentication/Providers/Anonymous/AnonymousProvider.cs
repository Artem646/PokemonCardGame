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
            Localizer.LocalizeNotification(NotificationKey.SingOutError, NotificationType.Error, ex.Message);
            Debug.Log($"[P][AnonymousProvider] Критическая ошибка в методе SignIn: {ex.Message}");
        }
    }

    private void OnAnonymousSuccess(AuthResult result)
    {
        Localizer.LocalizeNotification(NotificationKey.AnonymousSingInSuccess, NotificationType.Success);
    }

    public void SignOut() => anonymousController.SignOut();

    public bool IsSignedIn() => anonymousController.IsSignedIn;
    public string GetUserId() => anonymousController.CurrentUser?.UserId;
}
