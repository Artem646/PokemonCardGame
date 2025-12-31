using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    public void StartGame()
    {
        BotDeckIds = GenerateRandomDeck(5);
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
    }

    private void TryResolveBattle()
    {
        BattleCardController playerCard = gameManager.CurrentGame.PlayerFieldListController.CardControllers.LastOrDefault();
        BattleCardController enemyCard = gameManager.CurrentGame.EnemyFieldListController.CardControllers.LastOrDefault();

        if (playerCard != null && enemyCard != null)
        {
            TypeChart chart = gameManager.typeChart;
            StartCoroutine(ResolveBattleWithDelay(playerCard, enemyCard, chart, 2f));
        }
        else
        {
            IsPlayerTurn = true;
            RoundNumber++;
            gameManager.OnTurnChanged(RoundNumber);
            CheckGameEnd();
        }
    }

    private IEnumerator ResolveBattleWithDelay(BattleCardController attacker, BattleCardController defender, TypeChart chart, float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);
        ResolveBattle(attacker, defender, chart);

        IsPlayerTurn = true;
        RoundNumber++;
        gameManager.OnTurnChanged(RoundNumber);
        CheckGameEnd();
    }

    private void ResolveBattle(BattleCardController attacker, BattleCardController defender, TypeChart chart)
    {
        if (attacker == null || defender == null) return;

        float attackerMultiplier = chart.GetMultiplier(attacker.CardModel.mainElement, defender.CardModel.mainElement);
        float defenderMultiplier = chart.GetMultiplier(defender.CardModel.mainElement, attacker.CardModel.mainElement);

        string attackerOwner = gameManager.CurrentGame.PlayerFieldListController.CardControllers.Contains(attacker)
            ? "Игрок" : "Бот";
        string defenderOwner = gameManager.CurrentGame.PlayerFieldListController.CardControllers.Contains(defender)
            ? "Игрок" : "Бот";

        string resultMessage;

        if (attackerMultiplier > defenderMultiplier)
        {
            gameManager.MoveCardToReset(defender, false);
            resultMessage = $"{attackerOwner}: {attacker.CardModel.title} победил {defenderOwner}: {defender.CardModel.title}, " +
                            $"потому что {attacker.CardModel.mainElement} сильнее {defender.CardModel.mainElement}";
        }
        else if (attackerMultiplier < defenderMultiplier)
        {
            gameManager.MoveCardToReset(attacker, true);
            resultMessage = $"{defenderOwner}: {defender.CardModel.title} победил {attackerOwner}: {attacker.CardModel.title}, " +
                            $"потому что {defender.CardModel.mainElement} сильнее {attacker.CardModel.mainElement}";
        }
        else
        {
            gameManager.MoveCardToReset(attacker, true);
            gameManager.MoveCardToReset(defender, false);
            resultMessage = $"{attackerOwner}: {attacker.CardModel.title} и {defenderOwner}: {defender.CardModel.title} равны по силе, оба отправлены в сброс";
        }

        NotificationManager.ShowNotification(resultMessage, NotificationType.Info);
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
            StartCoroutine(LoadSceneAfterDelay(3f));
        }
    }

    private IEnumerator LoadSceneAfterDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        SceneManager.LoadScene("CollectionScene");
    }

    private List<int> GenerateRandomDeck(int cardCount)
    {
        GameCardModelList allGameCards = CardRepository.Instance.GetGameCards();
        return allGameCards.cards.OrderBy(card => Random.value).Take(cardCount).Select(card => card.id).ToList();
    }
}
