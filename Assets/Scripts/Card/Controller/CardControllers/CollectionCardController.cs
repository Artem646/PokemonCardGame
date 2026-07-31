using UnityEngine.SceneManagement;
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
        CollectionCardView.RegisterClickHandlers(OnCardClicked);
        CollectionCardView.RegisterClickHandlersOnDescriptionButton(OnDescriptionButtonClicked);
    }

    public override void UnregisterEvents()
    {
        CollectionCardView.UnregisterClickHandlers(OnCardClicked);
    }

    private void OnCardClicked(ClickEvent evt)
    {
        CardControllerFactory.Init(template: CollectionCardView.CardTemplate);
        CollectionCardController cloneController = CardControllerFactory.Create<CollectionCardController>(CardModel);
        cloneController.UnregisterEvents();

        ICollectionCardView cloneCardView = cloneController.CollectionCardView;
        cloneCardView.ApplyCloneCardStyle();

        CardOverlayManager.Instance.ShowCollectionCard(CollectionCardView, cloneCardView);
    }

    private void OnDescriptionButtonClicked(ClickEvent evt)
    {
        CardOverlayManager.Instance.HideOverlay();
        SelectedCardModelStorage.SelectedCardModel = CardModel;

        if (SceneManager.GetActiveScene().name != "DescriptionScene")
            SceneContext.PreviousDescriptionSceneName = SceneManager.GetActiveScene().name;

        SceneSwitcher.SwitchScene("DescriptionScene", null);
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