using UnityEngine;
using UnityEngine.UIElements;
public class CollectionCardView : CardViewBase, ICollectionCardView
{
    public VisualElement CardRoot { get; }
    public VisualTreeAsset CardTemplate { get; }
    private VisualElement cardElement;
    private VisualElement border;
    private VisualElement body;
    private Button descriptionButton;

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
        border = CardRoot.Q<VisualElement>("cardFrame");
        cardElement = CardRoot.Q<VisualElement>("fullCard");
        descriptionButton = CardRoot.Q<Button>("descriptionButton");
        body = CardRoot.Q<VisualElement>("body");
    }

    public override void BindData()
    {
        cardElement.style.backgroundColor = new StyleColor(CardModel.colors.cardColor);
        Localizer.LocalizeElement(CardRoot, "title", CardModel.titleKey, "PokemonTitles");
        CardViewHelper.UpdateBodyUIToolkit(CardRoot, CardModel, CardElementLayoutModeConfig.Collection);
        CardViewHelper.SetImagesUIToolkit(CardRoot, CardModel);
    }

    public void RegisterClickHandlers(EventCallback<ClickEvent> onBodyClick)
    {
        body.RegisterCallback(onBodyClick);
    }

    public void RegisterClickHandlersOnDescriptionButton(EventCallback<ClickEvent> onDescriptionButtonClick)
    {
        descriptionButton.RegisterCallback<ClickEvent>(evt =>
        {
            evt.StopPropagation();
            onDescriptionButtonClick.Invoke(evt);
        });
    }

    public void UnregisterClickHandlers(EventCallback<ClickEvent> onBodyClick)
    {
        body.UnregisterCallback(onBodyClick);
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

    public void ApplyStyleForCloneCard()
    {
        RemoveMargins();
        ResetBorderWidth();
    }

    private void RemoveMargins()
    {
        border.style.marginTop = 0;
        border.style.marginRight = 0;
        border.style.marginBottom = 0;
        border.style.marginLeft = 0;
    }

    private void ResetBorderWidth()
    {
        border.style.borderTopWidth = 0;
        border.style.borderRightWidth = 0;
        border.style.borderBottomWidth = 0;
        border.style.borderLeftWidth = 0;
    }
}