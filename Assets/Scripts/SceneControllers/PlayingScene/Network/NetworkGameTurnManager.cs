using Fusion;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NetworkGameTurnManager : NetworkBehaviour
{
    private GameManager gameManager;
    private NetworkGameController networkGameController;

    [Header("Network")]
    [Networked] public bool IsFirstPlayerTurn { get; private set; }
    [Networked] public int RoundNumber { get; private set; }
    [Networked] public float TurnDuration { get; set; }
    [Networked] public float TurnEndTime { get; set; }
    [Networked] public bool TurnManagerInitialized { get; private set; }

    private const float TURN_DURATION = 90f;

    public TurnPhase CurrentPhase { get; private set; } = TurnPhase.Waiting;

    private BattleCardController[] playerFieldCards, playerHandCards, enemyFieldCards;

    public event Action<int> OnTurnStarted;
    public event Action<TurnPhase> OnPhaseChanged;

    public void FindNetworkGameController() => networkGameController = FindAnyObjectByType<NetworkGameController>();
    public void FindGameManager() => gameManager = FindAnyObjectByType<GameManager>();

    public void FindObjects()
    {
        FindNetworkGameController();
        FindGameManager();
    }

    public bool IsMyTurn()
    {
        if (!TurnManagerInitialized) return false;
        if (networkGameController == null) FindNetworkGameController();
        return (networkGameController.IsFirstPlayer && IsFirstPlayerTurn) ||
                (networkGameController.IsSecondPlayer && !IsFirstPlayerTurn);
    }

    public void InitializeVariables()
    {
        playerFieldCards = gameManager.CurrentGame.PlayerFieldControllers;
        playerHandCards = gameManager.CurrentGame.PlayerHandControllers;
        enemyFieldCards = gameManager.CurrentGame.EnemyFieldControllers;
    }

    public override void FixedUpdateNetwork()
    {
        if (HasStateAuthority && !TurnManagerInitialized)
        {
            if (networkGameController != null &&
                networkGameController.IsFirstPlayerGameFullLoaded &&
                networkGameController.IsSecondPlayerGameFullLoaded)
            {
                StartFirstTurn();
            }
        }
    }

    public void StartFirstTurn()
    {
        IsFirstPlayerTurn = true;
        RoundNumber = 1;
        TurnDuration = TURN_DURATION;
        TurnEndTime = Runner.SimulationTime + TurnDuration;
        TurnManagerInitialized = true;

        RpcNotifyTurnStarted();
    }

    private void StartTurn()
    {
        IsFirstPlayerTurn = !IsFirstPlayerTurn;
        if (IsFirstPlayerTurn) RoundNumber++;
        TurnEndTime = Runner.SimulationTime + TurnDuration;
        RpcNotifyTurnStarted();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RpcNotifyTurnStarted()
    {
        OnTurnStarted?.Invoke(RoundNumber);

        if (IsMyTurn())
            GoToPlayCardPhase();
        else
        {
            CurrentPhase = TurnPhase.Waiting;
            OnPhaseChanged?.Invoke(CurrentPhase);
        }
    }

    public void GoToPlayCardPhase()
    {
        InitializeVariables();

        CurrentPhase = TurnPhase.PlayCard;

        NotificationManager.ShowNotification($"Фаза: {CurrentPhase}", NotificationType.Info, 1f);

        if ((playerFieldCards.AnyNull() && playerHandCards.IsEmpty()) || playerFieldCards.IsFull())
            GoToAttackPhase();
        else
            OnPhaseChanged?.Invoke(CurrentPhase);
    }

    public void GoToAttackPhase()
    {
        InitializeVariables();

        if (!CheckAttackAvailability()) RequestEndTurn();
        else
        {
            CurrentPhase = TurnPhase.Attack;
            OnPhaseChanged?.Invoke(CurrentPhase);

            NotificationManager.ShowNotification($"Фаза: {CurrentPhase}", NotificationType.Info, 1f);

            foreach (BattleCardController card in playerFieldCards.GetAllNotNull())
                card.BattleCardView.ApplyBattleStyle(card.BattleState);
        }
    }

    private bool CheckAttackAvailability()
    {
        bool canAttack = GetAttackableCards(IsMyTurn()).Any();
        if (!canAttack) return false;
        else return true;
    }

    public IEnumerable<BattleCardController> GetAttackableCards(bool forFirstPlayer)
    {
        InitializeVariables();

        BattleCardController[] array = forFirstPlayer ? playerFieldCards : enemyFieldCards;
        return array.Where(c => c != null && c.CanAttack);
    }

    public void RequestEndTurn()
    {
        ResetTurnFlagsForAllCards();
        RpcEndTurn();
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RpcEndTurn() => StartTurn();

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
