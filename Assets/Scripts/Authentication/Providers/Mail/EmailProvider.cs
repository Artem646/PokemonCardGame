using System;
using System.Threading.Tasks;
using Firebase.Auth;
using UnityEngine;

public class EmailProvider : IAuthProvider
{
    private readonly EmailSignInController emailController;

    public string Email { get; set; }
    public string Password { get; set; }

    public Action<AuthErrorTarget, string> OnErrorAction { get; set; }

    public EmailProvider()
    {
        emailController = new EmailSignInController();
    }

    public void SignIn() => AuthenticateWithCredentials(Email, Password, true);
    public void SignUp() => AuthenticateWithCredentials(Email, Password, false);

    private void AuthenticateWithCredentials(string email, string password, bool isLogin)
    {
        try
        {
            Task<AuthResult> emailAuthTask = emailController.AuthenticateAsync(email, password, isLogin);
            emailAuthTask.ContinueWith(taskResult =>
            {
                AuthResponseHandler.HandleEmailFirebaseResult(taskResult,
                async (authResult) =>
                {
                    FirebaseUser user = authResult.User;
                    if (!isLogin)
                    {
                        try
                        {
                            await user.SendEmailVerificationAsync();
                            emailController.SignOut();
                            OnErrorAction(AuthErrorTarget.General, "Успешно! На почту отправлено письмо для подтверждения.");
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"[EmailProvider] Ошибка отправки письма: {ex.Message}");
                            OnErrorAction?.Invoke(AuthErrorTarget.General, "Ошибка при отправке письма подтверждения.");
                        }
                    }
                    else
                    {
                        if (!user.IsEmailVerified)
                        {
                            emailController.SignOut();
                            OnErrorAction(AuthErrorTarget.General, "Почта не подтверждена. Проверьте папку 'Входящие' или 'Спам'!");
                        }
                        else
                            OnEmailSuccess();
                    }
                }, OnErrorAction);
            },
            TaskScheduler.FromCurrentSynchronizationContext());
        }
        catch (Exception ex)
        {
            Debug.Log($"[P][EmailProvider] Критическая ошибка: {ex.Message}");
            OnErrorAction?.Invoke(AuthErrorTarget.General, "Критическая ошибка аутентификации!");
        }
    }

    private void OnEmailSuccess()
    {
        Localizer.LocalizeNotification(NotificationKey.EmailAuthSuccess, NotificationType.Success);
    }

    public void SignOut() => emailController.SignOut();

    public bool IsSignedIn() => emailController.IsSignedIn;
    public string GetUserId() => emailController.CurrentUser?.UserId;
}
