using UnityEngine;
using UnityEngine.UIElements;

public class DeckCardView : CardViewBase, IUIToolkitCardView, IDeckCardView
{
    public VisualElement CardRoot { get; }
    public VisualTreeAsset CardTemplate { get; }
    private VisualElement cardElement;
    private Label titleLabel;

    public DeckCardView(CardModel model, VisualTreeAsset template)
        : base(model)
    {
        CardTemplate = template;
        CardRoot = CardTemplate.Instantiate();
        InitializeElements();
        BindData();
    }

    private void InitializeElements()
    {
        cardElement = CardRoot.Q<VisualElement>("fullCard");
        titleLabel = CardRoot.Q<Label>("title");
    }

    public override void BindData()
    {
        cardElement.style.backgroundColor = new StyleColor(CardModel.colors.cardColor);
        titleLabel.text = CardModel.title;
        CardViewHelper.UpdateBodyUIToolkit(CardRoot, CardModel, CardElementLayoutModeConfig.Deck);
        CardViewHelper.SetImagesUIToolkit(CardRoot, CardModel);
    }

    public void RegisterClickHandlers(EventCallback<ClickEvent> onCardElementClick)
    {
        cardElement.RegisterCallback(onCardElementClick);
    }

    public void UnregisterClickHandlers(EventCallback<ClickEvent> onCardElementClick)
    {
        cardElement.UnregisterCallback(onCardElementClick);
    }

    public void SetSelected(bool isSelected)
    {
        if (isSelected)
        {
            cardElement.style.opacity = 1f;
            cardElement.style.borderTopColor = Color.coral;
            cardElement.style.borderRightColor = Color.coral;
            cardElement.style.borderBottomColor = Color.coral;
            cardElement.style.borderLeftColor = Color.coral;
            cardElement.style.borderTopWidth = 3;
            cardElement.style.borderRightWidth = 3;
            cardElement.style.borderBottomWidth = 3;
            cardElement.style.borderLeftWidth = 3;
        }
        else
        {
            cardElement.style.opacity = 0.5f;
            cardElement.style.borderTopWidth = 0;
            cardElement.style.borderRightWidth = 0;
            cardElement.style.borderBottomWidth = 0;
            cardElement.style.borderLeftWidth = 0;
        }
    }
}