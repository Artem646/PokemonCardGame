using System;
using System.Threading.Tasks;
using Firebase.Auth;
using UnityEngine;

public class GoogleProvider : IAuthProvider
{
    private readonly GoogleSignInController googleController;
    private readonly FirebaseSignInController firebaseController;
    private readonly string webClientId = ConfigLoader.GetWebClientId();

    public GoogleProvider()
    {
        googleController = new GoogleSignInController(webClientId);
        firebaseController = new FirebaseSignInController();
        CheckExistingAuthSession();
        Debug.Log("[P][GoogleProvider] GoogleProvider успешно создан.");
    }

    private void CheckExistingAuthSession()
    {
        if (firebaseController.IsSignedIn)
        {
            SignInSilently();
        }
        else
        {
            Debug.Log("[P][GoogleProvider] Сессия не найдена, требуется вход через Google.");
        }
    }

    public void SignIn()
    {
        try
        {
            googleController.Initialize();
            Task<Google.GoogleSignInUser> googleSignInTask = googleController.SignIn();
            googleSignInTask.ContinueWith(taskResult =>
            {
                AuthResponseHandler.HandleGoogleResult(taskResult, OnGoogleSuccess);
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }
        catch (Exception ex)
        {
            Localizer.LocalizeNotification(NotificationKey.SingOutError, NotificationType.Error, ex.Message);
            Debug.Log($"[P][GoogleProvider] Критическая ошибка в методе SignIn: {ex.Message}");
        }
    }

    public void SignInSilently()
    {
        try
        {
            googleController.Initialize(true);
            var googleSignInTask = googleController.SignInSilently();
            googleSignInTask.ContinueWith(task =>
            {
                AuthResponseHandler.HandleGoogleResult(task, OnGoogleSuccess);
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }
        catch (Exception ex)
        {
            Localizer.LocalizeNotification(NotificationKey.SingOutError, NotificationType.Error, ex.Message);
            Debug.Log($"[P][GoogleProvider] Критическая ошибка в методе SignInSilently: {ex.Message}");

        }
    }

    private void OnGoogleSuccess(string idToken)
    {
        try
        {
            Task<FirebaseUser> firebaseAuthTask = firebaseController.SignInWithGoogleAsync(idToken);
            firebaseAuthTask.ContinueWith(taskResult =>
            {
                AuthResponseHandler.HandleGoogleFirebaseResult(taskResult, OnFirebaseSuccess);
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }
        catch (Exception e)
        {
            Localizer.LocalizeNotification(NotificationKey.FirebaseAuthError, NotificationType.Error);
            Debug.Log($"[P][GoogleProvider] Ошибка при запуске аутентификации Firebase: {e.Message}");
        }
    }

    private void OnFirebaseSuccess(FirebaseUser user)
    {
        Localizer.LocalizeNotification(NotificationKey.GoogleSingInSuccess, NotificationType.Success, user.DisplayName);
        Debug.Log($"[P][GoogleProvider] Вход выполнен: {user.Email}");
    }

    public void SignOut()
    {
        googleController.SignOut();
        firebaseController.SignOut();
    }

    public bool IsSignedIn() => firebaseController.IsSignedIn;
    public string GetUserId() => firebaseController.CurrentUser?.UserId;
}
