using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class DecksSceneController : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private SettingsController settingsController;
    [SerializeField] private DeckEditorController editorController;

    private VisualElement root;
    private VisualElement profileField;
    private VisualElement cardOverlay;
    private Button addDeckButton;

    private async void Start()
    {
        InitializeUI();

        LocalizeElements();

        CardOverlayManager.Instance.RegisterOverlayVisualElement(SceneManager.GetActiveScene().name, cardOverlay);

        UserProfileView.Instance.SetUIDocument(uiDocument, settingsController);
        await UserProfileView.Instance.LoadUserData();

        RegisterCallbacks();
    }

    private void InitializeUI()
    {
        root = uiDocument.rootVisualElement;
        profileField = root.Q<VisualElement>("profileField");
        cardOverlay = root.Q<VisualElement>("overlay");
        addDeckButton = root.Q<Button>("addDeckButton");
    }

    private void LocalizeElements()
    {
        Localizer.LocalizeElements(root, new[]
        {
            ("playButton", "PlayButton"),
            ("collectionButton", "CollectionButton"),
            ("bestiaryButton", "BestiaryButton"),
            ("decksButton", "DecksButton"),
            ("addDeckButton", "AddDeckButton")
        }, "ElementsText");
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

        addDeckButton.RegisterCallback<ClickEvent>(evt =>
        {
            Deck newDeck = new() { name = "New deck", cards = new List<int>() };
            editorController.SetDeckEditorAction(DeckEditorAction.SaveDeck);
            editorController.OpenDeckEditor(newDeck);
        });
    }
}
