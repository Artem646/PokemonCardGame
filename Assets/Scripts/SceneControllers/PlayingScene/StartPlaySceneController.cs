using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public static class GameModeConfig
{
    public static bool IsMultiplayer { get; set; }
}

public class StartPlaySceneController : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private SettingsController settingsController;

    private VisualElement root;
    private CustomizableButton singlePlayerButton;
    private CustomizableButton multyPlayerButton;
    private CustomizableButton decksButton;
    private VisualElement profileField;

    private async void Start()
    {
        root = uiDocument.rootVisualElement;
        singlePlayerButton = root.Q<CustomizableButton>("singlePlayerButton");
        multyPlayerButton = root.Q<CustomizableButton>("multyPlayerButton");
        decksButton = root.Q<CustomizableButton>("decksButton");
        profileField = root.Q<VisualElement>("profileField");

        UserProfileView.Instance.SetUIDocument(uiDocument, settingsController);
        await UserProfileView.Instance.LoadUserData();

        RegisterCallbacks();
    }

    private void RegisterCallbacks()
    {
        root.Q<Button>("bestiaryButton").RegisterCallback<ClickEvent>(evt =>
        {
            SceneContext.PreviousSceneName = SceneManager.GetActiveScene().name;
            SceneSwitcher.SwitchScene("BestiaryScene", root);
        });

        root.Q<Button>("collectionButton").RegisterCallback<ClickEvent>(evt =>
        {
            SceneSwitcher.SwitchScene("CollectionScene", root);
        });

        multyPlayerButton.RegisterCallback<ClickEvent>(evt =>
        {
            GameModeConfig.IsMultiplayer = true;
            SceneSwitcher.SwitchScene("DeckSelectionScene", root);
        });

        singlePlayerButton.RegisterCallback<ClickEvent>(evt =>
        {
            GameModeConfig.IsMultiplayer = false;
            SceneSwitcher.SwitchScene("DeckSelectionScene", root);
        });

        decksButton.RegisterCallback<ClickEvent>(evt =>
        {
            SceneSwitcher.SwitchScene("DecksScene", root);
        });

        profileField.RegisterCallback<ClickEvent>(evt =>
        {
            settingsController.OpenSettings();
        });
    }
}
