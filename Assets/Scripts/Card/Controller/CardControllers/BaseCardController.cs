using UnityEngine.UIElements;

public abstract class BaseCardController
{
    public CardModel CardModel { get; protected set; }
    public CardView CardView { get; protected set; }

    protected BaseCardController(CardModel card, VisualTreeAsset template)
    {
        CardModel = card;
        CardView = new CardView(card, template);
    }

    public abstract void RegisterEvents();
}
