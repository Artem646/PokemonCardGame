using UnityEngine.UIElements;

public class UserProfileView
{
    private static UserProfileView _instance;
    public static UserProfileView Instance => _instance ??= new UserProfileView();

    private VisualElement userImage;
    private Label userName;

    private UserProfileData cachedProfile;

    private UserProfileView() { }

    public void SetUIDocument(VisualElement root)
    {
        userImage = root.Q<VisualElement>("userImage");
        userName = root.Q<Label>("userName");

        UpdateView();

        UserProfileService.OnAvatarUpdated -= UpdateViewAfterUpdate;
        UserProfileService.OnAvatarUpdated += UpdateViewAfterUpdate;
    }

    public void UpdateView()
    {
        userName.text = cachedProfile.DisplayName;
        if (cachedProfile.PhotoTexture != null)
            userImage.style.backgroundImage = new StyleBackground(cachedProfile.PhotoTexture);
    }

    private void UpdateViewAfterUpdate()
    {
        cachedProfile = UserProfileService.Instance.GetUserProfile();
        UpdateView();
    }

    public void PreloadData(UserProfileData profileData) => cachedProfile = profileData;
    public UserProfileData GetCachedProfile() => cachedProfile;
}
