using System.Collections.Generic;

public class Game
{
    public List<CardModel> PlayerDeck { get; set; }
    public List<CardModel> EnemyDeck { get; set; }

    public BattleCardListController PlayerHandListController { get; set; }
    public BattleCardListController EnemyHandListController { get; set; }
    public BattleCardListController PlayerFieldListController { get; set; }
    public BattleCardListController EnemyFieldListController { get; set; }
}