using UnityEngine.UIElements;

public interface ICollectionCardView : ICardView
{
    VisualTreeAsset CardTemplate { get; }
    VisualElement CardRoot { get; }

    void SetOpacity(bool isUserCard);
    void SetActive(bool isActive);
    void RegisterClickHandlers(EventCallback<ClickEvent> onCardElementClick);
    void UnregisterClickHandlers(EventCallback<ClickEvent> onCardElementClick);
}