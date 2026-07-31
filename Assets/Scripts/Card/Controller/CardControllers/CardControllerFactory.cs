using UnityEngine.UIElements;
using System;
using UnityEngine;
using System.Collections.Generic;

public class CardControllerFactory
{
    private static VisualTreeAsset uxmlTemplate;
    private static GameObject uguiPrefab3D;
    private static readonly Dictionary<Type, Func<CardModel, ICardController>> registry = new();

    public static void Init(VisualTreeAsset template = null, GameObject prefab3D = null)
    {
        uxmlTemplate = template;
        uguiPrefab3D = prefab3D;

        registry[typeof(CollectionCardController)] = (model) => CreateCollection(model);
        registry[typeof(DeckCardController)] = (model) => CreateDeck(model);
        registry[typeof(BattleCardController)] = (model) => CreateBattle3D(model);
    }

    public static T Create<T>(CardModel model) where T : ICardController
    {
        if (registry.TryGetValue(typeof(T), out var factory))
            return (T)factory(model);

        throw new NotSupportedException($"CardControllerFactory: Unknown controller type {typeof(T).Name}");
    }

    private static CollectionCardController CreateCollection(CardModel model)
    {
        CollectionCardView view = new(model, uxmlTemplate);
        return new CollectionCardController(model, view);
    }

    private static DeckCardController CreateDeck(CardModel model)
    {
        DeckCardView view = new(model, uxmlTemplate);
        return new DeckCardController(model, view);
    }

    private static BattleCardController CreateBattle3D(CardModel model)
    {
        BattleCardView view = new(model, uguiPrefab3D);
        BattleCardController controller = new(model, view);
        if (view.CardRoot.TryGetComponent<CardControllerLink>(out var link))
            link.Controller = controller;
        return controller;
    }
}
