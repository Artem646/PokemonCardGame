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

    private VisualElement root;
    private ParallelogramButton2 singlePlayerButton;
    private ParallelogramButton2 multyPlayerButton;
    private ParallelogramButton2 decksButton;

    private void Start()
    {
        root = uiDocument.rootVisualElement;
        singlePlayerButton = root.Q<ParallelogramButton2>("singlePlayerButton");
        multyPlayerButton = root.Q<ParallelogramButton2>("multyPlayerButton");
        decksButton = root.Q<ParallelogramButton2>("decksButton");

        UserProfileView.Instance.SetUIDocument(uiDocument);
        UserProfileView.Instance.UpdateView(UserProfileView.Instance.GetCachedProfile());

        RegisterCallbacks();
    }

    private void RegisterCallbacks()
    {
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

        root.Q<Button>("bestiaryButton").RegisterCallback<ClickEvent>(evt =>
        {
            SceneContext.PreviousSceneName = SceneManager.GetActiveScene().name;
            SceneSwitcher.SwitchScene("BestiaryScene", root);
        });

        root.Q<Button>("collectionButton").RegisterCallback<ClickEvent>(evt =>
        {
            SceneSwitcher.SwitchScene("CollectionScene", root);
        });
    }
}
