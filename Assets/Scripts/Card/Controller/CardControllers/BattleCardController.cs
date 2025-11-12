using UnityEngine.UIElements;

public class BattleCardController : BaseCardController
{
    public BattleCardController(CardModel model, VisualTreeAsset template)
        : base(model, template)
    {
        RegisterEvents();
        CardView.SetStyleForBattle();
    }

    public override void RegisterEvents()
    {
        CardView.EnableDragging();
    }
}