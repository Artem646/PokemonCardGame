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
    { }
}
