using UnityEngine.UIElements;
using System;

public static class CardControllerBuilder
{
    private static VisualTreeAsset defaultTemplate;

    public static void SetDefaultTemplate(VisualTreeAsset template)
    {
        defaultTemplate = template;
    }

    public static BaseCardController CreateCardController<T>(CardModel card) where T : BaseCardController
    {
        return (BaseCardController)Activator.CreateInstance(typeof(T), card, defaultTemplate);
    }
}
