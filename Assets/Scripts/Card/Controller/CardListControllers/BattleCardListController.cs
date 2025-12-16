using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class BattleCardListController : CardListController<BattleCardController>
{
    public BattleCardListController(Transform container)
        : base(container) { }

    public async Task LoadCardsByIds(List<int> ids)
    {
        Clear();
        if (ids == null || ids.Count == 0)
        {
            await Task.Yield();
            return;
        }
        GameCardModelList gameCards = CardRepository.Instance.GetGameCards();
        List<CardModel> cards = gameCards.cards.Where(card => ids.Contains(card.id)).ToList();
        await AddCardsToContainer(cards);
    }

    protected override BattleCardController CreateController(CardModel model)
    {
        Transform handContainer = cardContainer as Transform;
        bool faceDown = handContainer.name == "EnemyHand" || handContainer.name == "EnemyBattleField";
        BattleCardController controller = CardControllerFactory.Create<BattleCardController>(model, handContainer, faceDown);
        if (controller.BattleCardView.CardRoot.TryGetComponent<CardMovemantScript>(out var movement))
            movement.CardId = controller.CardModel.id;
        return controller;
    }

    // protected override void OnCardAdded(BattleCardController controller)
    // {
    //     if (cardContainer is Transform handContainer)
    //         controller.CardView.CardRootGameObject.transform.SetParent(handContainer, false);
    // }
}
