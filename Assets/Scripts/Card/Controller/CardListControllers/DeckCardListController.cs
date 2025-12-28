using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.UIElements;

public class DeckCardListController : CardListController<DeckCardController>
{
    private readonly VisualTreeAsset deckCardTemplate;

    public DeckCardListController(VisualElement container, VisualTreeAsset template)
        : base(container)
    {
        deckCardTemplate = template;
    }

    public async Task LoadCardsToDeckContainer(Deck deck)
    {
        Clear();
        UserCardModelList userCards = CardRepository.Instance.GetUserCards();
        List<CardModel> deckCards = userCards.cards.Where(card => deck.cards.Contains(card.id)).ToList();
        await AddCardsToContainer(deckCards);
    }

    protected override DeckCardController CreateController(CardModel cardModel)
    {
        DeckCardView view = new(cardModel, deckCardTemplate);
        DeckCardController controller = new(cardModel, view);
        return controller;
    }
}
