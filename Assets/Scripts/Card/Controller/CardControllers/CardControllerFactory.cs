using UnityEngine.UIElements;
using System;
using UnityEngine;
using System.Collections.Generic;

public class CardControllerFactory
{
    private static VisualTreeAsset uxmlTemplate;
    private static GameObject uguiPrefab;
    private static readonly Dictionary<Type, Func<CardModel, Transform, bool, ICardController>> registry = new();

    public static void Init(VisualTreeAsset template = null, GameObject prefab = null)
    {
        uxmlTemplate = template;
        uguiPrefab = prefab;

        registry[typeof(CollectionCardController)] = (model, parent, faceDown) => CreateCollection(model);
        registry[typeof(BattleCardController)] = (model, parent, faceDown) => CreateBattle(model, parent, faceDown);
    }

    public static T Create<T>(CardModel model, Transform parent = null, bool faceDown = false) where T : ICardController
    {
        if (registry.TryGetValue(typeof(T), out var factory))
            return (T)factory(model, parent, faceDown);

        throw new NotSupportedException($"CardControllerFactory: Unknown controller type {typeof(T).Name}");
    }

    private static CollectionCardController CreateCollection(CardModel model)
    {
        CollectionCardView view = new(model, uxmlTemplate);
        return new CollectionCardController(model, view);
    }

    private static BattleCardController CreateBattle(CardModel model, Transform uguiParent, bool faceDown)
    {
        BattleCardView view = new(model, uguiPrefab, uguiParent, faceDown);
        return new BattleCardController(model, view);
    }
}
