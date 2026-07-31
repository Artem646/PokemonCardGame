using Firebase.Auth;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class EnterNicknameDialogController : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;

    private VisualElement root;
    private VisualElement overlay;
    private TextField nicknameTextField;
    private Button confirmNicknameButton;
    private Button closeDialogButton;

    private FirebaseUser pendingUser;

    private void Start()
    {
        InitializeUI();
        RegisterCallbacks();
    }

    private void InitializeUI()
    {
        root = uiDocument.rootVisualElement;
        overlay = root.Q<VisualElement>("overlay");
        nicknameTextField = root.Q<TextField>("nicknameTextField");
        confirmNicknameButton = root.Q<Button>("confirmNicknameButton");
        closeDialogButton = root.Q<Button>("closeDialogButton");
    }

    public void OpenEnterNicknameDialog(FirebaseUser user)
    {
        pendingUser = user;
        nicknameTextField.value = "";
        overlay.style.display = DisplayStyle.Flex;
    }

    private void RegisterCallbacks()
    {
        confirmNicknameButton.RegisterCallback<ClickEvent>(async evt =>
        {
            if (pendingUser == null) return;

            string desiredNickname = nicknameTextField.value.Trim();
            if (string.IsNullOrWhiteSpace(desiredNickname) || desiredNickname.Length < 3)
            {
                NotificationManager.ShowNotification("Имя должно содержать минимум 3 символа.", NotificationType.Info);
                return;
            }

            confirmNicknameButton.SetEnabled(false);
            closeDialogButton.SetEnabled(false);

            bool isAvailable = await FirebaseFirestoreService.Instance.IsNicknameAvailable(desiredNickname);
            if (isAvailable)
            {
                User newUser = await FirebaseFirestoreService.Instance.CreateUserDocument(pendingUser, desiredNickname);
                UserSession.Instance.ActiveUser = newUser;
                SceneManager.LoadScene("UploadingScene");
            }
            else
            {
                NotificationManager.ShowNotification("Это имя уже занято. Выберите другое.", NotificationType.Error);
                confirmNicknameButton.SetEnabled(true);
                closeDialogButton.SetEnabled(true);
            }
        });

        closeDialogButton.RegisterCallback<ClickEvent>(async evt =>
        {
            closeDialogButton.SetEnabled(false);
            confirmNicknameButton.SetEnabled(false);

            if (pendingUser != null)
            {
                try
                {
                    await pendingUser.DeleteAsync();
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[NicknameDialog] Ошибка при удалении аккаунта: {ex.Message}");
                }
                finally
                {
                    FirebaseAuthService.Instance.GetAuth().SignOut();
                }
            }

            overlay.style.display = DisplayStyle.None;
            pendingUser = null;

            closeDialogButton.SetEnabled(true);
            confirmNicknameButton.SetEnabled(true);
        });
    }
}
