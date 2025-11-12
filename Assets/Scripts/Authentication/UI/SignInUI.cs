using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class SignInUI : MonoBehaviour
{
    [SerializeField] UIDocument uiDocument;
    private VisualElement root;

    void Start()
    {
        SceneContext.PreviousSceneName = SceneManager.GetActiveScene().name;
        root = uiDocument.rootVisualElement;
        VisualElement googleSignInButton = root.Q<VisualElement>("googleSignInButton");
        VisualElement anonymousSignInButton = root.Q<VisualElement>("anonymousSignInButton");
        googleSignInButton.RegisterCallback<ClickEvent>(OnGoogleSignInClicked);
        anonymousSignInButton.RegisterCallback<ClickEvent>(OnAnonymousSignInClicked);
    }

    public void OnGoogleSignInClicked(ClickEvent evt)
    {
        InternetChecker.Instance.CheckBeforeAction(() =>
        {
            AuthManager.Instance.SignIn(AuthType.Google);
        });
    }

    public void OnAnonymousSignInClicked(ClickEvent evt)
    {
        InternetChecker.Instance.CheckBeforeAction(() =>
        {
            AuthManager.Instance.SignIn(AuthType.Anonymous);
        });
    }


    // public void OnGoogleSignInClicked(ClickEvent evt) => AuthManager.Instance.SignIn(AuthType.Google);

    // public void OnAnonymousSignInClicked(ClickEvent evt) => AuthManager.Instance.SignIn(AuthType.Anonymous);
}
