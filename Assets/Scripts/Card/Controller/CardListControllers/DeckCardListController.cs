using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.UIElements;

public class DeckCardListController : CardListController<DeckCardController>
{
    public DeckCardListController(VisualElement container)
        : base(container) { }

    public async Task LoadCardsToDeckContainer(Deck deck)
    {
        Clear();

        List<CardModel> result = new();
        foreach (int id in deck.cards)
        {
            CardModel model = CardRepository.Instance.GetGameCardModelById(id);
            if (model != null) result.Add(model);
        }

        await AddCardsToContainer(result);
    }

    protected override DeckCardController CreateController(CardModel cardModel)
    {
        DeckCardController controller = CardControllerFactory.Create<DeckCardController>(cardModel);
        return controller;
    }
}
