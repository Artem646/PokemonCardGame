using UnityEngine;
using UnityEngine.UIElements;
using System;

public class EmailSignInDialogController : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;

    private VisualElement root;
    private VisualElement overlay;
    private TextField emailTextField;
    private TextField passwordTextField;
    private Label emailErrorLabel;
    private Label passwordErrorLabel;

    private Button loginButton;
    private Button registerButton;
    private Button closeDialogButton;

    private void Start()
    {
        InitializeUI();
        RegisterCallbacks();
    }

    private void InitializeUI()
    {
        root = uiDocument.rootVisualElement;
        overlay = root.Q<VisualElement>("overlay");
        emailTextField = root.Q<TextField>("emailTextField");
        passwordTextField = root.Q<TextField>("passwordTextField");
        emailErrorLabel = root.Q<Label>("emailErrorLabel");
        passwordErrorLabel = root.Q<Label>("passwordErrorLabel");

        loginButton = root.Q<Button>("signInButton");
        registerButton = root.Q<Button>("registerButton");
        closeDialogButton = root.Q<Button>("closeDialogButton");
    }

    public void OpenEmailSignInDialog()
    {
        overlay.style.display = DisplayStyle.Flex;
    }

    private void RegisterCallbacks()
    {
        loginButton.RegisterCallback<ClickEvent>(evt => ProcessAuth(isLogin: true));
        registerButton.RegisterCallback<ClickEvent>(evt => ProcessAuth(isLogin: false));
        closeDialogButton.RegisterCallback<ClickEvent>(evt => overlay.style.display = DisplayStyle.None);
    }

    private void ProcessAuth(bool isLogin)
    {
        string email = emailTextField.value.Trim();
        string password = passwordTextField.value.Trim();

        if (!ValidateInput(email, password)) return;

        SetUIEnabled(false);

        try
        {
            InternetChecker.Instance.CheckBeforeAction(() =>
            {
                AuthManager.Instance.AuthenticateEmail(AuthType.Email, email, password, isLogin, HandleFirebaseError);
            });
        }
        catch (Exception)
        {
            SetUIEnabled(true);
        }
    }

    private bool ValidateInput(string email, string password)
    {
        HideEmailError();
        HidePasswordError();

        if (string.IsNullOrEmpty(email) && string.IsNullOrEmpty(password))
        {
            ShowEmailError(Localizer.GetNotificationText(NotificationKey.MissingEmail));
            ShowPasswordError(Localizer.GetNotificationText(NotificationKey.EmailMissingPassword));
            return false;
        }

        if (string.IsNullOrEmpty(email))
        {
            ShowEmailError(Localizer.GetNotificationText(NotificationKey.MissingEmail));
            return false;
        }

        if (string.IsNullOrEmpty(password))
        {
            ShowPasswordError(Localizer.GetNotificationText(NotificationKey.EmailMissingPassword));
            return false;
        }

        if (!IsValidEmail(email) || !email.Contains("@") || !email.Contains("."))
        {
            ShowEmailError(Localizer.GetNotificationText(NotificationKey.InvalidEmail));
            return false;
        }

        return true;
    }

    private void HandleFirebaseError(AuthErrorTarget target, string message)
    {
        SetUIEnabled(true);
        HideEmailError();
        HidePasswordError();

        switch (target)
        {
            case AuthErrorTarget.Email:
                ShowEmailError(message);
                break;
            case AuthErrorTarget.Password:
                ShowPasswordError(message);
                break;
            case AuthErrorTarget.General:
                NotificationManager.ShowNotification(message, NotificationType.Error, 3.5f);
                break;
            default:
                NotificationManager.ShowNotification(message, NotificationType.Error);
                break;
        }
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private void ShowEmailError(string message) => emailErrorLabel.text = message;
    private void ShowPasswordError(string message) => passwordErrorLabel.text = message;
    private void HideEmailError() => emailErrorLabel.text = "";
    private void HidePasswordError() => passwordErrorLabel.text = "";

    public void SetUIEnabled(bool isEnabled)
    {
        emailTextField.SetEnabled(isEnabled);
        passwordTextField.SetEnabled(isEnabled);
        loginButton.SetEnabled(isEnabled);
        registerButton.SetEnabled(isEnabled);
        closeDialogButton.SetEnabled(isEnabled);
    }
}