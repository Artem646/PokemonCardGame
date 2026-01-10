using System;
using System.Threading.Tasks;
using Firebase.Auth;
using Google;
using UnityEngine;

public static class AuthResponseHandler
{
    public static void HandleGoogleResult(Task<GoogleSignInUser> taskResult, Action<string> onSuccess)
    {
        if (taskResult == null)
        {
            Debug.LogError("[P][GoogleProvider] Task is null");
            Localizer.LocalizeNotification(NotificationKey.GoogleSingOutFailed, NotificationType.Error);
            return;
        }

        if (taskResult.IsFaulted)
        {
            Debug.Log("[P][GoogleProvider] ОШИБКА АУТЕНТИФИКАЦИИ");

            if (taskResult.Exception != null)
            {
                foreach (var exception in taskResult.Exception.InnerExceptions)
                {
                    if (exception is GoogleSignIn.SignInException googleSignInException)
                    {
                        Debug.Log($"[P][GoogleProvider] 🔸 Google Error: {googleSignInException.Status}");
                        Debug.Log($"[P][GoogleProvider] 🔸 Exception: {exception.GetType().Name}");
                        Debug.Log($"[P][GoogleProvider] 🔸 Message: {googleSignInException.Message}");

                        NotificationKey key = GetGoogleErrorMessageKeyByStatus(googleSignInException.Status);
                        Localizer.LocalizeNotification(key, NotificationType.Error);
                    }
                    else if (exception is OperationCanceledException)
                    {
                        Localizer.LocalizeNotification(NotificationKey.GoogleCanceled, NotificationType.Error);
                    }

                    break;
                }
            }
        }
        else if (taskResult.IsCompletedSuccessfully)
        {
            try
            {
                GoogleSignInUser googleUser = taskResult.Result;

                if (string.IsNullOrEmpty(googleUser.IdToken))
                {
                    Localizer.LocalizeNotification(NotificationKey.GoogleTokenError, NotificationType.Error);
                    Debug.Log("[P][GoogleProvider] Ошибка получения токена Google");
                    return;
                }

                onSuccess(googleUser.IdToken);
            }
            catch (Exception e)
            {
                Localizer.LocalizeNotification(NotificationKey.GoogleDataProcessingError, NotificationType.Error);
                Debug.Log($"[P][GoogleProvider] Ошибка при обработке результата Google: {e.Message}");
            }
        }
    }

    private static NotificationKey GetGoogleErrorMessageKeyByStatus(GoogleSignInStatusCode status)
    {
        return status switch
        {
            GoogleSignInStatusCode.NetworkError => NotificationKey.GoogleNetworkError,
            GoogleSignInStatusCode.InternalError => NotificationKey.GoogleInternalError,
            GoogleSignInStatusCode.ApiNotConnected => NotificationKey.GoogleApiNotConnected,
            GoogleSignInStatusCode.InvalidAccount => NotificationKey.GoogleInvalidAccount,
            GoogleSignInStatusCode.Timeout => NotificationKey.GoogleTimeout,
            GoogleSignInStatusCode.DeveloperError => NotificationKey.GoogleDeveloperError,
            GoogleSignInStatusCode.Canceled => NotificationKey.GoogleCanceled,
            GoogleSignInStatusCode.Interrupted => NotificationKey.GoogleInterrupted,
            GoogleSignInStatusCode.Error => NotificationKey.GoogleError,
            _ => NotificationKey.GoogleUnknown
        };
    }

    public static void HandleGoogleFirebaseResult(Task<FirebaseUser> taskResult, Action<FirebaseUser> onSuccess)
    {
        if (taskResult.IsFaulted)
        {
            Debug.Log("[P][GoogleProvider] Ошибка Firebase аутентификации");
            Localizer.LocalizeNotification(NotificationKey.FirebaseAuthError, NotificationType.Error);

            if (taskResult.Exception != null)
            {
                foreach (var innerException in taskResult.Exception.InnerExceptions)
                {
                    Debug.Log($"[P][GoogleProvider] Auth Inner Exception: {innerException.Message}");
                }
            }
        }
        else if (taskResult.IsCompletedSuccessfully)
        {
            try
            {
                FirebaseUser firebaseUser = taskResult.Result;
                Localizer.LocalizeNotification(NotificationKey.FirebaseAuthSuccess, NotificationType.Success);
                onSuccess(firebaseUser);
            }
            catch (Exception e)
            {
                Localizer.LocalizeNotification(NotificationKey.FirebaseUserDataError, NotificationType.Error);
                Debug.Log($"[P][GoogleProvider] Ошибка при обработке Firebase пользователя: {e.Message}");
            }
        }
    }

    public static void HandleAnonymousFirebaseResult(Task<AuthResult> taskResult, Action<AuthResult> onSuccess)
    {
        if (taskResult.IsCanceled)
        {
            Localizer.LocalizeNotification(NotificationKey.AnonymousAuthCanceled, NotificationType.Error);
        }
        else if (taskResult.IsFaulted)
        {
            Debug.Log("[P][AnonymousProvider] Ошибка Firebase аутентификации");
            Localizer.LocalizeNotification(NotificationKey.AnonymousAuthError, NotificationType.Error);

            if (taskResult.Exception != null)
            {
                foreach (var innerException in taskResult.Exception.InnerExceptions)
                {
                    Debug.Log($"[P][AnonymousProvider] Auth Inner Exception: {innerException.Message}");
                }
            }
        }
        else if (taskResult.IsCompletedSuccessfully)
        {
            try
            {
                FirebaseUser firebaseUser = taskResult.Result.User;
                Localizer.LocalizeNotification(NotificationKey.AnonymousAuthSuccess, NotificationType.Success);
                onSuccess(taskResult.Result);
            }
            catch (Exception ex)
            {
                Localizer.LocalizeNotification(NotificationKey.AnonymousUserDataError, NotificationType.Error);
                Debug.Log($"[P][AnonymousProvider] Ошибка при обработке Firebase пользователя: {ex.Message}");
            }
        }
    }
}
