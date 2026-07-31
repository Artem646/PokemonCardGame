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

public interface ICard3DView : ICardView
{
    GameObject CardRoot { get; }
    GameObject CardPrefab { get; }
}

public interface ICollectionCardView : IUIToolkitCardView
{
    void ApplyOwnedCardStyle(bool isUserCard);
    void ApplyCloneCardStyle();
    void ApplyCardStyleForActiveScene(string nameActiveScene);
    void RemoveAllAddedStyles();
    void SetActive(bool isActive);
    void RegisterClickHandlers(EventCallback<ClickEvent> onClick);
    void RegisterClickHandlersOnDescriptionButton(EventCallback<ClickEvent> onClick);
    void UnregisterClickHandlers(EventCallback<ClickEvent> onClick);
}

public interface IDeckCardView : IUIToolkitCardView
{
    void SetSelected(bool isSelected);
    void ApplyCloneCardStyle();
    void RegisterClickHandlers(EventCallback<ClickEvent> onClick);
    void UnregisterClickHandlers(EventCallback<ClickEvent> onClick);
}

public interface IBattleCardView : ICard3DView
{
    void ApplyFaceDownState(bool faceDown);
    void ApplyBattleStyle(CardBattleState battleState);
    void ResetBattleStyle();
    void SetHPOnClone(int HP);
}