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
            ShowError("Не удалось начать вход через Google");
            return;
        }

        if (taskResult.IsFaulted)
        {
            Debug.Log("[P][GoogleProvider] ❌ ОШИБКА АУТЕНТИФИКАЦИИ");

            if (taskResult.Exception != null)
            {
                foreach (var exception in taskResult.Exception.InnerExceptions)
                {
                    if (exception is GoogleSignIn.SignInException googleSignInException)
                    {
                        Debug.Log($"[P][GoogleProvider] 🔸 Google Error: {googleSignInException.Status}");
                        Debug.Log($"[P][GoogleProvider] 🔸 Exception: {exception.GetType().Name}");
                        Debug.Log($"[P][GoogleProvider] 🔸 Message: {googleSignInException.Message}");

                        string errorMessage = GetGoogleErrorMessage(googleSignInException.Status);
                        Debug.LogError("[P][GoogleProvider] " + errorMessage);
                        ShowError(errorMessage);
                    }
                    else if (exception is OperationCanceledException)
                    {
                        Debug.Log("[P][GoogleProvider] OperationCanceledException (Вход отменён в диалоговом окне)");
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
                    ShowError("Ошибка получения токена Google");
                    Debug.Log("[P][GoogleProvider] Ошибка получения токена Google");
                    return;
                }

                Debug.Log("[P][GoogleProvider] ✅ УСПЕШНАЯ АУТЕНТИФИКАЦИЯ GOOGLE");
                Debug.Log("[P][GoogleProvider] 👤 ОСНОВНАЯ ИНФОРМАЦИЯ:");
                Debug.Log("[P][GoogleProvider] Welcome Google User: " + googleUser.DisplayName);
                Debug.Log("[P][GoogleProvider] Gmail: " + googleUser.Email);
                Debug.Log("[P][GoogleProvider] Google ID: " + googleUser.UserId);
                Debug.Log("[P][GoogleProvider] Имя: " + googleUser.GivenName);
                Debug.Log("[P][GoogleProvider] Фамилия: " + googleUser.FamilyName);

                Debug.Log("[P][GoogleProvider] 📊 ПРОВЕРКА ДАННЫХ:");
                Debug.Log("[P][GoogleProvider] Email: " + (string.IsNullOrEmpty(googleUser.Email) ? "❌" : "✅"));
                Debug.Log("[P][GoogleProvider] IdToken: " + (string.IsNullOrEmpty(googleUser.IdToken) ? "❌" : "✅"));
                Debug.Log("[P][GoogleProvider] Аватар: " + (googleUser.ImageUrl == null ? "❌" : "✅"));
                Debug.Log("[P][GoogleProvider] Платформа: " + Application.platform);

                onSuccess(googleUser.IdToken);
            }
            catch (Exception e)
            {
                ShowError("Ошибка при обработке данных Google");
                Debug.Log($"[P][GoogleProvider] Ошибка при обработке результата Google: {e.Message}");
            }
        }
    }

    private static string GetGoogleErrorMessage(GoogleSignInStatusCode status)
    {
        return status switch
        {
            GoogleSignInStatusCode.NetworkError => "Ошибка сети. Проверьте подключение к интернету",
            GoogleSignInStatusCode.InternalError => "Внутренняя ошибка сервиса Google",
            GoogleSignInStatusCode.ApiNotConnected => "Сервис Google недоступен",
            GoogleSignInStatusCode.InvalidAccount => "Неверный аккаунт Google",
            GoogleSignInStatusCode.Timeout => "Время ожидания истекло",
            GoogleSignInStatusCode.DeveloperError => "Ошибка настройки приложения",
            GoogleSignInStatusCode.Canceled => "Вход отменён пользователем",
            GoogleSignInStatusCode.Interrupted => "Вход отменён внешним событием",
            GoogleSignInStatusCode.Error => "Вход отменён пользователем",
            _ => "Ошибка входа через Google"
        };
    }

    public static void HandleGoogleFirebaseResult(Task<FirebaseUser> taskResult, Action<FirebaseUser> onSuccess)
    {
        if (taskResult.IsFaulted)
        {
            Debug.Log("[P][GoogleProvider] Ошибка Firebase аутентификации");
            ShowError("Ошибка подключения к серверу firebaseAuth");

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

                Debug.Log("[P][GoogleProvider] Firebase аутентификация успешна прошла!");
                Debug.Log($"[P][GoogleProvider] Firebase User: {firebaseUser.DisplayName}");
                Debug.Log($"[P][GoogleProvider] Firebase Email: {firebaseUser.Email}");
                Debug.Log($"[P][GoogleProvider] Firebase UserId: {firebaseUser.UserId}");

                onSuccess(firebaseUser);
            }
            catch (Exception e)
            {
                ShowError("Ошибка обработки данных пользователя");
                Debug.Log($"[P][GoogleProvider] Ошибка при обработке Firebase пользователя: {e.Message}");
            }
        }
    }

    public static void HandleAnonymousFirebaseResult(Task<AuthResult> taskResult, Action<AuthResult> onSuccess)
    {
        if (taskResult.IsCanceled)
        {
            Debug.Log("[P][AnonymousProvider] Firebase аутентификация отменена");
        }
        else if (taskResult.IsFaulted)
        {
            Debug.Log("[P][AnonymousProvider] Ошибка Firebase аутентификации");
            ShowError("Ошибка подключения к серверу firebaseAuth");

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

                Debug.Log("[P][AnonymousProvider] Firebase аутентификация успешна!");
                Debug.Log($"[P][AnonymousProvider] Firebase UserId: {firebaseUser.UserId}");

                onSuccess(taskResult.Result);
            }
            catch (Exception ex)
            {
                ShowError("Ошибка обработки данных пользователя");
                Debug.Log($"[P][AnonymousProvider] Ошибка при обработке Firebase пользователя: {ex.Message}");
            }
        }
    }

    private static void ShowError(string message)
    {
        NotificationManager.ShowNotification(message, NotificationType.Error);
    }
}
