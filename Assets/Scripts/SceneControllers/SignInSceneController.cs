using Firebase.Auth;
using UnityEngine;
using UnityEngine.UIElements;

public class SignInSceneController : MonoBehaviour
{
    [SerializeField] UIDocument uiDocument;
    [SerializeField] EnterNicknameDialogController enterNicknameDialogController;
    [SerializeField] EmailSignInDialogController emailSignInDialogController;

    private VisualElement root;
    private Button googleSignInButton;
    private Button emailSignInButton;
    private Button anonymousSignInButton;

    private void Start()
    {
        InitializeUI();

        googleSignInButton.clicked += OnGoogleSignInClicked;
        emailSignInButton.clicked += OnEmailSignInClicked;
        anonymousSignInButton.clicked += OnAnonymousSignInClicked;

        FirebaseAuthService.Instance.OnNewUserNeedsRegistration += HandleNewUserRegistration;
    }

    private void InitializeUI()
    {
        root = uiDocument.rootVisualElement;
        googleSignInButton = root.Q<Button>("googleSignInButton");
        emailSignInButton = root.Q<Button>("emailSignInButton");
        anonymousSignInButton = root.Q<Button>("anonymousSignInButton");
    }

    public void OnGoogleSignInClicked()
    {
        InternetChecker.Instance.CheckBeforeAction(() =>
        {
            AuthManager.Instance.SignIn(AuthType.Google);
        });
    }

    public void OnEmailSignInClicked()
    {
        InternetChecker.Instance.CheckBeforeAction(() =>
        {
            emailSignInDialogController.OpenEmailSignInDialog();
        });
    }

    public void OnAnonymousSignInClicked()
    {
        InternetChecker.Instance.CheckBeforeAction(() =>
        {
            AuthManager.Instance.SignIn(AuthType.Anonymous);
        });
    }

    private void HandleNewUserRegistration(FirebaseUser newUser)
    {
        enterNicknameDialogController.OpenEnterNicknameDialog(newUser);
    }

    private void OnDestroy()
    {
        if (FirebaseAuthService.Instance != null)
            FirebaseAuthService.Instance.OnNewUserNeedsRegistration -= HandleNewUserRegistration;
    }
}
