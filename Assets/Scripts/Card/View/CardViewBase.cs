public abstract class CardViewBase : ICardView
{
    public CardModel CardModel { get; }

    protected CardViewBase(CardModel model)
    {
        CardModel = model;
    }

    public abstract void BindData();
}
