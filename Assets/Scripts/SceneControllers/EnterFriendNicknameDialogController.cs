using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Linq;

public class EnterFriendNicknameDialogController : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;

    private VisualElement root;
    private VisualElement overlay;
    private TextField friendNicknameTextField;
    private Button confirmFriendNicknameButton;
    private Button closeDialogButton;

    private Friend currentFriend;
    bool isUpdate = false;

    public event Action<Friend> OnFriendAdded;
    public event Action<Friend> OnFriendNicknameUpdated;

    private void Start()
    {
        InitializeUI();
        RegisterCallbacks();
    }

    private void InitializeUI()
    {
        root = uiDocument.rootVisualElement;
        overlay = root.Q<VisualElement>("overlay");
        friendNicknameTextField = root.Q<TextField>("friendNicknameTextField");
        confirmFriendNicknameButton = root.Q<Button>("confirmFriendNicknameButton");
        closeDialogButton = root.Q<Button>("closeDialogButton");
    }

    public void OpenEnterFriendNicknameDialog(Friend friend, bool isUpdate)
    {
        this.isUpdate = isUpdate;
        currentFriend = friend;
        friendNicknameTextField.value = friend.aliasName;
        overlay.style.display = DisplayStyle.Flex;
    }

    private void RegisterCallbacks()
    {
        confirmFriendNicknameButton.RegisterCallback<ClickEvent>(async evt =>
        {
            if (currentFriend == null) return;

            string desiredFriendNickname = friendNicknameTextField.value?.Trim();

            if (string.IsNullOrWhiteSpace(desiredFriendNickname) || desiredFriendNickname.Length < 3)
            {
                NotificationManager.ShowNotification("Имя должно содержать минимум 3 символа.", NotificationType.Info);
                return;
            }

            if (desiredFriendNickname == UserSession.Instance.ActiveUser.userData.userName)
            {
                NotificationManager.ShowNotification("Это ваше имя!", NotificationType.Info);
                return;
            }

            if (isUpdate)
            {
                bool nameChanged = !string.Equals(currentFriend.aliasName, desiredFriendNickname, StringComparison.Ordinal);
                if (nameChanged)
                {
                    currentFriend.aliasName = desiredFriendNickname;
                    await FirebaseFirestoreService.Instance.UpdateFriend(UserSession.Instance.ActiveUser, currentFriend);
                    OnFriendNicknameUpdated?.Invoke(currentFriend);
                    overlay.style.display = DisplayStyle.None;
                }
                else
                    Localizer.LocalizeNotification(NotificationKey.NoChanges, NotificationType.Info);
            }
            else
            {
                confirmFriendNicknameButton.SetEnabled(false);
                closeDialogButton.SetEnabled(false);

                bool isAvailable = await FirebaseFirestoreService.Instance.IsNicknameAvailable(desiredFriendNickname);
                if (isAvailable)
                {
                    NotificationManager.ShowNotification("Игрока с этим именем не существует. Введите другое.", NotificationType.Error);
                    confirmFriendNicknameButton.SetEnabled(true);
                    closeDialogButton.SetEnabled(true);
                }
                else
                {
                    string friendId = await FirebaseFirestoreService.Instance.FindUserIdByName(desiredFriendNickname);
                    bool isAlreadyFriend = UserSession.Instance.ActiveUser.friends.Any(f => f.id == friendId);
                    if (isAlreadyFriend)
                    {
                        NotificationManager.ShowNotification("Этот игрок уже есть в друзьях.", NotificationType.Error);
                        confirmFriendNicknameButton.SetEnabled(true);
                        closeDialogButton.SetEnabled(true);
                        return;
                    }

                    overlay.style.display = DisplayStyle.None;

                    confirmFriendNicknameButton.SetEnabled(true);
                    closeDialogButton.SetEnabled(true);

                    Friend newFriend = new()
                    {
                        id = friendId,
                        aliasName = desiredFriendNickname
                    };

                    await FirebaseFirestoreService.Instance.AddFriend(UserSession.Instance.ActiveUser, newFriend);
                    OnFriendAdded?.Invoke(newFriend);
                }
            }
        });

        closeDialogButton.RegisterCallback<ClickEvent>(evt =>
        {
            currentFriend = null;
            overlay.style.display = DisplayStyle.None;
        });
    }
}
