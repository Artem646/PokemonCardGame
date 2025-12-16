public abstract class BaseCardController : ICardController
{
    public CardModel CardModel { get; protected set; }
    public ICardView CardView { get; protected set; }

    protected BaseCardController(CardModel model, ICardView view)
    {
        CardModel = model;
        CardView = view;
    }

    public virtual void RegisterEvents() { }
    public virtual void UnregisterEvents() { }

    public abstract void AddToContainer(object container);
    public abstract void RemoveFromContainer();

}
