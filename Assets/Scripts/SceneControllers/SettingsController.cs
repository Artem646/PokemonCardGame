using UnityEngine;
using UnityEngine.UIElements;
using System.Threading.Tasks;
using System;

public class SettingsController : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;

    private VisualElement root;
    private VisualElement overlay;
    private VisualElement userImage;
    private TextField userNameField;
    private TextField profilePhotoUrlField;
    private TextField userIdField;
    private TextField emailField;
    private Button saveSettingsButton;
    private Button closeButton;
    private CustomizableButton singOutButton;
    // private Button googleLinkButton;

    private User user;

    public event Action<User> OnProfileUpdated;

    private void Start()
    {
        InitializeUI();

        user = UserSession.Instance.ActiveUser;

        FillUIFromUser(user);

        // if (user.userData.email == "")
        // {
        //     googleLinkButton.style.display = DisplayStyle.Flex;
        //     googleLinkButton.clicked += OnGoogleLinkClicked;
        // }
        // else
        // {
        //     googleLinkButton.style.display = DisplayStyle.None;
        // }

        saveSettingsButton.clicked += OnSaveSettingsClicked;
        singOutButton.RegisterCallback<ClickEvent>(OnSignOutClicked);
        closeButton.clicked += OnCloseClicked;
    }

    private void InitializeUI()
    {
        root = uiDocument.rootVisualElement;
        overlay = root.Q<VisualElement>("overlay");
        userImage = root.Q<VisualElement>("userImage");
        userNameField = root.Q<TextField>("userNameField");
        profilePhotoUrlField = root.Q<TextField>("profilePhotoUrlField");
        userIdField = root.Q<TextField>("userIdField");
        emailField = root.Q<TextField>("emailField");
        saveSettingsButton = root.Q<Button>("saveSettingsButton");
        closeButton = root.Q<Button>("closeButton");
        singOutButton = root.Q<CustomizableButton>("singOutButton");
        // googleLinkButton = root.Q<Button>("googleLinkButton");
    }

    private async void OnSaveSettingsClicked()
    {
        string newUserName = userNameField.value;
        string newProfilePhotoUrl = profilePhotoUrlField.value;

        await FirebaseFirestoreService.Instance.UpdateUserProfile(user, newUserName, newProfilePhotoUrl);
        await UpdateUserImage(user.userData.profilePhotoUrl);

        userIdField.value = $"UserID: {user.userData.userId}";
        emailField.value = $"Email: {user.userData.email}";

        OnProfileUpdated?.Invoke(user);
    }

    // private async void OnGoogleLinkClicked()
    // {
    //     string idToken = await GoogleIdTokenGetter.GetIdTokenAsync();
    //     await FirebaseAuthService.Instance.LinkAnonymousToGoogle(idToken);

    //     var currentUser = FirebaseAuthService.Instance.GetAuth().CurrentUser;

    //     if (currentUser != null)
    //     {
    //         await currentUser.ReloadAsync();
    //         emailField.value = $"Email: {currentUser.Email}";
    //     }

    //     googleLinkButton.style.display = DisplayStyle.None;
    // }

    public void OpenSettings()
    {
        user = UserSession.Instance.ActiveUser;
        FillUIFromUser(user);
        overlay.style.display = DisplayStyle.Flex;
    }

    private void OnCloseClicked()
    {
        overlay.style.display = DisplayStyle.None;
    }

    private void OnSignOutClicked(ClickEvent evt)
    {
        CardRepository.Instance.ClearUserCards();
        AuthManager.Instance.SignOut();
    }

    private void FillUIFromUser(User user)
    {
        userNameField.value = user.userData.userName;
        profilePhotoUrlField.value = user.userData.profilePhotoUrl;
        userIdField.value = $"UserID: {user.userData.userId}";
        emailField.value = $"Email: {user.userData.email}";
        _ = UpdateUserImage(user.userData.profilePhotoUrl);
    }

    private async Task UpdateUserImage(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return;

        Texture2D tex = await UserProfileService.Instance.GetUserProfile()
            .ContinueWith(t => t.Result.PhotoTexture);

        if (tex != null)
            userImage.style.backgroundImage = new StyleBackground(tex);
    }
}
