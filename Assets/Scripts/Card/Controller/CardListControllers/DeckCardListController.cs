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

    public async Task LoadUserCardsToDeckScrollView()
    {
        Clear();
        UserCardModelList userCards = CardRepository.Instance.GetUserCards();
        await AddCardsToContainer(userCards.cards);
    }

    protected override DeckCardController CreateController(CardModel cardModel)
    {
        DeckCardView view = new(cardModel, deckCardTemplate);
        bool selected = selectedCards.Contains(cardModel.id);
        DeckCardController controller = new(cardModel, view, selected);
        controller.OnCardSelectionChanged += editorController.HandleCardSelectionChanged;
        return controller;
    }
}
