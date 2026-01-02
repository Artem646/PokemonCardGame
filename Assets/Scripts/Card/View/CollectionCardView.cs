using UnityEngine;
using UnityEngine.UIElements;

public class CollectionCardView : CardViewBase, ICollectionCardView, IUIToolkitCardView
{
    public VisualElement CardRoot { get; }
    public VisualTreeAsset CardTemplate { get; }
    private VisualElement cardElement;
    private VisualElement border;
    private Label titleLabel;

    public CollectionCardView(CardModel model, VisualTreeAsset template)
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
        border = CardRoot.Q<VisualElement>("border");
        titleLabel = CardRoot.Q<Label>("title");
    }

    public override void BindData()
    {
        cardElement.style.backgroundColor = new StyleColor(CardModel.colors.cardColor);
        titleLabel.text = CardModel.title;
        CardViewHelper.UpdateBodyUIToolkit(CardRoot, CardModel, CardElementLayoutModeConfig.Collection);
        CardViewHelper.SetImagesUIToolkit(CardRoot, CardModel);
    }

    public void RegisterClickHandlers(EventCallback<ClickEvent> onCardElementClick)
    {
        cardElement?.RegisterCallback(onCardElementClick);
    }

    public void UnregisterClickHandlers(EventCallback<ClickEvent> onCardElementClick)
    {
        cardElement?.UnregisterCallback(onCardElementClick);
    }

    public void SetActive(bool isActive)
    {
        cardElement.style.display = isActive ? DisplayStyle.Flex : DisplayStyle.None;
    }

    public void ApplyOwnedCardStyle(bool isUserCard)
    {
        if (!isUserCard)
        {
            cardElement.AddToClassList("grayCard");
        }
        else
        {
            border.style.borderTopColor = Color.softYellow;
            border.style.borderRightColor = Color.softYellow;
            border.style.borderBottomColor = Color.softYellow;
            border.style.borderLeftColor = Color.softYellow;
        }
    }
}