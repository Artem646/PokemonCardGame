using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BotGameState : MonoBehaviour
{
    private static WaitForSeconds _waitForSeconds5 = new(5);

    public List<int> BotDeckIds { get; private set; }
    public bool IsPlayerTurn { get; private set; } = true;
    public int RoundNumber { get; private set; } = 1;

    private GameManagerScript gameManager;

    public void BindGameManager(GameManagerScript manager)
    {
        gameManager = manager;
        gameManager.Init(this);
    }

    public async Task StartGame()
    {
        BotDeckIds = await GenerateRandomDeck(5);
        gameManager.LoadCardsToEnemyHand(BotDeckIds);
        RoundNumber = 1;
        IsPlayerTurn = true;
        gameManager.OnTurnChanged(RoundNumber);
    }

    public void EndPlayerTurn()
    {
        if (!IsPlayerTurn) return;

        IsPlayerTurn = false;
        StartCoroutine(BotTurn());
    }

    private IEnumerator BotTurn()
    {
        yield return _waitForSeconds5;

        List<BattleCardController> handList = gameManager.CurrentGame.EnemyHandListController.CardControllers;
        if (handList.Count > 0)
        {
            BattleCardController cardController = handList[Random.Range(0, handList.Count)];
            int siblingIndex = 0;

            handList.Remove(cardController);
            gameManager.CurrentGame.EnemyFieldListController.CardControllers.Add(cardController);

            cardController.BattleCardView.CardRoot.transform.SetParent(gameManager.EnemyFieldContainer, false);
            cardController.BattleCardView.CardRoot.transform.SetSiblingIndex(siblingIndex);

            cardController.BattleCardView.CardRoot.GetComponent<CardFlipScript>().FlipToFaceUp();
        }

        TryResolveBattle();

        IsPlayerTurn = true;
        RoundNumber++;
        gameManager.OnTurnChanged(RoundNumber);

        CheckGameEnd();
    }

    private void TryResolveBattle()
    {
        var playerCard = gameManager.CurrentGame.PlayerFieldListController.CardControllers.LastOrDefault();
        var enemyCard = gameManager.CurrentGame.EnemyFieldListController.CardControllers.LastOrDefault();

        if (playerCard != null && enemyCard != null)
        {
            TypeChart chart = gameManager.typeChart;
            ResolveBattle(playerCard, enemyCard, chart);
        }
    }

    private void ResolveBattle(BattleCardController attacker, BattleCardController defender, TypeChart chart)
    {
        if (attacker == null || defender == null) return;

        float attackerMultiplier = chart.GetMultiplier(attacker.CardModel.mainElement, defender.CardModel.mainElement);
        float defenderMultiplier = chart.GetMultiplier(defender.CardModel.mainElement, attacker.CardModel.mainElement);

        if (attackerMultiplier > defenderMultiplier)
        {
            gameManager.MoveCardToReset(defender, false);
        }
        else if (attackerMultiplier < defenderMultiplier)
        {
            gameManager.MoveCardToReset(attacker, true);
        }
        else
        {
            gameManager.MoveCardToReset(attacker, true);
            gameManager.MoveCardToReset(defender, false);
        }
    }

    private void CheckGameEnd()
    {
        int playerFieldCount = gameManager.CurrentGame.PlayerFieldListController.CardControllers.Count;
        int enemyFieldCount = gameManager.CurrentGame.EnemyFieldListController.CardControllers.Count;
        int playerHandCount = gameManager.CurrentGame.PlayerHandListController.CardControllers.Count;
        int enemyHandCount = gameManager.CurrentGame.EnemyHandListController.CardControllers.Count;

        if (playerFieldCount == 3 || enemyFieldCount == 3 || playerHandCount == 0 || enemyHandCount == 0)
        {
            gameManager.ShowGameOverOverlay();
            SceneManager.LoadScene("CollectionScene");
        }
    }

    private async Task<List<int>> GenerateRandomDeck(int cardCount)
    {
        GameCardModelList allGameCards = await CardRepository.Instance.GetAllGameCards();
        return allGameCards.cards.OrderBy(card => Random.value).Take(cardCount).Select(card => card.id).ToList();
    }
}
