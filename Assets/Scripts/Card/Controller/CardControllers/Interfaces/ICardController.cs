public interface ICardController
{
    CardModel CardModel { get; }
    ICardView CardView { get; }

    void AddToContainer(object container);
    void RemoveFromContainer();
}
