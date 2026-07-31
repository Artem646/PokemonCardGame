using System.Collections.Generic;
using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public enum RoomVisibility
{
    Visible = 0,
    Hidden = 1
}


public class RoomCreationDialogController : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;

    private VisualElement root;
    private VisualElement overlay;
    private TextField roomNameTextField;
    private RadioButtonGroup roomVisibilityRadioButtonGroup;
    private Button createRoomButton;
    private Button closeRoomCreationDialogButton;

    private NetworkRunner networkRunner;

    private void Start()
    {
        InitializeUI();
        string[] keys = { "PublicAccessChoice", "PrivateAccessChoice" };
        Localizer.LocalizeChoices(roomVisibilityRadioButtonGroup, keys);
        RegisterCallbacks();
    }

    private void InitializeUI()
    {
        root = uiDocument.rootVisualElement;
        overlay = root.Q<VisualElement>("overlay");
        roomNameTextField = root.Q<TextField>("roomNameTextField");
        roomVisibilityRadioButtonGroup = root.Q<RadioButtonGroup>("roomVisibilityRadioButtonGroup");
        createRoomButton = root.Q<Button>("createRoomButton");
        closeRoomCreationDialogButton = root.Q<Button>("closeRoomCreationDialogButton");
    }

    public void OpenRoomCreationDialog()
    {
        overlay.style.display = DisplayStyle.Flex;
    }

    private void RegisterCallbacks()
    {
        createRoomButton.RegisterCallback<ClickEvent>(async evt =>
        {
            if (SelectedDeckManager.SelectedDeck == null)
            {
                NotificationManager.ShowNotification("Колода не выбрана!", NotificationType.Info);
                return;
            }

            SetUIEnabled(false);

            bool isVisible = (RoomVisibility)roomVisibilityRadioButtonGroup.value == RoomVisibility.Visible;

            ConnectionConfig.RoomName = string.IsNullOrEmpty(roomNameTextField.value) ? "Room" : roomNameTextField.value;
            string roomName = null;

            if (isVisible)
                roomName = ConnectionConfig.RoomName;
            else
            {
                string myUserId = UserSession.Instance.ActiveUser.userData.userId;
                roomName = $"{ConnectionConfig.RoomName}_{myUserId}";
            }

            Debug.Log($"Создаем комнату: {ConnectionConfig.RoomName}, Видимость: {isVisible}");

            NetworkRunnerController.Instance.InitializeRunnerIfNeeded();
            networkRunner = NetworkRunnerController.Instance.NetworkRunner;
            INetworkSceneManager sceneManager = networkRunner.gameObject.GetComponent<INetworkSceneManager>() ?? networkRunner.gameObject.AddComponent<NetworkSceneManagerDefault>();

            Dictionary<string, SessionProperty> properties = new()
            {
                { "IsStarted", 0 },
                { "IsLobbySceneLoaded", 0 },
                { "IsRoomFull", 0 }
            };

            StartGameResult result = await networkRunner.StartGame(new StartGameArgs
            {
                GameMode = ConnectionConfig.Mode,
                SessionName = roomName,
                Scene = SceneRef.FromIndex(SceneUtility.GetBuildIndexByScenePath($"Assets/Scenes/LobbyScene.unity")),
                SceneManager = sceneManager,
                PlayerCount = 20,
                IsVisible = isVisible,
                SessionProperties = properties,
                IsOpen = true
            });

            if (result.Ok)
                Debug.Log("Комната успешно создана!");
            else
            {
                Debug.LogError($"Ошибка создания комнаты: {result.ShutdownReason}");
                NotificationManager.ShowNotification("Не удалось создать комнату", NotificationType.Error);
                await NetworkRunnerController.Instance.DisconnectAndRejoinLobby();
                SetUIEnabled(true);
            }
        });

        closeRoomCreationDialogButton.RegisterCallback<ClickEvent>(evt =>
        {
            overlay.style.display = DisplayStyle.None;
        });
    }

    private void SetUIEnabled(bool isEnabled)
    {
        createRoomButton.SetEnabled(isEnabled);
        closeRoomCreationDialogButton.SetEnabled(isEnabled);
        roomNameTextField.SetEnabled(isEnabled);
        roomVisibilityRadioButtonGroup.SetEnabled(isEnabled);
    }
}
