using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System;
using System.Linq;

public class DeckEditorController : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private VisualTreeAsset cardTemplate;

    private VisualElement root;
    private VisualElement overlay;
    private TextField deckTitleTextField;
    private ScrollView deckCardsScrollView;
    private Button performActionButton;
    private Button closeDeckEditorButton;

    private DeckEditorCardListController deckCardListController;
    private Deck currentDeck;
    private List<int> selectedCards = new();
    private const int MAX_CARDS_COUNT = 5;

    public event Action<Deck> OnDeckMaked;
    public event Action<Deck> OnDeckAdded;
    public event Action<Deck> OnDeckUpdate;

    private void Start()
    {
        InitializeUI();

        deckCardListController = new DeckEditorCardListController(deckCardsScrollView, cardTemplate, selectedCards, this);
        _ = deckCardListController.LoadUserCardsToDeckScrollView();

        closeDeckEditorButton.clicked += OnCloseDeckEditor;
        performActionButton.clicked += OnPerformAction;
    }

    private void InitializeUI()
    {
        root = uiDocument.rootVisualElement;
        overlay = root.Q<VisualElement>("overlay");
        deckTitleTextField = root.Q<TextField>("deckTitle");
        deckCardsScrollView = root.Q<ScrollView>("deckCardsScrollView");
        closeDeckEditorButton = root.Q<Button>("closeDeckEditorButton");
        performActionButton = root.Q<Button>("performActionButton");
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

        overlay.style.display = DisplayStyle.Flex;
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

            NotificationManager.ShowNotification($"В колоде должно быть ровно {MAX_CARDS_COUNT} карт!", NotificationType.Info);

            return;
        }

        controller?.SetSelected(isSelected);
    }

    private void OnCloseDeckEditor()
    {
        selectedCards.Clear();
        currentDeck = null;
        overlay.style.display = DisplayStyle.None;
    }

    private async void OnPerformAction()
    {
        if (currentDeck == null) return;

        if (selectedCards.Count != MAX_CARDS_COUNT)
        {
            NotificationManager.ShowNotification($"В колоде должно быть ровно {MAX_CARDS_COUNT} карт!", NotificationType.Info);
            return;
        }

        string newName = deckTitleTextField.value;
        List<int> newCards = new(selectedCards);

        bool nameChanged = !string.Equals(currentDeck.name, newName, StringComparison.Ordinal);
        bool cardsChanged = !currentDeck.cards.SequenceEqual(selectedCards);

        if (performActionButton.text == "Сохранить изменения")
        {
            overlay.style.display = DisplayStyle.None;

            if (nameChanged || cardsChanged)
            {
                currentDeck.name = newName;
                currentDeck.cards = newCards;

                await FirebaseFirestoreService.Instance.UpdateDeck(UserSession.Instance.ActiveUser, currentDeck);
                OnDeckUpdate?.Invoke(currentDeck);
                NotificationManager.ShowNotification("Изменения сохранены!", NotificationType.Success);
            }
            else
            {
                NotificationManager.ShowNotification("Нет изменений", NotificationType.Info);
            }
        }

        if (performActionButton.text == "Сохранить колоду")
        {
            overlay.style.display = DisplayStyle.None;

            currentDeck.name = newName;
            currentDeck.cards = newCards;

            await FirebaseFirestoreService.Instance.AddDeck(UserSession.Instance.ActiveUser, currentDeck);
            OnDeckAdded?.Invoke(currentDeck);
        }

        if (performActionButton.text == "Играть")
        {
            currentDeck.name = newName;
            currentDeck.cards = newCards;

            OnDeckMaked?.Invoke(currentDeck);
        }
    }

    public void SetSaveChangesButtonText(string text)
    {
        performActionButton.text = text;
    }
}
