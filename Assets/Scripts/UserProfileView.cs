using System.Collections;
using System.Threading.Tasks;
using Firebase.Auth;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

public class UserProfileView
{
    private readonly VisualElement userImage;
    private readonly Label userName;

    public UserProfileView(VisualElement root)
    {
        userImage = root.Q<VisualElement>("userImage");
        userName = root.Q<Label>("userName");
    }

    public async Task LoadUserData()
    {
        FirebaseUser user = FirebaseService.Instance.GetAuth().CurrentUser;
        if (user == null) return;

        userName.text = user.DisplayName;
        string photoUrl = user.PhotoUrl?.ToString();
        if (!string.IsNullOrEmpty(photoUrl))
        {
            Texture2D texture = await LoadUserImage(photoUrl);
            userImage.style.backgroundImage = new StyleBackground(texture);
        }
    }

    private async Task<Texture2D> LoadUserImage(string url)
    {
        using UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        var operation = request.SendWebRequest();

        while (!operation.isDone)
            await Task.Yield();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("[P] Ошибка загрузки изображения: " + request.error);
            return null;
        }

        Texture2D texture = DownloadHandlerTexture.GetContent(request);
        Debug.Log("[P] Изображение пользователя установлено.");
        return texture;
    }
}
