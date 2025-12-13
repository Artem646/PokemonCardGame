using UnityEngine.UIElements;

public class CollectionCardController : BaseCardController
{
    private readonly ICollectionCardView collectionCardView;

    public CollectionCardController(CardModel model, ICollectionCardView view)
        : base(model, view)
    {
        collectionCardView = view;

        RegisterEvents();
    }

    public override void RegisterEvents()
    {
        collectionCardView.RegisterClickHandlers(OnCardElementClicked);
    }

    public override void UnregisterEvents()
    {
        collectionCardView.UnregisterClickHandlers(OnCardElementClicked);
    }

    private void OnCardElementClicked(ClickEvent evt)
    {
        CollectionCardController cloneController = CardControllerFactory.Create<CollectionCardController>(CardModel);
        cloneController?.UnregisterEvents();
        ICollectionCardView cloneView = cloneController?.collectionCardView;
        if (cloneView != null)
        {
            CardOverlayManager.Instance?.ShowCollectionCard(collectionCardView, cloneView, evt);
        }
    }

    public ICollectionCardView CollectionCardView => collectionCardView;
}
