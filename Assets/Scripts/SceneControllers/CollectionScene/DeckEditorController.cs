using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System;

public class DeckEditorController : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private VisualTreeAsset cardTemplate;
    private VisualElement root;
    private DeckCardListController deckCardListController;
    private TextField deckTitleTextField;
    private ScrollView deckCardsScrollView;
    private Button performActionButton;
    private Button closeDeckEditorButton;

    private Deck currentDeck;
    private List<int> selectedCards = new();
    private const int MAX_CARDS_COUNT = 5;

    public event Action<Deck> OnDeckMaked;
    public event Action<Deck> OnDeckAdded;
    public event Action<Deck> OnDeckUpdate;

    private void Start()
    {
        root = uiDocument.rootVisualElement;
        root.style.display = DisplayStyle.None;
        deckTitleTextField = root.Q<TextField>("deckTitle");
        deckCardsScrollView = root.Q<ScrollView>("deckCardsScrollView");
        closeDeckEditorButton = root.Q<Button>("closeDeckEditorButton");
        performActionButton = root.Q<Button>("performActionButton");

        deckCardListController = new DeckCardListController(deckCardsScrollView, cardTemplate, selectedCards, this);
        _ = deckCardListController.LoadUserCardsToDeckScrollView();

        closeDeckEditorButton.clicked += OnCloseDeckEditor;
        performActionButton.clicked += OnPerformAction;
    }

    public void OpenDeckEditor(Deck deck)
    {
        currentDeck = deck;
        selectedCards = new List<int>(deck.cards);
        deckTitleTextField.value = deck.name;

        foreach (DeckCardController controller in deckCardListController.CardControllers)
        {
            bool inDeck = selectedCards.Contains(controller.CardModel.id);
            controller.DeckCardView.SetSelected(inDeck);
        }

        root.style.display = DisplayStyle.Flex;
    }

    public void HandleCardSelectionChanged(int cardId, bool isSelected)
    {
        if (isSelected)
        {
            if (selectedCards.Count >= MAX_CARDS_COUNT)
            {
                DeckCardController controller = deckCardListController.CardControllers.Find(card => card.CardModel.id == cardId);
                controller?.DeckCardView.SetSelected(false);
                return;
            }

            if (!selectedCards.Contains(cardId))
                selectedCards.Add(cardId);
        }
        else
        {
            selectedCards.Remove(cardId);
        }
    }

    private void OnCloseDeckEditor()
    {
        selectedCards.Clear();
        currentDeck = null;
        root.style.display = DisplayStyle.None;
    }

    private async void OnPerformAction()
    {
        if (currentDeck == null) return;

        currentDeck.name = deckTitleTextField.value;
        currentDeck.cards = new List<int>(selectedCards);

        if (performActionButton.text == "Сохранить изменения")
        {
            await FirebaseFirestoreService.Instance.UpdateDeck(UserSession.Instance.ActiveUser, currentDeck);
            root.style.display = DisplayStyle.None;
            OnDeckUpdate?.Invoke(currentDeck);
        }

        if (performActionButton.text == "Сохранить колоду")
        {
            await FirebaseFirestoreService.Instance.AddDeck(UserSession.Instance.ActiveUser, currentDeck);
            root.style.display = DisplayStyle.None;
            OnDeckAdded?.Invoke(currentDeck);
        }

        if (performActionButton.text == "Играть")
        {
            OnDeckMaked?.Invoke(currentDeck);
        }
    }

    public void SetSaveChangesButtonText(string text)
    {
        performActionButton.text = text;
    }
}
