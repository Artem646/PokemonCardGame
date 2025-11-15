using UnityEngine.UIElements;
using System;
using UnityEngine;

public class CardControllerFactory
{
    private static VisualTreeAsset uxmlTemplate;
    private static GameObject uguiPrefab;
    private static Transform uguiParent;

    public static void Init(VisualTreeAsset template = null, GameObject prefab = null, Transform parent = null)
    {
        uxmlTemplate = template;
        uguiPrefab = prefab;
        uguiParent = parent;
    }

    public static T Create<T>(CardModel model) where T : BaseCardController
    {
        Type t = typeof(T);

        if (t == typeof(CollectionCardController))
            return (T)(BaseCardController)CreateCollection(model);

        if (t == typeof(BattleCardController))
            return (T)(BaseCardController)CreateBattle(model);

        throw new NotSupportedException($"CardControllerFactory: Unknown controller type {t.Name}");
    }

    public static CollectionCardController CreateCollection(CardModel model)
    {
        CardViewUIToolkit view = new(model, uxmlTemplate);
        CollectionCardController controller = new(model, view);
        return controller;
    }

    public static BattleCardController CreateBattle(CardModel model)
    {
        CardViewUGUI view = new(model, uguiPrefab, uguiParent);
        return new BattleCardController(model, view);
    }
}
