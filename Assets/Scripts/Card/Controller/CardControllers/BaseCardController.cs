public abstract class BaseCardController
{
    public CardModel CardModel { get; protected set; }
    public ICardView CardView { get; protected set; }

    protected ICardView cardView;

    protected BaseCardController(CardModel model, ICardView view)
    {
        CardModel = model;
        CardView = view;
    }

    public abstract void RegisterEvents();
    public virtual void UnregisterEvents() { }
}
