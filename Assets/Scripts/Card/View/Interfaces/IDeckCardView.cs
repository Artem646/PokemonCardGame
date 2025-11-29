using UnityEngine.UIElements;

public interface IDeckCardView : ICardView
{
    VisualTreeAsset CardTemplate { get; }
    VisualElement CardRoot { get; }

    void SetSelected(bool isSelected);
    void RegisterClickHandlers(EventCallback<ClickEvent> onCardClick);
    void UnregisterClickHandlers(EventCallback<ClickEvent> onCardClick);
}
