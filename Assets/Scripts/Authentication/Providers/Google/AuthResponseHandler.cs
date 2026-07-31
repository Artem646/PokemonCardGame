using System;
using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using Google;
using UnityEngine;

public enum AuthErrorTarget
{
    Email,
    Password,
    General
}


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
                        Localizer.LocalizeNotification(NotificationKey.GoogleCanceled, NotificationType.Error);

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
                    Debug.Log($"[P][GoogleProvider] Auth Inner Exception: {innerException.Message}");
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

    public static void HandleAnonymousFirebaseResult(Task<AuthResult> taskResult, Action onSuccess)
    {
        if (taskResult.IsCanceled)
            Localizer.LocalizeNotification(NotificationKey.AnonymousAuthCanceled, NotificationType.Error);
        else if (taskResult.IsFaulted)
        {
            Debug.Log("[P][AnonymousProvider] Ошибка Firebase аутентификации");
            Localizer.LocalizeNotification(NotificationKey.AnonymousAuthError, NotificationType.Error);

            if (taskResult.Exception != null)
            {
                foreach (var innerException in taskResult.Exception.InnerExceptions)
                    Debug.Log($"[P][AnonymousProvider] Auth Inner Exception: {innerException.Message}");
            }
        }
        else if (taskResult.IsCompletedSuccessfully)
        {
            Localizer.LocalizeNotification(NotificationKey.AnonymousAuthSuccess, NotificationType.Success);
            onSuccess();
        }
    }

    public static void HandleEmailFirebaseResult(Task<AuthResult> taskResult, Action<AuthResult> onSuccess, Action<AuthErrorTarget, string> onError)
    {
        if (taskResult.IsCanceled)
        {
            onError(AuthErrorTarget.General, Localizer.GetNotificationText(NotificationKey.EmailAuthCanceled));
            return;
        }
        else if (taskResult.IsFaulted)
        {
            onError(AuthErrorTarget.General, Localizer.GetNotificationText(NotificationKey.EmailAuthError));

            if (taskResult.Exception != null)
            {
                foreach (var innerException in taskResult.Exception.Flatten().InnerExceptions)
                {
                    if (innerException is FirebaseException firebaseEx)
                    {
                        AuthError errorCode = (AuthError)firebaseEx.ErrorCode;
                        Debug.LogError($"[P][AuthResponseHandler] Firebase Auth Error: {errorCode}");

                        var (target, messageKey) = GetEmailErrorDetailsByCode(errorCode);
                        onError(target, Localizer.GetNotificationText(messageKey));
                        return;
                    }
                    else
                    {
                        onError(AuthErrorTarget.General, Localizer.GetNotificationText(NotificationKey.EmailNetworkRequestFailed));
                        Debug.LogError($"[P][AuthResponseHandler] System Error: {innerException.Message}");
                    }
                }
            }
        }
        else if (taskResult.IsCompletedSuccessfully)
            onSuccess(taskResult.Result);
    }

    private static (AuthErrorTarget target, NotificationKey messageKey) GetEmailErrorDetailsByCode(AuthError errorCode)
    {
        return errorCode switch
        {
            AuthError.MissingEmail => (AuthErrorTarget.Email, NotificationKey.MissingEmail),
            AuthError.InvalidEmail => (AuthErrorTarget.Email, NotificationKey.InvalidEmail),
            AuthError.UserNotFound => (AuthErrorTarget.Email, NotificationKey.EmailUserNotFound),
            AuthError.EmailAlreadyInUse => (AuthErrorTarget.Email, NotificationKey.EmailAlreadyInUse),
            AuthError.UserDisabled => (AuthErrorTarget.Email, NotificationKey.EmailUserDisabled),

            AuthError.MissingPassword => (AuthErrorTarget.Password, NotificationKey.EmailMissingPassword),
            AuthError.WrongPassword => (AuthErrorTarget.Password, NotificationKey.EmailWrongPassword),
            AuthError.WeakPassword => (AuthErrorTarget.Password, NotificationKey.EmailWeakPassword),

            AuthError.NetworkRequestFailed => (AuthErrorTarget.General, NotificationKey.EmailNetworkRequestFailed),
            AuthError.TooManyRequests => (AuthErrorTarget.General, NotificationKey.EmailTooManyRequests),
            AuthError.OperationNotAllowed => (AuthErrorTarget.General, NotificationKey.EmailOperationNotAllowed),
            AuthError.InvalidCredential => (AuthErrorTarget.General, NotificationKey.EmailInvalidCredential),
            AuthError.AccountExistsWithDifferentCredentials => (AuthErrorTarget.General, NotificationKey.EmailAccountExistsWithDifferentCredentials),

            _ => (AuthErrorTarget.General, NotificationKey.EmailUnknownError)
        };
    }
}