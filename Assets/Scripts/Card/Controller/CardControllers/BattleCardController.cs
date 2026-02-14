using UnityEngine;

public class BattleCardController : BaseCardController
{
    public IBattleCardView BattleCardView { get; private set; }

    public CardBattleState BattleState { get; private set; }
    public bool CanAttack => BattleState.CanAttack();

    public BattleCardController(CardModel model, IBattleCardView view)
        : base(model, view)
    {
        BattleCardView = view;

        if (BattleCardView.CardRoot.TryGetComponent<CardClickScript>(out var clickScript))
            clickScript.OnCardClicked += OnCardClicked;

        BattleState = view.CardRoot.GetComponent<CardBattleState>();
    }

    private void OnCardClicked()
    {
        BattleCardController cloneController = CardControllerFactory.Create<BattleCardController>(CardModel, faceDown: false);
        IBattleCardView cloneView = cloneController?.BattleCardView;
        if (cloneView != null)
        {
            cloneView.SetHPOnClone(BattleState.CurrentHP);
            CardOverlayManager.Instance?.ShowBattleCard(BattleCardView, cloneView);
        }
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

    public void MarkAsPlayedInThisTurn(CardOwner owner) => BattleState.Init(owner);
    public void ResetTurnFlags() => BattleState.MarkAsReady();
    public void MarkAsAttacked() => BattleState.MarkAsAttacked();
}
