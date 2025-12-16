using UnityEngine.UIElements;

public class CollectionCardView : CardViewBase, ICollectionCardView, IUIToolkitCardView
{
    public VisualElement CardRoot { get; }
    public VisualTreeAsset CardTemplate { get; }
    private VisualElement cardElement;
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
        titleLabel = CardRoot.Q<Label>("title");
    }

    public override void BindData()
    {
        cardElement.style.backgroundColor = new StyleColor(CardModel.colors.cardColor);
        titleLabel.text = CardModel.title;
        CardViewHelper.UpdateBodyUIToolkit(CardRoot, CardModel, false);
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

    public void SetOpacity(bool isUserCard)
    {
        cardElement.style.opacity = isUserCard ? 1f : 0.3f;
    }
}