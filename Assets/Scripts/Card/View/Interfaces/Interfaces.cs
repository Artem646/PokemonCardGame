using UnityEngine;
using UnityEngine.UIElements;

public interface ICardView
{
    CardModel CardModel { get; }
    void BindData();
}

public interface IUIToolkitCardView : ICardView
{
    VisualElement CardRoot { get; }
    VisualTreeAsset CardTemplate { get; }
}

public interface IUGUICardView : ICardView
{
    GameObject CardRoot { get; }
    GameObject CardPrefab { get; }
}

public interface ICollectionCardView : IUIToolkitCardView
{
    void ApplyOwnedCardStyle(bool isUserCard);
    void SetActive(bool isActive);
    void RegisterClickHandlers(EventCallback<ClickEvent> onClick);
    void RegisterClickHandlersOnDescriptionButton(EventCallback<ClickEvent> onClick);
    void UnregisterClickHandlers(EventCallback<ClickEvent> onClick);
}

public interface IDeckCardView : IUIToolkitCardView
{
    void SetSelected(bool isSelected);
    void RegisterClickHandlers(EventCallback<ClickEvent> onClick);
    void UnregisterClickHandlers(EventCallback<ClickEvent> onClick);
}

public interface IBattleCardView : IUGUICardView
{
    void ApplyFaceDownState(bool faceDown);
}