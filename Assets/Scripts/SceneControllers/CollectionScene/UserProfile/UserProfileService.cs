using System;
using UnityEngine;

public class UserProfileData
{
    public string DisplayName;
    public Texture2D PhotoTexture;
}

public class UserProfileService
{
    private static UserProfileService _instance;
    public static UserProfileService Instance => _instance ??= new UserProfileService();

    public static event Action OnAvatarUpdated;

    private UserProfileService() { }

    public void NotifyAvatarUpdated() => OnAvatarUpdated?.Invoke();

    public UserProfileData GetUserProfile()
    {
        User user = UserSession.Instance.ActiveUser;
        Texture2D photoTexture = BytesToTexture(user.userData.profilePhotoData);
        return new UserProfileData
        {
            DisplayName = user.userData.userName,
            PhotoTexture = photoTexture
        };
    }

    private Texture2D BytesToTexture(byte[] bytes)
    {
        if (bytes == null || bytes.Length == 0) return null;
        Texture2D texture = new(2, 2);
        if (texture.LoadImage(bytes)) return texture;
        return null;
    }
}
