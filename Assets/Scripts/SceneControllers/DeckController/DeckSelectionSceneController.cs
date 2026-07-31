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

    private VisualElement root;
    private DropdownField deckDropdown;
    private Button confirmButton;
    private Button makeDeckButton;

    private void Start()
    {
        InitializeUI();
        RefreshDeckDropdown();
        RegisterCallbacks();
    }

    private void InitializeUI()
    {
        root = uiDocument.rootVisualElement;
        deckDropdown = root.Q<DropdownField>("deckSelectField");
        confirmButton = root.Q<Button>("confirmDeckButton");
        makeDeckButton = root.Q<Button>("makeDeckButton");
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
            LocalizedString localizedValue = new("ElementsText", "EmptyDropdown");
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
                SceneManager.LoadScene("GameLoadingScene");
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
                SceneManager.LoadScene("GameLoadingScene");
            };
        });

        SceneManager.activeSceneChanged += (oldScene, newScene) =>
        {
            if (newScene.name == "DeckSelectionScene")
                RefreshDeckDropdown();
        };
    }
}
