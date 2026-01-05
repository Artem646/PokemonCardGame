using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System;
using System.Linq;

public enum DeckEditorAction
{
    SaveChanges,
    SaveDeck,
    Play
}

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

    private DeckEditorAction currentAction;

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

            Localizer.LocalizeNotification(NotificationKey.DeckMustHaveCountCards, NotificationType.Info, MAX_CARDS_COUNT);
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
            Localizer.LocalizeNotification(NotificationKey.DeckMustHaveCountCards, NotificationType.Info, MAX_CARDS_COUNT);
            return;
        }

        string newName = deckTitleTextField.value;
        List<int> newCards = new(selectedCards);

        bool nameChanged = !string.Equals(currentDeck.name, newName, StringComparison.Ordinal);
        bool cardsChanged = !currentDeck.cards.SequenceEqual(selectedCards);

        switch (currentAction)
        {
            case DeckEditorAction.SaveChanges:
                {
                    overlay.style.display = DisplayStyle.None;

                    if (nameChanged || cardsChanged)
                    {
                        currentDeck.name = newName;
                        currentDeck.cards = newCards;

                        await FirebaseFirestoreService.Instance.UpdateDeck(UserSession.Instance.ActiveUser, currentDeck);
                        OnDeckUpdate?.Invoke(currentDeck);
                    }
                    else
                        Localizer.LocalizeNotification(NotificationKey.NoChanges, NotificationType.Info);

                    break;
                }
            case DeckEditorAction.SaveDeck:
                {
                    overlay.style.display = DisplayStyle.None;

                    currentDeck.name = newName;
                    currentDeck.cards = newCards;

                    await FirebaseFirestoreService.Instance.AddDeck(UserSession.Instance.ActiveUser, currentDeck);
                    OnDeckAdded?.Invoke(currentDeck);
                    break;
                }
            case DeckEditorAction.Play:
                {
                    currentDeck.name = newName;
                    currentDeck.cards = newCards;
                    OnDeckMaked?.Invoke(currentDeck);
                    break;
                }
        }
    }

    public void SetDeckEditorAction(DeckEditorAction action)
    {
        currentAction = action;
        UpdatePerformActionButtonText();
    }

    private void UpdatePerformActionButtonText()
    {
        string key = currentAction switch
        {
            DeckEditorAction.SaveChanges => "SaveChangesButton",
            DeckEditorAction.SaveDeck => "SaveDeckButton",
            DeckEditorAction.Play => "PlayWithDeckButton",
            _ => "PlayWithDeckButton"
        };

        Localizer.LocalizeElement(root, "performActionButton", key, "ElementsText");
    }
}
