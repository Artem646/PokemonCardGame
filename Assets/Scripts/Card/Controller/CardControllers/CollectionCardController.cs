using UnityEngine.UIElements;

public class CollectionCardController : BaseCardController
{
    private bool isAddedToDeck;
    private readonly ICollectionCardView collectionCardView;

    public CollectionCardController(CardModel model, ICollectionCardView view)
        : base(model, view)
    {
        collectionCardView = view;

        isAddedToDeck = CardDeck.Instance.IsCardInDeck(model.id);
        collectionCardView.SetAddedToDeck(isAddedToDeck);

        RegisterEvents();
    }

    public override void RegisterEvents()
    {
        collectionCardView.RegisterClickHandlers(OnCardElementClicked, OnAddToCardDeckButtonClicked);
    }

    public override void UnregisterEvents()
    {
        collectionCardView.UnregisterClickHandlers(OnCardElementClicked, OnAddToCardDeckButtonClicked);
    }

    private void OnCardElementClicked(ClickEvent evt)
    {
        CollectionCardController cloneController = CardControllerFactory.Create<CollectionCardController>(CardModel);
        cloneController?.UnregisterEvents();
        ICollectionCardView cloneView = cloneController?.collectionCardView;
        if (cloneView != null)
            CardOverlayManager.Instance?.ShowCard(collectionCardView, cloneView, evt);
    }

    private void OnAddToCardDeckButtonClicked(ClickEvent evt)
    {
        if (!isAddedToDeck)
        {
            if (CardDeck.Instance.AddCardToDeck(CardModel.id))
            {
                isAddedToDeck = true;
                collectionCardView.SetAddedToDeck(true);
            }
        }
        else
        {
            CardDeck.Instance.RemoveCardFromDeck(CardModel.id);
            isAddedToDeck = false;
            collectionCardView.SetAddedToDeck(false);
        }
    }

    public ICollectionCardView CollectionCardView => collectionCardView;
}
