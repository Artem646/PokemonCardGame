using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class BattleCardListController : CardListController<BattleCardController>
{
    public BattleCardListController(Transform container)
        : base(container) { }

    public async Task LoadEnemyCards()
    {
        Clear();
        UserCardModelList userCards = CardRepository.Instance.GetUserCards();
        List<int> deckCardIds = CardDeck.Instance.GetDeckCards();
        var deckCards = userCards.cards.Where(card => deckCardIds.Contains(card.id)).ToList();
        await LoadCardsToContainer(deckCards);
    }

    public async Task LoadPlayerCards()
    {
        Clear();
        UserCardModelList userCards = CardRepository.Instance.GetUserCards();
        List<int> deckCardIds = CardDeck.Instance.GetDeckCards();
        var deckCards = userCards.cards.Where(card => deckCardIds.Contains(card.id)).ToList();
        await LoadCardsToContainer(deckCards);
    }
}
