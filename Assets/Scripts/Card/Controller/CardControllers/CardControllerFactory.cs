using UnityEngine.UIElements;
using System;
using UnityEngine;
using System.Collections.Generic;

public class CardControllerFactory
{
    private static VisualTreeAsset uxmlTemplate;
    private static GameObject uguiPrefab;
    // private static Transform uguiParent;
    private static readonly Dictionary<Type, Func<CardModel, Transform, bool, BaseCardController>> registry = new();

    // public static void Init(VisualTreeAsset template = null, GameObject prefab = null, Transform parent = null)
    public static void Init(VisualTreeAsset template = null, GameObject prefab = null)

    {
        uxmlTemplate = template;
        uguiPrefab = prefab;
        // uguiParent = parent;

        registry[typeof(CollectionCardController)] = (model, parent, faceDown) => CreateCollection(model);
        registry[typeof(BattleCardController)] = (model, parent, faceDown) => CreateBattle(model, parent, faceDown);
    }

    public static T Create<T>(CardModel model, Transform parent = null, bool faceDown = false) where T : BaseCardController
    {
        if (registry.TryGetValue(typeof(T), out var factory))
            return (T)factory(model, parent, faceDown);

        // Type t = typeof(T);

        // if (t == typeof(CollectionCardController))
        //     return (T)(BaseCardController)CreateCollection(model);

        // if (t == typeof(BattleCardController))
        //     return (T)(BaseCardController)CreateBattle(model);

        throw new NotSupportedException($"CardControllerFactory: Unknown controller type {typeof(T).Name}");
    }

    private static CollectionCardController CreateCollection(CardModel model)
    {
        CardViewUIToolkit view = new(model, uxmlTemplate);
        return new CollectionCardController(model, view);
    }

    private static BattleCardController CreateBattle(CardModel model, Transform uguiParent, bool faceDown)
    {
        // bool faceDown = false;
        // DropPlaceScript dropPlace = uguiParent.GetComponent<DropPlaceScript>();
        // NotificationManager.ShowNotification(dropPlace.type.ToString());
        // if (dropPlace.type == FieldType.ENEMY_HAND || dropPlace.type == FieldType.ENEMY_FIELD)
        // {

        // faceDown = true;
        // }

        CardViewUGUI view = new(model, uguiPrefab, uguiParent, faceDown);
        return new BattleCardController(model, view);
    }
}
