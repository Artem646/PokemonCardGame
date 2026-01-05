using UnityEngine;
using UnityEngine.UIElements;
using System.Threading.Tasks;
using System;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization;
using System.Collections.Generic;

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
    private Button saveProfileButton;
    private Button closeButton;
    private CustomizableButton singOutButton;
    private RadioButtonGroup languageGroup;
    // private Button googleLinkButton;

    private User user;

    public event Action<User> OnProfileUpdated;

    private void Start()
    {
        InitializeUI();

        LocalizeElements();

        // user = UserSession.Instance.ActiveUser;
        // FillUIFromUser(user);

        // if (user.userData.email == "")
        // {
        //     googleLinkButton.style.display = DisplayStyle.Flex;
        //     googleLinkButton.clicked += OnGoogleLinkClicked;
        // }
        // else
        // {
        //     googleLinkButton.style.display = DisplayStyle.None;
        // }

        saveProfileButton.clicked += OnSaveSettingsClicked;
        singOutButton.RegisterCallback<ClickEvent>(OnSignOutClicked);
        closeButton.clicked += OnCloseClicked;
        languageGroup.RegisterValueChangedCallback(OnLanguageChanged);
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
        saveProfileButton = root.Q<Button>("saveProfileButton");
        closeButton = root.Q<Button>("closeButton");
        singOutButton = root.Q<CustomizableButton>("singOutButton");
        languageGroup = root.Q<RadioButtonGroup>("languageGroup");
        // googleLinkButton = root.Q<Button>("googleLinkButton");
    }

    private void LocalizeElements()
    {
        Localizer.LocalizeElements(root, new[]
        {
            ("profileSettingsLabel", "ProfileSettingsLabel"),
            ("langSettingsLabel", "LangSettingsLabel"),
            ("singOutLabel", "SingOutLabel"),
            ("saveProfileButton", "SaveProfileButton")
        }, "ElementsText");
    }

    private void OnLanguageChanged(ChangeEvent<int> evt)
    {
        int selectedIndex = evt.newValue;
        string selectedCode = selectedIndex switch
        {
            0 => "ru",
            1 => "en",
            2 => "be",
            _ => "en"
        };

        SetLocale(selectedCode);
        PlayerPrefs.SetString("locale", selectedCode);
        PlayerPrefs.Save();
    }

    private void SetLocale(string code)
    {
        List<Locale> locales = LocalizationSettings.AvailableLocales.Locales;
        Locale locale = locales.Find(locale => locale.Identifier.Code == code);
        LocalizationSettings.SelectedLocale = locale;
    }

    private void SyncLanguageGroupWithCurrentLocale()
    {
        Locale currentLocale = LocalizationSettings.SelectedLocale;
        switch (currentLocale.Identifier.Code)
        {
            case "ru":
                languageGroup.value = 0;
                break;
            case "en":
                languageGroup.value = 1;
                break;
        }
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
        SyncLanguageGroupWithCurrentLocale();
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

        Texture2D texture = await UserProfileService.Instance.GetUserProfile()
            .ContinueWith(tex => tex.Result.PhotoTexture);

        if (texture != null)
            userImage.style.backgroundImage = new StyleBackground(texture);
    }
}
