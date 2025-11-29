using UnityEngine.UIElements;
using System;
using UnityEngine;
using System.Collections.Generic;

public class CardControllerFactory
{
    private static VisualTreeAsset uxmlTemplate;
    private static GameObject uguiPrefab;
    private static readonly Dictionary<Type, Func<CardModel, Transform, bool, BaseCardController>> registry = new();

    public static void Init(VisualTreeAsset template = null, GameObject prefab = null)
    {
        uxmlTemplate = template;
        uguiPrefab = prefab;

        registry[typeof(CollectionCardController)] = (model, parent, faceDown) => CreateCollection(model);
        registry[typeof(BattleCardController)] = (model, parent, faceDown) => CreateBattle(model, parent, faceDown);
        // registry[typeof(DeckCardController)] = (model, parent, facedown) => CreateDeck(model);
    }

    public static T Create<T>(CardModel model, Transform parent = null, bool faceDown = false) where T : BaseCardController
    {
        if (registry.TryGetValue(typeof(T), out var factory))
            return (T)factory(model, parent, faceDown);

        throw new NotSupportedException($"CardControllerFactory: Unknown controller type {typeof(T).Name}");
    }

    private static CollectionCardController CreateCollection(CardModel model)
    {
        CardViewUIToolkit view = new(model, uxmlTemplate);
        return new CollectionCardController(model, view);
    }

    private static BattleCardController CreateBattle(CardModel model, Transform uguiParent, bool faceDown)
    {
        CardViewUGUI view = new(model, uguiPrefab, uguiParent, faceDown);
        return new BattleCardController(model, view);
    }

    // private static DeckCardController CreateDeck(CardModel model)
    // {
    //     DeckCardViewUIToolkit view = new(model, uxmlTemplate);
    //     return new DeckCardController(model, view);
    // }
}
