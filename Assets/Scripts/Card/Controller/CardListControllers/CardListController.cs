using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class CardListController<T> : ICardListController where T : ICardController
{
    public List<T> CardControllers { get; private set; } = new();
    public List<CardModel> CardModels { get; private set; } = new();

    protected readonly object cardContainer;

    public CardListController(object container)
    {
        cardContainer = container;
    }

    public async Task AddCardsToContainer(List<CardModel> cards)
    {
        Clear();
        CardModels = cards;
        foreach (CardModel model in CardModels)
        {
            var controller = CreateController(model);
            CardControllers.Add(controller);
            AddToContainer(controller);
            await Task.Yield();
        }
    }

    protected virtual T CreateController(CardModel model)
    {
        return CardControllerFactory.Create<T>(model);
    }

    protected virtual void AddToContainer(T controller)
    {
        controller.AddToContainer(cardContainer);
    }

    protected virtual void OnCardRemoved(T controller)
    {
        controller.RemoveFromContainer();
    }

    public void Clear()
    {
        foreach (var controller in CardControllers)
            controller.RemoveFromContainer();
        CardControllers.Clear();
    }
}