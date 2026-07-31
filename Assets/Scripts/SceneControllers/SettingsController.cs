using UnityEngine;
using UnityEngine.UIElements;
using System;
using WebSocketSharp;

public class SettingsController : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;

    private VisualElement root;
    private VisualElement overlay;
    private VisualElement userImage;
    private Label userNameLabel;
    private Label userIdLabel;
    private Label emailLabel;
    private Label accountCreationDateLabel;
    private Button closeButton;
    private CustomizableButton singOutButton;
    private Button updatePhotoButton;

    private User user;

    private const int MaxImageSize = 256;

    private void Start()
    {
        InitializeUI();

        singOutButton.RegisterCallback<ClickEvent>(OnSignOutButtonClicked);
        closeButton.clicked += OnCloseButtonClicked;
        updatePhotoButton.clicked += OnUpdatePhotoButtonClicked;
    }

    private void InitializeUI()
    {
        root = uiDocument.rootVisualElement;
        overlay = root.Q<VisualElement>("overlay");
        userImage = root.Q<VisualElement>("userImage");
        userNameLabel = root.Q<Label>("userNameLabel");
        userIdLabel = root.Q<Label>("userIdLabel");
        emailLabel = root.Q<Label>("emailLabel");
        accountCreationDateLabel = root.Q<Label>("accountCreationDateLabel");
        closeButton = root.Q<Button>("closeButton");
        singOutButton = root.Q<CustomizableButton>("singOutButton");
        updatePhotoButton = root.Q<Button>("updatePhotoButton");
    }

    public void OpenSettings()
    {
        user = UserSession.Instance.ActiveUser;
        FillUIFromUser(user);
        overlay.style.display = DisplayStyle.Flex;
    }

    private void FillUIFromUser(User user)
    {
        userNameLabel.text = user.userData.userName;
        userIdLabel.text = user.userData.userId;
        emailLabel.text = !user.userData.email.IsNullOrEmpty() ? user.userData.email : "Anonim";
        accountCreationDateLabel.text = user.userData.createdAt.ToString();

        userIdLabel.RegisterCallback<ClickEvent>(evt =>
        {
            GUIUtility.systemCopyBuffer = userIdLabel.text;
            NotificationManager.ShowNotification($"Скопировано: {userIdLabel.text}", NotificationType.Info, 1);
        });

        UpdateUserImage();
    }

    private void UpdateUserImage()
    {
        static Texture2D BytesToTexture(byte[] bytes)
        {
            Texture2D texture = new(2, 2);
            if (texture.LoadImage(bytes)) return texture;
            return null;
        }

        Texture2D photoTexture = BytesToTexture(user.userData.profilePhotoData);
        if (photoTexture != null)
            userImage.style.backgroundImage = new StyleBackground(photoTexture);
    }

    public void OnUpdatePhotoButtonClicked()
    {
        NativeGallery.GetImageFromGallery((path) =>
        {
            if (path != null)
            {
                Texture2D texture = NativeGallery.LoadImageAtPath(path, markTextureNonReadable: false);
                if (texture == null) return;
                ProcessAndUploadPhoto(texture);
            }
        }, "Выберите фото профиля", "image/*");
    }

    public async void ProcessAndUploadPhoto(Texture2D originalTexture)
    {
        Texture2D resizedTexture = ResizeTexture(originalTexture, MaxImageSize, MaxImageSize);
        byte[] photoBytes = resizedTexture.EncodeToJPG(75);

        string userId = UserSession.Instance.ActiveUser.userData.userId;

        try
        {
            await FirebaseFirestoreService.Instance.UpdateUserPhoto(userId, photoBytes);
            UserSession.Instance.ActiveUser.userData.profilePhotoData = photoBytes;
            UpdateUserImage();
            UserProfileService.Instance.NotifyAvatarUpdated();
            NotificationManager.ShowNotification("Аватар успешно обновлён!", NotificationType.Success);
        }
        catch (Exception e)
        {
            Debug.LogError("Ошибка загрузки фото: " + e.Message);
        }

        Destroy(originalTexture);
        Destroy(resizedTexture);
    }

    private Texture2D ResizeTexture(Texture2D source, int targetWidth, int targetHeight)
    {
        RenderTexture renderTexture = RenderTexture.GetTemporary(targetWidth, targetHeight);
        Graphics.Blit(source, renderTexture);

        Texture2D result = new(targetWidth, targetHeight);
        RenderTexture.active = renderTexture;
        result.ReadPixels(new Rect(0, 0, targetWidth, targetHeight), 0, 0);
        result.Apply();

        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(renderTexture);
        return result;
    }

    private void OnCloseButtonClicked()
    {
        overlay.style.display = DisplayStyle.None;
    }

    private void OnSignOutButtonClicked(ClickEvent evt)
    {
        CardRepository.Instance.ClearUserCards();
        AuthManager.Instance.SignOut();
    }
}
