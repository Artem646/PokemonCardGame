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
        catch (Exception e)
        {
            ShowError("Ошибка при запуске входа: " + e.Message);
            Debug.Log($"[P][GoogleProvider] Критическая ошибка в методе SignIn: {e.Message}");
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
        catch (Exception e)
        {
            ShowError("Ошибка при запуске входа: " + e.Message);
            Debug.Log($"[P][GoogleProvider] Критическая ошибка в методе SignInSilently: {e.Message}");

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
            ShowError("Ошибка подключения к серверу");
            Debug.Log($"[P][GoogleProvider] Ошибка при запуске аутентификации Firebase: {e.Message}");
        }
    }

    private void OnFirebaseSuccess(FirebaseUser user)
    {
        ShowSuccess($"Добро пожаловать, {user.DisplayName}!");
        Debug.Log($"[P][GoogleProvider] Вход выполнен: {user.Email}");
    }

    public void SignOut()
    {
        googleController.SignOut();
        firebaseController.SignOut();
    }

    private void ShowError(string message) => NotificationManager.ShowNotification(message, true);
    private void ShowSuccess(string message) => NotificationManager.ShowNotification(message, false);

    public void SignUp() { }

    public bool IsSignedIn() => firebaseController.IsSignedIn;
    public string GetUserId() => firebaseController.CurrentUser?.UserId;
}
