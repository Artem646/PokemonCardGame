using UnityEngine;
using UnityEngine.UIElements;

public class FriendSceneController : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private EnterFriendNicknameDialogController enterFriendNicknameDialogController;

    private VisualElement root;
    private Button addFriendButton;

    private void Start()
    {
        InitializeUI();
        RegisterCallbacks();
    }

    private void InitializeUI()
    {
        root = uiDocument.rootVisualElement;
        addFriendButton = root.Q<Button>("addFriendButton");
    }

    private void RegisterCallbacks()
    {
        addFriendButton.RegisterCallback<ClickEvent>(evt =>
        {
            enterFriendNicknameDialogController.OpenEnterFriendNicknameDialog(new Friend(), false);
        });
    }
}
