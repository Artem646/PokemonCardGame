using UnityEngine.UIElements;

public class CollectionCardView : CardViewBase, ICollectionCardView
{
    public VisualElement CardRoot { get; }
    public VisualTreeAsset CardTemplate { get; }
    private VisualElement cardFrame;
    private VisualElement fullCard;
    private VisualElement body;
    private Label cardNumberLabel;
    private VisualElement noOwnedCardCover;
    private Button descriptionButton;

    public CollectionCardView(CardModel model, VisualTreeAsset template)
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
        body = CardRoot.Q<VisualElement>("body");
        cardNumberLabel = CardRoot.Q<Label>("cardNumberLabel");
        noOwnedCardCover = CardRoot.Q<VisualElement>("noOwnedCardCover");
        descriptionButton = CardRoot.Q<Button>("descriptionButton");
    }

    public override void BindData()
    {
        fullCard.style.backgroundColor = new StyleColor(CardModel.colors.cardColor);
        Localizer.LocalizeCardTitleElement(CardRoot, "title", CardModel, "PokemonTitles");
        CardViewHelper.UpdateBodyUIToolkit(CardRoot, CardModel);
        CardViewHelper.SetBodyImagesUIToolkit(CardRoot, CardModel);
        CardViewHelper.UpdateStatsUIToolkit(CardRoot, CardModel);
        CardViewHelper.UpdateAbilitiesUIToolkit(CardRoot, CardModel);
        CardViewHelper.SetAbilityElementsImagesUIToolkit(CardRoot, CardModel);
        CardViewHelper.UpdateAbilityDamageUIToolkit(CardRoot, CardModel);
        cardNumberLabel.text = CardModel.id.ToString();
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
        fullCard.style.display = isActive ? DisplayStyle.Flex : DisplayStyle.None;
    }

    public void ApplyOwnedCardStyle(bool isUserCard)
    {
        if (isUserCard) cardFrame.AddToClassList("owned-card_frame");
        else
        {
            noOwnedCardCover.AddToClassList("no-owned-card_cover");
            cardFrame.AddToClassList("no-owned-card_frame");
        }
    }

    public void ApplyCloneCardStyle() => CardRoot.AddToClassList("clone-card");

    public void ApplyCardStyleForActiveScene(string nameActiveScene)
    {
        if (nameActiveScene == "CollectionScene")
            CardRoot.AddToClassList("collection-card");
        else if (nameActiveScene == "BestiaryScene")
            CardRoot.AddToClassList("bestiary-card");
        else if (nameActiveScene == "DescriptionScene")
            CardRoot.AddToClassList("description-card");
    }

    public void RemoveAllAddedStyles()
    {
        CardRoot.RemoveFromClassList("collection-card");
        CardRoot.RemoveFromClassList("bestiary-card");
        cardFrame.RemoveFromClassList("owned-card_frame");
        noOwnedCardCover.RemoveFromClassList("no-owned-card_cover");
        cardFrame.RemoveFromClassList("no-owned-card_frame");
        CardRoot.RemoveFromClassList("middle-card");
        CardRoot.RemoveFromClassList("description-card");
        CardRoot.RemoveFromClassList("evolution-card");
    }
}