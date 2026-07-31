using System.Linq;
using Fusion;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UIElements;

public class FriendSelectionDialogController : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;

    private VisualElement root;
    private VisualElement overlay;
    private TextField roomNameTextField;
    private DropdownField friendDropdown;
    private Toggle spectatorToggle;
    private Button joinFriendRoomButton;
    private Button closeFriendDialogButton;

    private void Start()
    {
        InitializeUI();
        RefreshFriendsDropdown();
        RegisterCallbacks();
    }

    private void InitializeUI()
    {
        root = uiDocument.rootVisualElement;
        overlay = root.Q<VisualElement>("overlay");
        roomNameTextField = root.Q<TextField>("roomNameTextField");
        friendDropdown = root.Q<DropdownField>("friendSelectField");
        spectatorToggle = root.Q<Toggle>("spectatorToggle");
        joinFriendRoomButton = root.Q<Button>("joinFriendRoomButton");
        closeFriendDialogButton = root.Q<Button>("closeFriendDialogButton");
    }

    public void RefreshFriendsDropdown()
    {
        User user = UserSession.Instance.ActiveUser;
        if (user != null && user.friends.Count > 0)
        {
            friendDropdown.choices = user.friends.Select(friend => friend.aliasName).ToList();
            joinFriendRoomButton.SetEnabled(true);
        }
        else
        {
            LocalizedString localizedValue = new("ElementsText", "EmptyDropdown");
            localizedValue.StringChanged += (str) =>
            {
                friendDropdown.choices = new() { str };
                friendDropdown.value = str;
            };
            joinFriendRoomButton.SetEnabled(false);
        }
    }

    public void OpenFriendSelectionDialog()
    {
        overlay.style.display = DisplayStyle.Flex;
    }

    private void RegisterCallbacks()
    {
        joinFriendRoomButton.RegisterCallback<ClickEvent>(async evt =>
        {
            if (string.IsNullOrEmpty(friendDropdown.value))
            {
                NotificationManager.ShowNotification("Не выбран друг для присоединения!", NotificationType.Info);
                return;
            }

            User user = UserSession.Instance.ActiveUser;
            Friend selectedFriend = user.friends.FirstOrDefault(friend => friend.aliasName == friendDropdown.value);

            string friendRoomName = string.IsNullOrEmpty(roomNameTextField.value) ? "Room" : roomNameTextField.value;
            ConnectionConfig.RoomName = friendRoomName;

            string fullFriendRoomName = $"{friendRoomName}_{selectedFriend.id}";

            bool isJoiningAsSpectator = spectatorToggle.value;
            ConnectionConfig.IsSpectator = isJoiningAsSpectator;

            if (!isJoiningAsSpectator && SelectedDeckManager.SelectedDeck == null)
            {
                NotificationManager.ShowNotification("Колода не выбрана!", NotificationType.Info);
                return;
            }

            SetUIEnabled(false);

            Debug.Log($"Подключение к комнате друга {(isJoiningAsSpectator ? "как ЗРИТЕЛЬ" : "как ИГРОК")}: {friendRoomName}...");

            StartGameResult result = await NetworkRunnerController.Instance.JoinRoom(fullFriendRoomName);
            if (result.Ok)
            {
                if (NetworkRunnerController.Instance.NetworkRunner.IsSharedModeMasterClient)
                {
                    Debug.Log("Комнаты друга нет. Отключаемся...");
                    await NetworkRunnerController.Instance.DisconnectAndRejoinLobby();
                    NotificationManager.ShowNotification("У друга нет созданной комнаты", NotificationType.Error);
                    SetUIEnabled(true);
                }
                else
                    NotificationManager.ShowNotification("Вы успешно подключились к другу!", NotificationType.Success);
            }
            else
            {
                Debug.LogError($"Ошибка подключения: {result.ShutdownReason}");
                NotificationManager.ShowNotification("Не удалось подключиться к другу", NotificationType.Error);
                await NetworkRunnerController.Instance.DisconnectAndRejoinLobby();
                SetUIEnabled(true);
            }
        });

        closeFriendDialogButton.RegisterCallback<ClickEvent>(evt =>
        {
            overlay.style.display = DisplayStyle.None;
        });
    }

    private void SetUIEnabled(bool isEnabled)
    {
        roomNameTextField.SetEnabled(isEnabled);
        friendDropdown.SetEnabled(isEnabled);
        spectatorToggle.SetEnabled(isEnabled);
        joinFriendRoomButton.SetEnabled(isEnabled);
        closeFriendDialogButton.SetEnabled(isEnabled);
    }
}
