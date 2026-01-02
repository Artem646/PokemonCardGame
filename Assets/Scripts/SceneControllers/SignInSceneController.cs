using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class SignInController : MonoBehaviour
{
    [SerializeField] UIDocument uiDocument;

    private VisualElement root;
    private Button googleSignInButton;
    private Button anonymousSignInButton;

    private void Start()
    {
        root = uiDocument.rootVisualElement;
        googleSignInButton = root.Q<Button>("googleSignInButton");
        anonymousSignInButton = root.Q<Button>("anonymousSignInButton");

        googleSignInButton.clicked += OnGoogleSignInClicked;
        anonymousSignInButton.clicked += OnAnonymousSignInClicked;
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
