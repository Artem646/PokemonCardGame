using UnityEngine.UIElements;

public interface ICollectionCardView : ICardView
{
    VisualTreeAsset CardTemplate { get; }
    VisualElement CardRoot { get; }

    void SetAddedToDeck(bool isAdded);
    void SetOpacity(bool value);
    void RegisterClickHandlers(EventCallback<ClickEvent> onCardElementClick, EventCallback<ClickEvent> onAddToCardDeckButtonClick);
    void UnregisterClickHandlers(EventCallback<ClickEvent> onCardElementClick, EventCallback<ClickEvent> onAddToCardDeckButtonClick);
}