using UnityEngine;
using UnityEngine.UIElements;

public class SignOutUI : MonoBehaviour
{
    [SerializeField] UIDocument uiDocument;
    private VisualElement root;

    void Start()
    {
        root = uiDocument.rootVisualElement;
        var signOutButton = root.Q<VisualElement>("signOutButton");
        signOutButton.RegisterCallback<ClickEvent>(OnSignOutClicked);
    }

    public void OnSignOutClicked(ClickEvent evt) => AuthManager.Instance.SignOut();
}