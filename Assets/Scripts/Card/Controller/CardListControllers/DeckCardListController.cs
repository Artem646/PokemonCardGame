using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.UIElements;

public class DeckCardListController : CardListController<DeckCardController>
{
    private readonly VisualTreeAsset deckCardTemplate;
    private readonly List<int> selectedCards;
    private readonly DeckEditorController editorController;

    public DeckCardListController(VisualElement container, VisualTreeAsset template, List<int> selectedCards, DeckEditorController editor)
        : base(container)
    {
        deckCardTemplate = template;
        this.selectedCards = selectedCards;
        editorController = editor;
    }

    protected override DeckCardController CreateController(CardModel model)
    {
        DeckCardViewUIToolkit view = new(model, deckCardTemplate);
        bool initiallySelected = selectedCards.Contains(model.id);
        DeckCardController controller = new(model, view, initiallySelected);
        controller.OnCardSelectionChanged += editorController.HandleCardSelectionChanged;
        return controller;
    }

    protected override void OnCardAdded(DeckCardController controller)
    {
        if (cardContainer is VisualElement deckEditorContainer)
            deckEditorContainer.Add(controller.DeckCardView.CardRoot);
    }

    public async Task LoadUserCardsToDeckScrollView()
    {
        Clear();
        UserCardModelList userCards = CardRepository.Instance.GetUserCards();
        await LoadCardsToContainer(userCards.cards);
    }
}
