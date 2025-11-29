using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class DeckElementController : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private VisualTreeAsset deckElementTemplate;
    [SerializeField] private DeckEditorController editorController;

    private ScrollView decksScrollView;
    private VisualElement deckRoot;
    private VisualElement deckElement;
    private Label deckName;
    private Button addDeckButton;
    private Button deleteDeckButton;

    private readonly Dictionary<string, Label> deckLabels = new();

    private void Start()
    {
        VisualElement root = uiDocument.rootVisualElement;
        decksScrollView = root.Q<ScrollView>("deckScrollView");
        addDeckButton = root.Q<Button>("addDeckButton");

        AddDecksToContainer(UserSession.Instance.ActiveUser.decks);

        addDeckButton.clicked += OnAddButtonClicked;

        RegisterEvent();
    }

    private void OnAddButtonClicked()
    {
        Deck newDeck = new()
        {
            name = "New deck",
            cards = new List<int>()
        };

        editorController.SetSaveChangesButtonText("Сохранить колоду");
        editorController.OpenDeckEditor(newDeck);
    }

    private void AddDecksToContainer(List<Deck> decks)
    {
        decksScrollView.Clear();
        deckLabels.Clear();

        foreach (Deck deck in decks)
        {
            AddDeckToContainer(deck);
        }
    }

    private void AddDeckToContainer(Deck deck)
    {
        deckRoot = deckElementTemplate.Instantiate();
        deckElement = deckRoot.Q<VisualElement>("deckElement");
        deleteDeckButton = deckRoot.Q<Button>("deleteDeckButton");

        deckName = deckRoot.Q<Label>("deckName");
        deckName.text = deck.name;
        deckLabels[deck.deckId] = deckName;

        deckElement.RegisterCallback<ClickEvent>(evt =>
        {
            editorController.SetSaveChangesButtonText("Сохранить изменения");
            editorController.OpenDeckEditor(deck);
        });

        deleteDeckButton.RegisterCallback<ClickEvent>(evt =>
        {
            _ = FirebaseFirestoreService.Instance.DeleteDeck(UserSession.Instance.ActiveUser, deck);
            AddDecksToContainer(UserSession.Instance.ActiveUser.decks);
        });

        decksScrollView.Add(deckRoot);
    }

    private void RegisterEvent()
    {
        editorController.OnDeckUpdate += deck =>
        {
            if (deckLabels.TryGetValue(deck.deckId, out Label label))
                label.text = deck.name;
        };

        editorController.OnDeckAdded += newDeck =>
        {
            AddDeckToContainer(newDeck);
        };
    }
}
