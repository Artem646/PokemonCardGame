using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class DecksSceneController : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private SettingsController settingsController;

    private VisualElement root;
    private VisualElement profileField;
    private VisualElement cardOverlay;

    private async void Start()
    {
        root = uiDocument.rootVisualElement;
        profileField = root.Q<VisualElement>("profileField");
        cardOverlay = root.Q<VisualElement>("overlay");

        CardOverlayManager.Instance.RegisterOverlayVisualElement(SceneManager.GetActiveScene().name, cardOverlay);

        UserProfileView.Instance.SetUIDocument(uiDocument, settingsController);
        await UserProfileView.Instance.LoadUserData();

        RegisterCallbacks();
    }

    private void RegisterCallbacks()
    {
        root.Q<Button>("playButton").RegisterCallback<ClickEvent>(evt =>
        {
            SceneSwitcher.SwitchScene("StartPlayScene", root);
        });

        root.Q<Button>("bestiaryButton").RegisterCallback<ClickEvent>(evt =>
        {
            SceneContext.PreviousMenuSceneName = SceneManager.GetActiveScene().name;
            SceneSwitcher.SwitchScene("BestiaryScene", root);
        });

        root.Q<Button>("collectionButton").RegisterCallback<ClickEvent>(evt =>
        {
            SceneSwitcher.SwitchScene("CollectionScene", root);
        });

        profileField.RegisterCallback<ClickEvent>(evt =>
        {
            settingsController.OpenSettings();
        });
    }
}
