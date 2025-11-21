public class BattleCardController : BaseCardController
{
    private readonly IBattleCardView battleCardView;

    public BattleCardController(CardModel model, IBattleCardView view)
        : base(model, view)
    {
        battleCardView = view;
        RegisterEvents();

        if (view.CardRootGameObject.TryGetComponent(out CardClickScript clickScript))
        {
            clickScript.OnCardClicked += OnCardElementClicked;
        }
    }

    public override void RegisterEvents()
    { }

    public override void UnregisterEvents()
    { }

    private void OnCardElementClicked(CardClickScript cardClick)
    {
        BattleCardController cloneController = CardControllerFactory.Create<BattleCardController>(CardModel, faceDown: false);
        cloneController?.UnregisterEvents();
        IBattleCardView cloneView = cloneController?.battleCardView;
        if (cloneView != null)
        {
            CardOverlayManager.Instance?.ShowBattleCard(battleCardView, cloneView);
        }
    }
}
