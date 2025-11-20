using UnityEngine.EventSystems;

public class BattleCardController : BaseCardController
{
    private readonly IBattleCardView battleCardView;

    public BattleCardController(CardModel model, IBattleCardView view)
        : base(model, view)
    {
        battleCardView = view;
        RegisterEvents();
    }

    public override void RegisterEvents()
    {
        if (!battleCardView.CardRootGameObject.TryGetComponent<EventTrigger>(out var trigger))
            trigger = battleCardView.CardRootGameObject.AddComponent<EventTrigger>();

        var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
        entry.callback.AddListener((data) => OnCardElementClicked());
        trigger.triggers.Add(entry);
    }

    public override void UnregisterEvents()
    {
        if (battleCardView.CardRootGameObject.TryGetComponent<EventTrigger>(out var trigger))
            trigger.triggers.Clear();
    }

    private void OnCardElementClicked()
    {
        if (!CardStateManager.IsCardRaised)
        {
            BattleCardController cloneController = CardControllerFactory.Create<BattleCardController>(CardModel);
            cloneController?.UnregisterEvents();
            IBattleCardView cloneView = cloneController?.battleCardView;
            if (cloneView != null)
            {
                CardOverlayManager.Instance?.ShowBattleCard(battleCardView, cloneView);
            }
        }
    }
}
