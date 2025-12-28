using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.UIElements;

public class DeckEditorCardListController : CardListController<DeckEditorCardController>
{
    private readonly VisualTreeAsset deckCardTemplate;
    private readonly List<int> selectedCards;
    private readonly DeckEditorController editorController;

    public DeckEditorCardListController(VisualElement container, VisualTreeAsset template, List<int> selectedCards, DeckEditorController editor)
        : base(container)
    {
        deckCardTemplate = template;
        this.selectedCards = selectedCards;
        editorController = editor;
    }

    public async Task LoadUserCardsToDeckScrollView()
    {
        Clear();
        UserCardModelList userCards = CardRepository.Instance.GetUserCards();
        await AddCardsToContainer(userCards.cards);
    }

    protected override DeckEditorCardController CreateController(CardModel cardModel)
    {
        DeckCardView view = new(cardModel, deckCardTemplate);
        bool selected = selectedCards.Contains(cardModel.id);
        DeckEditorCardController controller = new(cardModel, view, selected);
        controller.OnCardSelectionChanged += editorController.HandleCardSelectionChanged;
        return controller;
    }
}
