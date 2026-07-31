using UnityEngine.UIElements;

public class DeckCardView : CardViewBase, IDeckCardView
{
    public VisualElement CardRoot { get; }
    public VisualTreeAsset CardTemplate { get; }
    private VisualElement cardFrame;
    private VisualElement fullCard;
    private Label cardNumberLabel;

    public DeckCardView(CardModel model, VisualTreeAsset template)
        : base(model)
    {
        model.uiToolkitVisualData = CardUIToolkitVisualRegistrySO.Instance.GetVisualData(model.id);

        CardTemplate = template;
        CardRoot = CardTemplate.Instantiate();
        InitializeElements();
        BindData();
    }

    private void InitializeElements()
    {
        cardFrame = CardRoot.Q<VisualElement>("cardFrame");
        fullCard = CardRoot.Q<VisualElement>("fullCard");
        cardNumberLabel = CardRoot.Q<Label>("cardNumberLabel");
    }

    public override void BindData()
    {
        fullCard.style.backgroundColor = new StyleColor(CardModel.colors.cardColor);
        Localizer.LocalizeCardTitleElement(CardRoot, "title", CardModel, "PokemonTitles");
        CardViewHelper.UpdateBodyUIToolkit(CardRoot, CardModel);
        CardViewHelper.SetBodyImagesUIToolkit(CardRoot, CardModel);
        cardNumberLabel.text = CardModel.id.ToString();
    }

    public void RegisterClickHandlers(EventCallback<ClickEvent> onFullCardClick)
    {
        fullCard.RegisterCallback(onFullCardClick);
    }

    public void UnregisterClickHandlers(EventCallback<ClickEvent> onFullCardClick)
    {
        fullCard.UnregisterCallback(onFullCardClick);
    }

    public void SetSelected(bool isSelected)
    {
        if (isSelected)
        {
            cardFrame.AddToClassList("selected-card_frame");
            fullCard.RemoveFromClassList("no-selected-card");
        }
        else
        {
            fullCard.AddToClassList("no-selected-card");
            cardFrame.RemoveFromClassList("selected-card_frame");
        }
    }

    public void ApplyCloneCardStyle() => CardRoot.AddToClassList("clone-card");
}