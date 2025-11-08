using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.UIElements;

public class BattleFiledCardsListController : CardsListController<BattleCardController>
{
    public BattleFiledCardsListController(VisualElement container)
        : base(container) { }

    public async Task LoadUserCards()
    {
        Clear();
        UserCardsModelList userCards = CardRepository.Instance.GetUserCards();
        List<int> deckCardIds = CardDeck.Instance.GetDeckCards();

        var deckCards = userCards.cards.Where(card => deckCardIds.Contains(card.id)).ToList();

        await LoadCardsToContainer(deckCards);
    }
}
