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
        List<int> deckCardIds = SelectedDeckManager.SelectedDeck.cards;
        var deckCards = userCards.cards.Where(card => deckCardIds.Contains(card.id)).ToList();
        await LoadCardsToContainer(deckCards);
    }

    public async Task LoadPlayerCards()
    {
        Clear();
        UserCardModelList userCards = CardRepository.Instance.GetUserCards();
        List<int> deckCardIds = SelectedDeckManager.SelectedDeck.cards;
        var deckCards = userCards.cards.Where(card => deckCardIds.Contains(card.id)).ToList();
        await LoadCardsToContainer(deckCards);
    }

    protected override BattleCardController CreateController(CardModel model)
    {
        Transform handContainer = cardContainer as Transform;
        bool faceDown = handContainer.name == "EnemyHand";
        return CardControllerFactory.Create<BattleCardController>(model, handContainer, faceDown);
    }

    protected override void OnCardAdded(BattleCardController controller)
    {
        if (cardContainer is Transform handContainer)
            controller.CardView.CardRootGameObject.transform.SetParent(handContainer, false);
    }
}
