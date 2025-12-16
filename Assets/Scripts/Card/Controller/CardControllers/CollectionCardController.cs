using UnityEngine.UIElements;

public class CollectionCardController : BaseCardController
{
    public ICollectionCardView CollectionCardView { get; set; }

    public CollectionCardController(CardModel model, ICollectionCardView view)
        : base(model, view)
    {
        CollectionCardView = view;
        RegisterEvents();
    }

    public override void RegisterEvents()
    {
        CollectionCardView.RegisterClickHandlers(OnCardElementClicked);
    }

    public override void UnregisterEvents()
    {
        CollectionCardView.UnregisterClickHandlers(OnCardElementClicked);
    }

    private void OnCardElementClicked(ClickEvent evt)
    {
        CollectionCardController cloneController = CardControllerFactory.Create<CollectionCardController>(CardModel);
        cloneController?.UnregisterEvents();
        ICollectionCardView cloneView = cloneController?.CollectionCardView;
        if (cloneView != null)
            CardOverlayManager.Instance?.ShowCollectionCard(CollectionCardView, cloneView, evt);
    }

    public override void AddToContainer(object container)
    {
        if (container is VisualElement visualElement)
            visualElement.Add(CollectionCardView.CardRoot);
    }

    public override void RemoveFromContainer()
    {
        CollectionCardView.CardRoot.RemoveFromHierarchy();
    }
}
