using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public enum GameType
{
    Bot,
    Multiplayer
}

public static class GameTypeConfig
{
    public static GameType CurrentType { get; set; }
}

public class StartPlaySceneController : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;

    private VisualElement root;
    private CustomizableButton singlePlayerButton;
    private CustomizableButton multyPlayerButton;
    private CustomizableButton bestiaryButton;

    private void Start()
    {
        InitializeUI();
        RegisterCallbacks();
    }

    private void InitializeUI()
    {
        root = uiDocument.rootVisualElement;
        singlePlayerButton = root.Q<CustomizableButton>("singlePlayerButton");
        multyPlayerButton = root.Q<CustomizableButton>("multyPlayerButton");
        bestiaryButton = root.Q<CustomizableButton>("bestiaryButton");
    }

    private void RegisterCallbacks()
    {
        singlePlayerButton.RegisterCallback<ClickEvent>(evt =>
        {
            GameTypeConfig.CurrentType = GameType.Bot;
            SceneSwitcher.SwitchScene("DeckSelectionScene", root);
        });

        multyPlayerButton.RegisterCallback<ClickEvent>(evt =>
        {
            GameTypeConfig.CurrentType = GameType.Multiplayer;
            SceneManager.LoadScene("RoomManagerScene");
        });

        bestiaryButton.RegisterCallback<ClickEvent>(evt =>
        {
            SceneContext.PreviousMenuSceneName = SceneManager.GetActiveScene().name;
            SceneSwitcher.SwitchScene("BestiaryScene", root);
        });
    }
}
