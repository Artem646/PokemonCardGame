using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum TurnPhase
{
    PlayCard,
    Attack,
    Waiting
}

public class BotTurnManager : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private BotController botController;

    public bool IsPlayerTurn { get; private set; }
    public int RoundNumber { get; private set; }
    public float TurnDuration { get; set; } = TURN_DURATION;
    public float TurnEndTime { get; set; }

    private const float TURN_DURATION = 90f;

    public TurnPhase CurrentPhase { get; private set; } = TurnPhase.PlayCard;

    private BattleCardController[] playerFieldCards, playerHandCards, enemyFieldCards;

    public event Action<int> OnTurnStarted;
    public event Action<TurnPhase> OnPhaseChanged;

    public void StartFirstTurn()
    {
        IsPlayerTurn = true;
        RoundNumber = 1;
        TurnDuration = TURN_DURATION;
        TurnEndTime = Time.time + TurnDuration;

        GoToPlayCardPhase();

        OnTurnStarted?.Invoke(RoundNumber);
    }

    private void InitializeVariables()
    {
        playerHandCards = gameManager.CurrentGame.PlayerHandControllers;
        playerFieldCards = gameManager.CurrentGame.PlayerFieldControllers;
        enemyFieldCards = gameManager.CurrentGame.EnemyFieldControllers;
    }

    private void StartTurn()
    {
        IsPlayerTurn = !IsPlayerTurn;
        if (IsPlayerTurn) RoundNumber++;

        TurnEndTime = Time.time + TurnDuration;

        if (IsPlayerTurn)
            GoToPlayCardPhase();
        else
        {
            CurrentPhase = TurnPhase.Waiting;
            OnPhaseChanged?.Invoke(CurrentPhase);

            botController.EndPlayerTurn();
        }

        OnTurnStarted?.Invoke(RoundNumber);
    }

    public void GoToPlayCardPhase()
    {
        InitializeVariables();

        CurrentPhase = TurnPhase.PlayCard;

        if ((playerFieldCards.AnyNull() && playerHandCards.IsEmpty()) || playerFieldCards.IsFull())
            GoToAttackPhase();
        else
            OnPhaseChanged?.Invoke(CurrentPhase);
    }

    public void GoToAttackPhase()
    {
        InitializeVariables();

        CurrentPhase = TurnPhase.Attack;

        foreach (BattleCardController card in playerFieldCards.GetAllNotNull())
            card.BattleCardView.ApplyBattleStyle(card.BattleState);

        CheckAttackAvailability();
    }

    public void CheckAttackAvailability()
    {
        bool canAttack = GetAttackableCards(IsPlayerTurn).Any();
        if (!canAttack) EndTurn();
        else OnPhaseChanged?.Invoke(CurrentPhase);
    }

    public IEnumerable<BattleCardController> GetAttackableCards(bool forPlayer)
    {
        InitializeVariables();

        BattleCardController[] array = forPlayer ? playerFieldCards : enemyFieldCards;
        return array.Where(c => c != null && c.CanAttack);
    }

    public void EndTurn()
    {
        ResetTurnFlagsForAllCards();
        StartTurn();
    }

    public void ResetTurnFlagsForAllCards()
    {
        InitializeVariables();

        foreach (BattleCardController card in playerFieldCards.GetAllNotNull())
        {
            card.ResetTurnFlags();
            card.BattleCardView.ResetBattleStyle();
        }

        foreach (BattleCardController card in enemyFieldCards.GetAllNotNull())
            card.ResetTurnFlags();
    }
}
