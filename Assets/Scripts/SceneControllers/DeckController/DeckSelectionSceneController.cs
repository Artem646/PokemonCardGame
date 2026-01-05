using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class DeckSelectionSceneController : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private DeckEditorController editorController;
    [SerializeField] private SettingsController settingsController;

    private VisualElement root;
    private DropdownField deckDropdown;
    private Button confirmButton;
    private Button makeDeckButton;
    private VisualElement profileField;

    private async void Start()
    {
        InitializeUI();

        LocalizeElements();

        UserProfileView.Instance.SetUIDocument(uiDocument, settingsController);
        await UserProfileView.Instance.LoadUserData();

        RefreshDeckDropdown();
        RegisterCallbacks();
    }

    private void InitializeUI()
    {
        root = uiDocument.rootVisualElement;
        deckDropdown = root.Q<DropdownField>("deckSelectField");
        confirmButton = root.Q<Button>("confirmDeckButton");
        makeDeckButton = root.Q<Button>("makeDeckButton");
        profileField = root.Q<VisualElement>("profileField");
    }

    private void LocalizeElements()
    {
        Localizer.LocalizeElements(root, new[]
        {
            ("playButton", "PlayButton"),
            ("collectionButton", "CollectionButton"),
            ("bestiaryButton", "BestiaryButton"),
            ("decksButton", "DecksButton"),
            ("makeDeckLabel", "MakeDeckLabel"),
            ("selectDeckLabel", "SelectDeckLabel"),
            ("makeDeckButton", "MakeDeckButton"),
            ("confirmDeckButton", "ConfirmDeckButton")
        }, "ElementsText");
    }

    public void RefreshDeckDropdown()
    {
        User user = UserSession.Instance.ActiveUser;
        if (user != null && user.decks.Count > 0)
        {
            deckDropdown.choices = user.decks.Select(deck => deck.name).ToList();
            deckDropdown.value = deckDropdown.choices.FirstOrDefault();
            confirmButton.SetEnabled(true);
        }
        else
        {
            LocalizedString localizedValue = new("MenuElementsText", "EmptyDropdown");
            localizedValue.StringChanged += (str) =>
            {
                deckDropdown.choices = new() { str };
                deckDropdown.value = str;
            };
            confirmButton.SetEnabled(false);
        }
    }

    private void RegisterCallbacks()
    {
        confirmButton.RegisterCallback<ClickEvent>(evt =>
        {
            User user = UserSession.Instance.ActiveUser;
            string selectedDeckName = deckDropdown.value;
            Deck selectedDeck = user.decks.FirstOrDefault(deck => deck.name == selectedDeckName);

            if (selectedDeck != null)
            {
                SelectedDeckManager.SetSelectedDeck(selectedDeck);
                ProcessGameMode();
            }
        });

        makeDeckButton.RegisterCallback<ClickEvent>(evt =>
        {
            Deck deck = new()
            {
                name = "New deck",
                cards = new List<int>()
            };

            editorController.SetDeckEditorAction(DeckEditorAction.Play);
            editorController.OpenDeckEditor(deck);

            editorController.OnDeckMaked += makedDeck =>
            {
                SelectedDeckManager.SetSelectedDeck(makedDeck);
                ProcessGameMode();
            };
        });

        root.Q<Button>("playButton").RegisterCallback<ClickEvent>(evt =>
        {
            SceneSwitcher.SwitchScene("StartPlayScene", root);
        });

        root.Q<Button>("collectionButton").RegisterCallback<ClickEvent>(evt =>
        {
            SceneSwitcher.SwitchScene("CollectionScene", root);
        });

        root.Q<Button>("bestiaryButton").RegisterCallback<ClickEvent>(evt =>
        {
            SceneContext.PreviousMenuSceneName = SceneManager.GetActiveScene().name;
            SceneSwitcher.SwitchScene("BestiaryScene", root);
        });

        root.Q<Button>("decksButton").RegisterCallback<ClickEvent>(evt =>
        {
            SceneSwitcher.SwitchScene("DecksScene", root);
        });

        profileField.RegisterCallback<ClickEvent>(evt =>
        {
            settingsController.OpenSettings();
        });

        SceneManager.activeSceneChanged += (oldScene, newScene) =>
        {
            if (newScene.name == "DeckSelectionScene")
                RefreshDeckDropdown();
        };
    }

    private void ProcessGameMode()
    {
        if (GameModeConfig.IsMultiplayer)
        {
            SceneManager.LoadScene("RoomSelectionScene");
        }
        else
        {
            SceneManager.LoadScene("PlayingScene");
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        GameManagerScript gameManager = FindAnyObjectByType<GameManagerScript>();
        if (gameManager != null)
        {
            BotGameState botState = new GameObject("BotGameState").AddComponent<BotGameState>();
            botState.BindGameManager(gameManager);
            botState.StartGame();
        }
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
