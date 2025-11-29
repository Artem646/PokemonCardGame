using UnityEngine.UIElements;

public class DeckCardController : BaseCardController
{
    private readonly IDeckCardView deckCardView;
    private bool isSelected;
    public event System.Action<int, bool> OnCardSelectionChanged;

    public DeckCardController(CardModel model, IDeckCardView view, bool initiallySelected)
        : base(model, view)
    {
        deckCardView = view;
        isSelected = initiallySelected;
        deckCardView.SetSelected(isSelected);

        RegisterEvents();
    }

    public override void RegisterEvents()
    {
        deckCardView.RegisterClickHandlers(OnCardElementClicked);
    }

    public override void UnregisterEvents()
    {
        deckCardView.UnregisterClickHandlers(OnCardElementClicked);
    }

    private void OnCardElementClicked(ClickEvent evt)
    {
        isSelected = !isSelected;
        deckCardView.SetSelected(isSelected);
        OnCardSelectionChanged?.Invoke(CardModel.id, isSelected);
    }

    public IDeckCardView DeckCardView => deckCardView;
}
