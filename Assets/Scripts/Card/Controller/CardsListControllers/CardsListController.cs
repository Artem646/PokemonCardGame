using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

public class CardsListController<T, V> : ICardsListContainer where T : BaseCardController where V : class, ICardView
{
    // public readonly VisualElement cardsContainer;
    public readonly List<T> activeCardControllers = new();
    private readonly object container;

    // public CardsListController(VisualElement container)
    // {
    //     cardsContainer = container;
    // }

    public CardsListController(object container)
    {
        if (container is VisualElement || container is Transform)
        {
            this.container = container;
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

        // V view = controller.GetView<V>();

        // switch (container)
        // {
        //     case VisualElement cardContainer:
        //         cardContainer.Add(view.CardRootUIToolkit;);
        //         break;
        //     case Transform cardContainer:
        //         view.CardRootGameObject.transform.SetParent(cardContainer, false);
        //         break;
        // }

        switch (container)
        {
            case VisualElement ve:
                if (view is ICollectionCardView collectionView)
                {
                    ve.Add(collectionView.CardRootUIToolkit);
                }
                break;

            case Transform parent:
                if (view is IBattleCardView battleView)
                {
                    battleView.CardRootGameObject.transform.SetParent(parent, false);
                }
                break;
        }

        // cardsContainer.Add(controller.CardView.CardRootUIToolkit);
    }

    public void Clear()
    {
        foreach (var controller in activeCardControllers)
        {
            V view = controller.GetView<V>();

            switch (container)
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

    // public List<T> GetAllControllers() => cardControllers;
}
