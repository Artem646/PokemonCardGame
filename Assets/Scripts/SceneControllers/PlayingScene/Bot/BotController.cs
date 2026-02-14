using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BotController : MonoBehaviour
{
    [SerializeField] private GameManagerScript gameManager;
    private BotTurnManager botTurnManager;

    public List<int> BotDeckIds { get; private set; }

    private List<BattleCardController> playerHandCards, playerFieldCards, enemyHandCards, enemyFieldCards;

    public void GenerateDeck()
    {
        static List<int> GenerateRandomDeck(int cardCount)
        {
            GameCardModelList allGameCards = CardRepository.Instance.GetGameCards();
            return allGameCards.cards
                .OrderBy(card => Random.value)
                .Take(cardCount)
                .Select(card => card.id)
                .ToList();
        }

        BotDeckIds = GenerateRandomDeck(5);
    }

    public void EndPlayerTurn()
    {
        StartCoroutine(BotTurnRoutine());
    }

    private void InitializeVariables()
    {
        enemyHandCards = gameManager.CurrentGame.EnemyHandListController.CardControllers;
        enemyFieldCards = gameManager.CurrentGame.EnemyFieldListController.CardControllers;
        playerHandCards = gameManager.CurrentGame.PlayerHandListController.CardControllers;
        playerFieldCards = gameManager.CurrentGame.PlayerFieldListController.CardControllers;
        botTurnManager = FindAnyObjectByType<BotTurnManager>();
    }

    public IEnumerator BotTurnRoutine()
    {
        InitializeVariables();

        yield return WaitForSeconds(1.5f);

        if (enemyFieldCards.Count < 3 && enemyHandCards.Count != 0)
        {
            yield return PlayBotCard();
        }

        bool canAttack = botTurnManager.GetAttackableCards(false).Any();
        if (!canAttack) botTurnManager.EndTurn();
        else
        {
            yield return WaitForSeconds(1.5f);
            yield return Attack();
            yield return WaitForSeconds(0.5f);
            botTurnManager.EndTurn();
        }
    }

    private IEnumerator PlayBotCard()
    {
        Transform enemyHandTransform = gameManager.EnemyHandContainer;
        Transform enemyFieldTransform = gameManager.EnemyFieldContainer;

        int cardIndexInHand = Random.Range(0, enemyHandTransform.childCount - 1);
        Transform cardTransform = enemyHandTransform.GetChild(cardIndexInHand);

        if (cardTransform.TryGetComponent<CardMovemantScript>(out var card) &&
            cardTransform.TryGetComponent<CardControllerLink>(out var link))
        {
            int siblingIndexInField = Random.Range(0, enemyFieldTransform.childCount + 1);
            yield return card.MoveCardTransformToAnotherField(enemyFieldTransform, siblingIndexInField);
            cardTransform.GetComponent<CardFlipScript>().FlipToFaceUp();

            gameManager.CurrentGame.EnemyHandListController.CardControllers.Remove(link.Controller);
            int cardIndexInField = Mathf.Clamp(cardTransform.GetSiblingIndex(), 0, gameManager.CurrentGame.EnemyFieldListController.CardControllers.Count);
            gameManager.CurrentGame.EnemyFieldListController.CardControllers.Insert(cardIndexInField, link.Controller);

            link.Controller.MarkAsPlayedInThisTurn(CardOwner.Enemy);
        }
    }

    private IEnumerator Attack()
    {
        List<BattleCardController> botAttackers = botTurnManager.GetAttackableCards(false).ToList();

        foreach (BattleCardController attacker in botAttackers)
        {
            if (playerFieldCards.Count != 0)
            {
                Transform cardTransform = attacker.BattleCardView.CardRoot.transform;
                int targetCardIndex = Random.Range(0, playerFieldCards.Count - 1);
                BattleCardController defender = playerFieldCards[targetCardIndex];
                Transform targetTransform = defender.BattleCardView.CardRoot.transform;

                if (cardTransform.TryGetComponent<CardAttackDragHandler>(out var card))
                    yield return card.MoveEnemyCardTransformForAttack(targetTransform, attacker, defender);

                yield return WaitForSeconds(1f);
            }
        }
    }

    private IEnumerator WaitForSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
    }
}
