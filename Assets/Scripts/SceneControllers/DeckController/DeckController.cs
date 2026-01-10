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
    private Label deckName;
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
        {
            AddDeckToContainer(deck);
        }
    }

    private void AddDeckToContainer(Deck deck)
    {
        deckRoot = deckTemplate.Instantiate();
        deckRoot.userData = deck.deckId;

        InitializeDeckUI();

        deckRoot.style.width = new StyleLength(new Length(100, LengthUnit.Percent));
        deckName.text = deck.name;

        deckCardsContainer.Clear();

        CardControllerFactory.Init(template: deckCardTemplate);
        deckCardListController = new DeckCardListController(deckCardsContainer);
        _ = deckCardListController.LoadCardsToDeckContainer(deck);

        RegisterCallbacks(deck);

        decksScrollView.Add(deckRoot);
    }

    private void InitializeDeckUI()
    {
        deckName = deckRoot.Q<Label>("deckName");
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
        editorController.OnDeckUpdate += deck =>
        {
            VisualElement deckRoot = decksScrollView.Children().FirstOrDefault(child => (string)child.userData == deck.deckId);
            if (deckRoot != null)
            {
                Label label = deckRoot.Q<Label>("deckName");
                label.text = deck.name;

                deckCardsContainer = deckRoot.Q<VisualElement>("deckCardsContainer");
                deckCardsContainer.Clear();

                deckCardListController = null;
                deckCardListController = new DeckCardListController(deckCardsContainer);
                _ = deckCardListController.LoadCardsToDeckContainer(deck);
            }
        };

        editorController.OnDeckAdded += newDeck =>
        {
            AddDeckToContainer(newDeck);
        };
    }
}
