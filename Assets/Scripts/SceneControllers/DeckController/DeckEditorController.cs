using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System;

public class DeckEditorController : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private VisualTreeAsset cardTemplate;
    private VisualElement root;
    private DeckEditorCardListController deckCardListController;
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

        deckCardListController = new DeckEditorCardListController(deckCardsScrollView, cardTemplate, selectedCards, this);
        _ = deckCardListController.LoadUserCardsToDeckScrollView();

        closeDeckEditorButton.clicked += OnCloseDeckEditor;
        performActionButton.clicked += OnPerformAction;
    }

    public void OpenDeckEditor(Deck deck)
    {
        currentDeck = deck;
        selectedCards = new List<int>(deck.cards);
        deckTitleTextField.value = deck.name;

        foreach (DeckEditorCardController controller in deckCardListController.CardControllers)
        {
            bool inDeck = selectedCards.Contains(controller.CardModel.id);
            controller.SetSelected(inDeck);
        }

        root.style.display = DisplayStyle.Flex;
    }

    public void HandleCardSelectionChanged(int cardId, bool isSelected)
    {
        DeckEditorCardController controller = deckCardListController.CardControllers.Find(card => card.CardModel.id == cardId);

        if (isSelected)
        {
            if (!selectedCards.Contains(cardId))
                selectedCards.Add(cardId);
        }
        else
        {
            selectedCards.Remove(cardId);
        }

        if (selectedCards.Count > MAX_CARDS_COUNT)
        {
            controller?.SetSelected(false);
            selectedCards.Remove(cardId);
            return;
        }

        controller?.SetSelected(isSelected);
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

        if (selectedCards.Count != MAX_CARDS_COUNT)
            return;

        currentDeck.name = deckTitleTextField.value;
        currentDeck.cards = new List<int>(selectedCards);

        if (performActionButton.text == "Сохранить изменения")
        {
            root.style.display = DisplayStyle.None;
            await FirebaseFirestoreService.Instance.UpdateDeck(UserSession.Instance.ActiveUser, currentDeck);
            OnDeckUpdate?.Invoke(currentDeck);
        }

        if (performActionButton.text == "Сохранить колоду")
        {
            root.style.display = DisplayStyle.None;
            await FirebaseFirestoreService.Instance.AddDeck(UserSession.Instance.ActiveUser, currentDeck);
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
