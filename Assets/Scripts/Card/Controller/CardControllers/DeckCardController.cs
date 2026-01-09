using UnityEngine.UIElements;

public class DeckCardController : BaseCardController
{
    public IDeckCardView DeckCardView { get; set; }

    public DeckCardController(CardModel model, IDeckCardView view)
        : base(model, view)
    {
        DeckCardView = view;
        RegisterEvents();
    }

    public override void RegisterEvents()
    {
        DeckCardView.RegisterClickHandlers(OnCardElementClicked);
    }

    public override void UnregisterEvents()
    {
        DeckCardView.UnregisterClickHandlers(OnCardElementClicked);
    }

    private void OnCardElementClicked(ClickEvent evt)
    {
        CardControllerFactory.Init(template: DeckCardView.CardTemplate);
        DeckCardController cloneController = CardControllerFactory.Create<DeckCardController>(CardModel);
        cloneController?.UnregisterEvents();
        IDeckCardView cloneView = cloneController?.DeckCardView;
        if (cloneView != null)
            CardOverlayManager.Instance?.ShowDeckCard(DeckCardView, cloneView);
    }

    public override void AddToContainer(object container)
    {
        if (container is VisualElement visualElement)
            visualElement.Add(DeckCardView.CardRoot);
    }

    public override void RemoveFromContainer()
    {
        DeckCardView.CardRoot.RemoveFromHierarchy();
    }
}
