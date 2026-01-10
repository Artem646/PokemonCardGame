using UnityEngine;
using UnityEngine.UIElements;

public class SignInController : MonoBehaviour
{
    [SerializeField] UIDocument uiDocument;

    private VisualElement root;
    private Button googleSignInButton;
    private Button anonymousSignInButton;

    private void Start()
    {
        InitializeUI();

        googleSignInButton.clicked += OnGoogleSignInClicked;
        anonymousSignInButton.clicked += OnAnonymousSignInClicked;
    }

    private void InitializeUI()
    {
        root = uiDocument.rootVisualElement;
        googleSignInButton = root.Q<Button>("googleSignInButton");
        anonymousSignInButton = root.Q<Button>("anonymousSingInButton");
    }

    public void OnGoogleSignInClicked()
    {
        InternetChecker.Instance.CheckBeforeAction(() =>
        {
            AuthManager.Instance.SignIn(AuthType.Google);
        });
    }

    public void OnAnonymousSignInClicked()
    {
        InternetChecker.Instance.CheckBeforeAction(() =>
        {
            AuthManager.Instance.SignIn(AuthType.Anonymous);
        });
    }
}
