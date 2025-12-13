using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

public class CardListController<T> : ICardListController where T : BaseCardController
{
    public List<T> CardControllers { get; set; } = new();
    public List<CardModel> CardModels { get; set; } = new();

    protected readonly object cardContainer;

    public CardListController(object container)
    {
        if (container is VisualElement || container is Transform)
            cardContainer = container;
    }

    public async Task LoadCardsToContainer(List<CardModel> cards)
    {
        Clear();
        CardModels = cards;
        foreach (CardModel model in cards)
        {
            var controller = CreateController(model);
            CardControllers.Add(controller);
            OnCardAdded(controller);
            await Task.Yield();
        }
    }

    protected virtual T CreateController(CardModel model)
    {
        return CardControllerFactory.Create<T>(model);
    }

    protected virtual void OnCardAdded(T controller)
    {
        switch (cardContainer)
        {
            case VisualElement cardContainer:
                if (controller.CardView is CardViewUIToolkit uiToolkitView)
                    cardContainer.Add(uiToolkitView.CardRootUIToolkit);
                break;

            case Transform handContainer:
                if (controller.CardView is IBattleCardView battleView)
                    battleView.CardRootGameObject.transform.SetParent(handContainer, false);
                break;
        }
    }

    public void Clear()
    {
        foreach (var controller in CardControllers)
        {
            ICardView view = controller.CardView;

            switch (cardContainer)
            {
                case VisualElement:
                    view.CardRootUIToolkit?.RemoveFromHierarchy();
                    break;
                case Transform:
                    if (view.CardRootGameObject != null)
                        Object.Destroy(view.CardRootGameObject);
                    break;
            }
        }
        CardControllers.Clear();
    }
}