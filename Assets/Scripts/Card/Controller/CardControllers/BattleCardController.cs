using UnityEngine;

public class BattleCardController : BaseCardController
{
    public IBattleCardView BattleCardView { get; set; }
    public bool CanAttack { get; private set; } = false;

    public BattleCardController(CardModel model, IBattleCardView view)
        : base(model, view)
    {
        BattleCardView = view;

        if (view.CardRoot.TryGetComponent(out CardClickScript clickScript))
            clickScript.OnCardClicked += OnCardElementClicked;
    }

    private void OnCardElementClicked(CardClickScript cardClick)
    {
        BattleCardController cloneController = CardControllerFactory.Create<BattleCardController>(CardModel, faceDown: false);
        IBattleCardView cloneView = cloneController?.BattleCardView;
        if (cloneView != null)
            CardOverlayManager.Instance?.ShowBattleCard(BattleCardView, cloneView);
    }

    public override void AddToContainer(object container)
    {
        if (container is Transform handContainer)
            BattleCardView.CardRoot.transform.SetParent(handContainer, false);
    }

    public override void RemoveFromContainer()
    {
        Object.Destroy(BattleCardView.CardRoot);
    }
}
