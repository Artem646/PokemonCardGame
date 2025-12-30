using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class UserProfileView
{
    private static UserProfileView _instance;
    public static UserProfileView Instance => _instance ??= new UserProfileView();

    private VisualElement userImage;
    private Label userName;
    private bool isInitialized;

    private UserProfileData cachedProfile;

    private UserProfileView() { }

    public void SetUIDocument(UIDocument uiDocument, SettingsController settingsController)
    {
        VisualElement root = uiDocument.rootVisualElement;
        userImage = root.Q<VisualElement>("userImage");
        userName = root.Q<Label>("userName");
        isInitialized = true;

        if (cachedProfile != null)
            UpdateView(cachedProfile);

        settingsController.OnProfileUpdated += UpdateViewFromUser;
    }

    public async Task LoadUserData()
    {
        if (cachedProfile != null)
        {
            UpdateView(cachedProfile);
            return;
        }

        UserProfileData profileData = await UserProfileService.Instance.GetUserProfile();
        cachedProfile = profileData;
        UpdateView(profileData);
    }

    public void UpdateView(UserProfileData profileData)
    {
        if (!isInitialized || profileData == null) return;

        userName.text = profileData.DisplayName;
        if (profileData.PhotoTexture != null)
        {
            userImage.style.backgroundImage = new StyleBackground(profileData.PhotoTexture);
        }
    }

    private async void UpdateViewFromUser(User user)
    {
        if (!isInitialized || user == null) return;

        UserProfileData profileData = new()
        {
            DisplayName = user.userData.userName,
            PhotoTexture = null
        };

        if (!string.IsNullOrEmpty(user.userData.profilePhotoUrl))
        {
            Texture2D texture = await UserProfileService.Instance.GetUserProfile()
                .ContinueWith(t => t.Result.PhotoTexture);
            profileData.PhotoTexture = texture;
        }

        cachedProfile = profileData;
        UpdateView(profileData);
    }

    public void PreloadData(UserProfileData data)
    {
        cachedProfile = data;
    }

    public UserProfileData GetCachedProfile() => cachedProfile;
}
