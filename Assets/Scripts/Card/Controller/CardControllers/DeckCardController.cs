using UnityEngine.UIElements;

public class DeckCardController : BaseCardController
{
    public IDeckCardView DeckCardView { get; set; }
    private bool isSelected;
    public event System.Action<int, bool> OnCardSelectionChanged;

    public DeckCardController(CardModel model, IDeckCardView view, bool selected)
        : base(model, view)
    {
        DeckCardView = view;
        isSelected = selected;
        DeckCardView.SetSelected(isSelected);
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
        isSelected = !isSelected;
        DeckCardView.SetSelected(isSelected);
        OnCardSelectionChanged?.Invoke(CardModel.id, isSelected);
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
