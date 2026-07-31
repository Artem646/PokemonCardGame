using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class FriendController : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private VisualTreeAsset friendTemplate;
    [SerializeField] private EnterFriendNicknameDialogController enterFriendNicknameDialogController;

    private VisualElement root;
    private ScrollView friendsScrollView;
    private VisualElement friendRoot;
    private Label friendNameLabel;
    private VisualElement friendImage;
    private Button editFriendNameButton;
    private Button deleteFriendButton;
    private Button updatePhotoButton;

    private const int MaxImageSize = 256;

    private void Start()
    {
        InitializeUI();
        AddFriendsToContainer(UserSession.Instance.ActiveUser.friends);
        RegisterEvent();
    }

    private void InitializeUI()
    {
        root = uiDocument.rootVisualElement;
        friendsScrollView = root.Q<ScrollView>("friendsScrollView");
    }

    private void AddFriendsToContainer(List<Friend> friends)
    {
        friendsScrollView.Clear();
        foreach (Friend friend in friends)
            AddFriendToContainer(friend);
    }

    private void AddFriendToContainer(Friend friend)
    {
        friendRoot = friendTemplate.Instantiate();
        friendRoot.userData = friend.id;

        friendRoot.style.width = new StyleLength(new Length(960, LengthUnit.Pixel));
        friendRoot.style.height = new StyleLength(new Length(300, LengthUnit.Pixel));

        InitializeFriendUI();

        friendNameLabel.text = friend.aliasName;
        UpdateFriendImage(friend, friendRoot);

        RegisterCallbacks(friend);

        friendsScrollView.Add(friendRoot);
    }

    private void InitializeFriendUI()
    {
        friendNameLabel = friendRoot.Q<Label>("friendNameLabel");
        editFriendNameButton = friendRoot.Q<Button>("editFriendNameButton");
        deleteFriendButton = friendRoot.Q<Button>("deleteFriendButton");
        updatePhotoButton = friendRoot.Q<Button>("updatePhotoButton");
    }

    private void UpdateFriendImage(Friend friend, VisualElement friendRoot)
    {
        static Texture2D BytesToTexture(byte[] bytes)
        {
            Texture2D texture = new(2, 2);
            if (texture.LoadImage(bytes)) return texture;
            return null;
        }

        Texture2D photoTexture = BytesToTexture(friend.photoData);
        if (photoTexture != null)
        {
            friendImage = friendRoot.Q<VisualElement>("friendImage");
            friendImage.style.backgroundImage = new StyleBackground(photoTexture);
        }
    }

    public async void ProcessAndUploadPhoto(Friend friend, Texture2D originalTexture)
    {
        VisualElement friendRoot = friendsScrollView.Children().FirstOrDefault(child => (string)child.userData == friend.id);

        Texture2D resizedTexture = ResizeTexture(originalTexture, MaxImageSize, MaxImageSize);
        byte[] photoBytes = resizedTexture.EncodeToJPG(75);

        try
        {
            await FirebaseFirestoreService.Instance.UpdateFriendPhoto(UserSession.Instance.ActiveUser.userData.userId, friend, photoBytes);
            friend.photoData = photoBytes;
            UpdateFriendImage(friend, friendRoot);
            NotificationManager.ShowNotification("Аватар друга успешно обновлён!", NotificationType.Success);
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

    private void RegisterCallbacks(Friend friend)
    {
        editFriendNameButton.RegisterCallback<ClickEvent>(evt =>
        {
            enterFriendNicknameDialogController.OpenEnterFriendNicknameDialog(friend, true);
        });

        deleteFriendButton.RegisterCallback<ClickEvent>(evt =>
        {
            _ = FirebaseFirestoreService.Instance.DeleteFriend(UserSession.Instance.ActiveUser, friend);
            VisualElement friendRootToRemove = friendsScrollView.Children().FirstOrDefault(child => (string)child.userData == friend.id);
            friendsScrollView.Remove(friendRootToRemove);
        });

        updatePhotoButton.RegisterCallback<ClickEvent>(evt =>
        {
            NativeGallery.GetImageFromGallery((path) =>
            {
                if (path != null)
                {
                    Texture2D texture = NativeGallery.LoadImageAtPath(path, markTextureNonReadable: false);
                    if (texture == null) return;
                    ProcessAndUploadPhoto(friend, texture);
                }
            }, "Выберите фото профиля", "image/*");
        });
    }

    private void RegisterEvent()
    {
        enterFriendNicknameDialogController.OnFriendNicknameUpdated += friend =>
        {
            friendRoot = friendsScrollView.Children().FirstOrDefault(child => (string)child.userData == friend.id);
            friendNameLabel = friendRoot.Q<Label>("friendNameLabel");
            friendNameLabel.text = friend.aliasName;
        };

        enterFriendNicknameDialogController.OnFriendAdded += newFriend =>
        {
            AddFriendToContainer(newFriend);
        };
    }
}
