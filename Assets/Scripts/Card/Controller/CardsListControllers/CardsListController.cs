using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.UIElements;

public class CardsListController<T> : ICardsListContainer where T : BaseCardController
{
    public readonly VisualElement cardsContainer;
    public readonly List<T> activeCardControllers = new();

    public CardsListController(VisualElement container)
    {
        cardsContainer = container;
    }

    public async Task LoadCardsToContainer(List<CardModel> cards)
    {
        foreach (CardModel card in cards)
        {
            AddCardToContainer(card);
            await Task.Yield();
        }
    }

    public void AddCardToContainer(CardModel card)
    {
        T controller = (T)CardControllerBuilder.CreateCardController<T>(card);
        activeCardControllers.Add(controller);
        cardsContainer.Add(controller.CardView.CardRoot);
    }

    // public void RefreshAllCards()
    // {
    //     cardsContainer.Clear();
    //     foreach (T controller in activeCardControllers)
    //     {
    //         cardsContainer.Add(controller.CardView.CardRoot);
    //     }
    // }

    public void Clear()
    {
        activeCardControllers.Clear();
        cardsContainer.Clear();
    }

    // public List<T> GetAllControllers() => cardControllers;
}
