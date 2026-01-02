using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.UIElements;

public class DeckCardListController : CardListController<DeckCardController>
{
    public DeckCardListController(VisualElement container)
        : base(container) { }

    public async Task LoadCardsToDeckContainer(Deck deck)
    {
        Clear();
        UserCardModelList userCards = CardRepository.Instance.GetUserCards();
        List<CardModel> deckCards = userCards.cards.Where(card => deck.cards.Contains(card.id)).ToList();
        await AddCardsToContainer(deckCards);
    }

    protected override DeckCardController CreateController(CardModel cardModel)
    {
        var controller = CardControllerFactory.Create<DeckCardController>(cardModel);
        return controller;
    }
}
