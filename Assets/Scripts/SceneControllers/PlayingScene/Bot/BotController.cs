using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class BotDeckData
{
    public static List<int> BotDeckIds = new();
}

public class BotController : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private TypeChart typeChart;

    private BotTurnManager botTurnManager;

    private BattleCardController[] playerFieldCards, enemyHandCards, enemyFieldCards;

    public void EndPlayerTurn()
    {
        StartCoroutine(BotTurnRoutine());
    }

    private void InitializeVariables()
    {
        playerFieldCards = gameManager.CurrentGame.PlayerFieldControllers;
        enemyHandCards = gameManager.CurrentGame.EnemyHandControllers;
        enemyFieldCards = gameManager.CurrentGame.EnemyFieldControllers;
        botTurnManager = FindAnyObjectByType<BotTurnManager>();
    }

    public IEnumerator BotTurnRoutine()
    {
        InitializeVariables();

        yield return WaitForSeconds(1.5f);

        if (enemyHandCards.AnyNotNull() && enemyFieldCards.AnyNull())
            yield return PlayBotCard();

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
        CardSlot filledSlot = gameManager.EnemyHandManager.GetRandomFilledSlot();
        Transform cardTransform = filledSlot.CurrentCard.transform;

        if (cardTransform.TryGetComponent<CardMovemantScript>(out var card) &&
            cardTransform.TryGetComponent<CardControllerLink>(out var link))
        {
            CardSlot emptySlot = gameManager.EnemyFieldManager.GetRandomEmptySlot();
            yield return card.MoveCardTransformToField(emptySlot);
            filledSlot.Clear();

            enemyHandCards[(int)filledSlot.indexSlotInField] = null;
            enemyFieldCards[(int)emptySlot.indexSlotInField] = link.Controller;

            link.Controller.MarkAsPlayedInThisTurn(CardOwner.Enemy);
        }
    }

    // private IEnumerator Attack()
    // {
    //     // foreach (BattleCard3DController attacker in botTurnManager.GetAttackableCards(false))
    //     // {
    //     //     if (playerFieldCards.AnyNotNull())
    //     //     {
    //     //         Transform cardTransform = attacker.BattleCardView.CardRoot.transform;

    //     //         CardSlot filledSlot = gameManager.PlayerFieldManager.GetRandomFilledSlot();
    //     //         Transform targetTransform = filledSlot.CurrentCard.transform;
    //     //         BattleCard3DController defender = playerFieldCards[(int)filledSlot.indexSlotInField];

    //     //         if (cardTransform.TryGetComponent<Card3DAttackDragHandler>(out var card))
    //     //             yield return card.MoveEnemyCardTransformForAttack(targetTransform, attacker, defender);

    //     //         yield return WaitForSeconds(1f);
    //     //     }
    //     // }
    // }

    private IEnumerator Attack()
    {
        List<BattleCardController> attackers = botTurnManager.GetAttackableCards(false).ToList();

        foreach (BattleCardController attacker in attackers)
        {
            int attackerSlotIndex = (int)attacker.BattleCardView.CardRoot.transform.parent.GetComponent<CardSlot>().indexSlotInField;

            List<(int index, BattleCardController card)> reachableTargets = new();

            for (int i = 0; i < playerFieldCards.Length; i++)
            {
                if (playerFieldCards[i] == null) continue;
                if ((attackerSlotIndex == 0 && i == 2) || (attackerSlotIndex == 2 && i == 0)) continue;
                reachableTargets.Add((i, playerFieldCards[i]));
            }

            if (!reachableTargets.Any()) continue;

            List<(int index, Ability ability)> attackingAbilities = new();

            if (attacker.CardModel.abilities.firstAbility.type == AbilityType.Physical || attacker.CardModel.abilities.firstAbility.type == AbilityType.Special)
                attackingAbilities.Add((0, attacker.CardModel.abilities.firstAbility));

            if (attacker.CardModel.abilities.secondAbility.type == AbilityType.Physical || attacker.CardModel.abilities.secondAbility.type == AbilityType.Special)
                attackingAbilities.Add((1, attacker.CardModel.abilities.secondAbility));

            if (attacker.CardModel.abilities.thirdAbility.type == AbilityType.Physical || attacker.CardModel.abilities.thirdAbility.type == AbilityType.Special)
                attackingAbilities.Add((2, attacker.CardModel.abilities.thirdAbility));

            if (attacker.CardModel.abilities.fourthAbility.type == AbilityType.Physical || attacker.CardModel.abilities.fourthAbility.type == AbilityType.Special)
                attackingAbilities.Add((3, attacker.CardModel.abilities.fourthAbility));

            if (!attackingAbilities.Any()) continue;

            (int index, BattleCardController card) chosenTarget = reachableTargets[Random.Range(0, reachableTargets.Count)];
            BattleCardController defender = chosenTarget.card;

            List<(int index, Ability ability, float multiplier)> abilityMultipliers = new();

            foreach ((int index, Ability ability) in attackingAbilities)
            {
                float multiplier = typeChart.GetMultiplier(ability.element.Value, defender.CardModel.mainElement);
                if (defender.CardModel.secondaryElement.HasValue)
                    multiplier *= typeChart.GetMultiplier(ability.element.Value, defender.CardModel.secondaryElement.Value);

                abilityMultipliers.Add((index, ability, multiplier));
            }

            float bestMultiplier = abilityMultipliers.Max(a => a.multiplier);
            List<(int index, Ability ability, float multiplier)> bestAbilities = abilityMultipliers.Where(a => a.multiplier == bestMultiplier).ToList();

            (int index, Ability ability, float multiplier) selected = bestAbilities.Count > 1
                ? bestAbilities[Random.Range(0, bestAbilities.Count)]
                : bestAbilities[0];

            attacker.SelectAbilityByIndex(selected.index);

            Transform cardTransform = attacker.BattleCardView.CardRoot.transform;
            if (cardTransform.TryGetComponent<CardAttackExecutor>(out var attackExecutor))
                yield return StartCoroutine(attackExecutor.ExecuteAttackRoutine(attacker, defender));

            yield return WaitForSeconds(1f);
        }
    }

    private IEnumerator WaitForSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
    }
}
