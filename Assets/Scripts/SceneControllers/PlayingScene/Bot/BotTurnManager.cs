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

// public class BotTurnManager : MonoBehaviour
// {
//     [SerializeField] private GameManagerScript gameManager;
//     [SerializeField] private BotController botController;

//     public bool IsPlayerTurn { get; private set; } = false;
//     public int RoundNumber { get; private set; } = 0;
//     public TurnPhase CurrentPhase { get; private set; } = TurnPhase.PlayCard;

//     public float TurnDuration { get; set; } = 30f;
//     public float TurnEndTime { get; set; } = 0f;

//     private List<BattleCardController> playerFieldCards, playerHandCards, enemyFieldCards;

//     public event Action<int> OnTurnStarted;
//     public event Action<TurnPhase> OnPhaseChanged;

//     public void StartFirstTurn()
//     {
//         InitializeVariables();
//         StartTurn();
//     }

//     private void InitializeVariables()
//     {
//         playerFieldCards = gameManager.CurrentGame.PlayerFieldListController.CardControllers;
//         playerHandCards = gameManager.CurrentGame.PlayerHandListController.CardControllers;
//         enemyFieldCards = gameManager.CurrentGame.EnemyFieldListController.CardControllers;
//     }

//     private void StartTurn()
//     {
//         IsPlayerTurn = !IsPlayerTurn;
//         if (IsPlayerTurn) RoundNumber++;

//         GoToPlayCardPhase();

//         TurnEndTime = Time.time + TurnDuration;

//         OnTurnStarted?.Invoke(RoundNumber);
//     }

//     public void GoToPlayCardPhase()
//     {
//         CurrentPhase = TurnPhase.PlayCard;

//         if (IsPlayerTurn)
//         {
//             NotificationManager.ShowNotification($"Фаза: {CurrentPhase}", NotificationType.Info);
//         }

//         OnPhaseChanged?.Invoke(CurrentPhase);

//         if (IsPlayerTurn)
//         {
//             if ((playerFieldCards.Count < 3 && playerHandCards.Count == 0) ||
//                 playerFieldCards.Count == 3)
//             {
//                 GoToAttackPhase();
//             }
//         }
//     }

//     public void GoToAttackPhase()
//     {
//         CurrentPhase = TurnPhase.Attack;

//         if (IsPlayerTurn)
//         {
//             NotificationManager.ShowNotification($"Фаза: {CurrentPhase}", NotificationType.Info);

//             foreach (BattleCardController card in playerFieldCards)
//                 card.BattleCardView.ApplyBattleStyle(card.BattleState);
//         }

//         OnPhaseChanged?.Invoke(CurrentPhase);

//         CheckAttackAvailability();
//     }

//     public void EndTurn()
//     {
//         ResetTurnFlagsForAllCards();

//         StartTurn();

//         if (!IsPlayerTurn)
//             botController.EndPlayerTurn();
//     }

//     public IEnumerable<BattleCardController> GetAttackableCards(bool forPlayer)
//     {
//         List<BattleCardController> list = forPlayer ? playerFieldCards : enemyFieldCards;
//         return list.Where(c => c != null && c.CanAttack);
//     }

//     public void CheckAttackAvailability()
//     {
//         bool canAttack = GetAttackableCards(IsPlayerTurn).Any();
//         if (!canAttack) EndTurn();
//     }

//     private void ResetTurnFlagsForAllCards()
//     {
//         foreach (BattleCardController card in playerFieldCards)
//         {
//             card.ResetTurnFlags();
//             card.BattleCardView.ResetBattleStyle();
//         }

//         foreach (BattleCardController card in enemyFieldCards)
//         {
//             card.ResetTurnFlags();
//         }
//     }
// }

public class BotTurnManager : MonoBehaviour
{
    [SerializeField] private GameManagerScript gameManager;
    [SerializeField] private BotController botController;

    public bool IsPlayerTurn { get; private set; }
    public int RoundNumber { get; private set; }
    public float TurnDuration { get; set; }
    public float TurnEndTime { get; set; }

    private const float TURN_DURATION = 30f;

    public TurnPhase CurrentPhase { get; private set; } = TurnPhase.PlayCard;

    private List<BattleCardController> playerFieldCards, playerHandCards, enemyFieldCards;

    public event Action<int> OnTurnStarted;
    public event Action<TurnPhase> OnPhaseChanged;

    public void StartFirstTurn()
    {
        InitializeVariables();

        IsPlayerTurn = true;
        RoundNumber = 1;
        TurnDuration = TURN_DURATION;
        TurnEndTime = Time.time + TurnDuration;

        GoToPlayCardPhase();

        OnTurnStarted?.Invoke(RoundNumber);
    }

    private void InitializeVariables()
    {
        playerFieldCards = gameManager.CurrentGame.PlayerFieldListController.CardControllers;
        playerHandCards = gameManager.CurrentGame.PlayerHandListController.CardControllers;
        enemyFieldCards = gameManager.CurrentGame.EnemyFieldListController.CardControllers;
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
        CurrentPhase = TurnPhase.PlayCard;

        // NotificationManager.ShowNotification($"Фаза: {CurrentPhase}", NotificationType.Info);

        if ((playerFieldCards.Count < 3 && playerHandCards.Count == 0) ||
            playerFieldCards.Count == 3)
        {
            GoToAttackPhase();
        }
        else
        {
            OnPhaseChanged?.Invoke(CurrentPhase);
        }
    }

    public void GoToAttackPhase()
    {
        CurrentPhase = TurnPhase.Attack;

        // NotificationManager.ShowNotification($"Фаза: {CurrentPhase}", NotificationType.Info);

        foreach (BattleCardController card in playerFieldCards)
            card.BattleCardView.ApplyBattleStyle(card.BattleState);

        CheckAttackAvailability();
    }

    public void EndTurn()
    {
        ResetTurnFlagsForAllCards();
        StartTurn();
    }

    public IEnumerable<BattleCardController> GetAttackableCards(bool forPlayer)
    {
        List<BattleCardController> list = forPlayer ? playerFieldCards : enemyFieldCards;
        return list.Where(c => c != null && c.CanAttack);
    }

    public void CheckAttackAvailability()
    {
        bool canAttack = GetAttackableCards(IsPlayerTurn).Any();
        if (!canAttack) EndTurn();
        else OnPhaseChanged?.Invoke(CurrentPhase);
    }

    public void ResetTurnFlagsForAllCards()
    {
        foreach (BattleCardController card in playerFieldCards)
        {
            card.ResetTurnFlags();
            card.BattleCardView.ResetBattleStyle();
        }

        foreach (BattleCardController card in enemyFieldCards)
            card.ResetTurnFlags();
    }
}
