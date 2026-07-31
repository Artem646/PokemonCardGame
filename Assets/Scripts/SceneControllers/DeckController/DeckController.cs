using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class DeckController : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private VisualTreeAsset deckTemplate;
    [SerializeField] private DeckEditorController editorController;
    [SerializeField] private VisualTreeAsset deckCardTemplate;

    private DeckCardListController deckCardListController;

    private VisualElement root;
    private ScrollView decksScrollView;
    private VisualElement deckRoot;
    private Label deckNameLabel;
    private Button editDeckButton;
    private Button deleteDeckButton;
    private VisualElement deckCardsContainer;

    private void Start()
    {
        InitializeUI();
        AddDecksToContainer(UserSession.Instance.ActiveUser.decks);
        RegisterEvent();
    }

    private void InitializeUI()
    {
        root = uiDocument.rootVisualElement;
        decksScrollView = root.Q<ScrollView>("decksScrollView");
    }

    private void AddDecksToContainer(List<Deck> decks)
    {
        decksScrollView.Clear();
        foreach (Deck deck in decks)
            AddDeckToContainer(deck);
    }

    private void AddDeckToContainer(Deck deck)
    {
        deckRoot = deckTemplate.Instantiate();
        deckRoot.userData = deck.deckId;

        InitializeDeckUI();

        deckRoot.style.width = new StyleLength(new Length(100, LengthUnit.Percent));
        deckNameLabel.text = deck.name;

        deckCardsContainer.Clear();

        CardControllerFactory.Init(template: deckCardTemplate);
        deckCardListController = new DeckCardListController(deckCardsContainer);
        _ = deckCardListController.LoadCardsToDeckContainer(deck);

        RegisterCallbacks(deck);

        decksScrollView.Add(deckRoot);
    }

    private void InitializeDeckUI()
    {
        deckNameLabel = deckRoot.Q<Label>("deckNameLabel");
        editDeckButton = deckRoot.Q<Button>("editDeckButton");
        deleteDeckButton = deckRoot.Q<Button>("deleteDeckButton");
        deckCardsContainer = deckRoot.Q<VisualElement>("deckCardsContainer");
    }

    private void RegisterCallbacks(Deck deck)
    {
        editDeckButton.RegisterCallback<ClickEvent>(evt =>
        {
            editorController.SetDeckEditorAction(DeckEditorAction.SaveChanges);
            editorController.OpenDeckEditor(deck);
        });

        deleteDeckButton.RegisterCallback<ClickEvent>(evt =>
        {
            _ = FirebaseFirestoreService.Instance.DeleteDeck(UserSession.Instance.ActiveUser, deck);
            VisualElement deckRootToRemove = decksScrollView.Children().FirstOrDefault(child => (string)child.userData == deck.deckId);
            if (deckRootToRemove != null)
                decksScrollView.Remove(deckRootToRemove);
        });
    }

    private void RegisterEvent()
    {
        editorController.OnDeckAdded += newDeck =>
        {
            AddDeckToContainer(newDeck);
        };

        editorController.OnDeckUpdated += deck =>
        {
            deckRoot = decksScrollView.Children().FirstOrDefault(child => (string)child.userData == deck.deckId);
            deckNameLabel = deckRoot.Q<Label>("deckNameLabel");
            deckCardsContainer = deckRoot.Q<VisualElement>("deckCardsContainer");

            deckNameLabel.text = deck.name;
            deckCardsContainer.Clear();
            deckCardListController = new DeckCardListController(deckCardsContainer);
            _ = deckCardListController.LoadCardsToDeckContainer(deck);
        };
    }
}
