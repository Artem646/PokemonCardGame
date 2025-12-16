using System.Threading.Tasks;
using Firebase.Auth;
using UnityEngine;
using UnityEngine.Networking;

public class UserProfileService
{
    private static UserProfileService _instance;
    public static UserProfileService Instance => _instance ??= new UserProfileService();

    private UserProfileService() { }

    public FirebaseUser CurrentUser => FirebaseAuthService.Instance.GetAuth().CurrentUser;

    public async Task<UserProfileData> GetUserProfile()
    {
        var user = CurrentUser;
        if (user == null) return null;

        Texture2D photoTexture = null;
        string photoUrl = user.PhotoUrl?.ToString();
        if (!string.IsNullOrEmpty(photoUrl))
        {
            photoTexture = await LoadUserImage(photoUrl);
        }

        return new UserProfileData
        {
            DisplayName = user.DisplayName,
            PhotoTexture = photoTexture
        };
    }

    private async Task<Texture2D> LoadUserImage(string url)
    {
        using UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        var operation = request.SendWebRequest();

        while (!operation.isDone)
            await Task.Yield();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("[P][UserProfileService] Ошибка загрузки изображения: " + request.error);
            return null;
        }

        Texture2D texture = DownloadHandlerTexture.GetContent(request);
        return texture;
    }
}

public class UserProfileData
{
    public string DisplayName;
    public Texture2D PhotoTexture;
}
