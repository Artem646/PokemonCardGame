using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

public class CardsListController<T> : ICardsListContainer where T : BaseCardController
{
    public readonly List<T> activeCardControllers = new();
    private readonly object cardsContainer;

    public CardsListController(object container)
    {
        if (container is VisualElement || container is Transform)
        {
            cardsContainer = container;
        }
        else
        {
            throw new ArgumentException("Container must be VisualElement (UI Toolkit) or Transform (UGUI)");
        }
    }


    public async Task LoadCardsToContainer(List<CardModel> cards)
    {
        Clear();

        foreach (CardModel model in cards)
        {
            AddCardToContainer(model);
            await Task.Yield();
        }
    }

    public void AddCardToContainer(CardModel model)
    {
        T controller = CardControllerFactory.Create<T>(model);
        activeCardControllers.Add(controller);

        ICardView view = controller.CardView;

        switch (cardsContainer)
        {
            case VisualElement cardContainer:
                if (view is ICollectionCardView collectionView)
                    cardContainer.Add(collectionView.CardRootUIToolkit);
                break;

            case Transform handContainer:
                if (view is IBattleCardView battleView)
                {
                    battleView.CardRootGameObject.transform.SetParent(handContainer, false);
                }
                break;
        }
    }

    public void Clear()
    {
        foreach (var controller in activeCardControllers)
        {
            ICardView view = controller.CardView;

            switch (cardsContainer)
            {
                case VisualElement:
                    view.CardRootUIToolkit?.RemoveFromHierarchy();
                    break;
                case Transform:
                    if (view.CardRootGameObject != null)
                        UnityEngine.Object.Destroy(view.CardRootGameObject);
                    break;
            }
        }
        activeCardControllers.Clear();
    }
}
