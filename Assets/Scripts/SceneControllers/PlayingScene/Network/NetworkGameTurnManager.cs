using Fusion;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NetworkGameTurnManager : NetworkBehaviour
{
    [SerializeField] private GameManagerScript gameManager;

    private NetworkGameController networkGameController;

    [Header("Network")]
    [Networked] public bool IsFirstPlayerTurn { get; private set; }
    [Networked] public int RoundNumber { get; private set; }
    [Networked] public float TurnDuration { get; set; }
    [Networked] public float TurnEndTime { get; set; }
    [Networked] public bool TurnManagerInitialized { get; private set; }

    private const float TURN_DURATION = 30f;

    public TurnPhase CurrentPhase { get; private set; } = TurnPhase.Waiting;

    private List<BattleCardController> playerFieldCards, playerHandCards, enemyFieldCards;

    public event Action<int> OnTurnStarted;
    public event Action<TurnPhase> OnPhaseChanged;

    public void FindNetworkGameController() => networkGameController = FindAnyObjectByType<NetworkGameController>();

    public void FindObjects()
    {
        FindNetworkGameController();
        gameManager = FindAnyObjectByType<GameManagerScript>();
    }

    public bool IsMyTurn()
    {
        if (!TurnManagerInitialized) return false;
        if (networkGameController == null) FindNetworkGameController();
        return (networkGameController.IsFirstPlayer && IsFirstPlayerTurn) ||
                (networkGameController.IsSecondPlayer && !IsFirstPlayerTurn);
    }

    private void InitializeVariables()
    {
        playerFieldCards = gameManager.CurrentGame.PlayerFieldListController.CardControllers;
        playerHandCards = gameManager.CurrentGame.PlayerHandListController.CardControllers;
        enemyFieldCards = gameManager.CurrentGame.EnemyFieldListController.CardControllers;
    }

    public void StartFirstTurn()
    {
        InitializeVariables();

        if (HasStateAuthority)
        {
            IsFirstPlayerTurn = true;
            RoundNumber = 1;
            TurnDuration = TURN_DURATION;
            TurnEndTime = Runner.SimulationTime + TurnDuration;
            TurnManagerInitialized = true;
            RpcNotifyTurnStarted();
        }
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

    public override void Spawned() { }

    public void GoToPlayCardPhase()
    {
        CurrentPhase = TurnPhase.PlayCard;

        // NotificationManager.ShowNotification($"Фаза: {CurrentPhase}", NotificationType.Info);

        if ((playerFieldCards.Count < 3 && playerHandCards.Count == 0) ||
            playerFieldCards.Count == 3)
            GoToAttackPhase();
        else
            OnPhaseChanged?.Invoke(CurrentPhase);
    }

    public void GoToAttackPhase()
    {
        CurrentPhase = TurnPhase.Attack;

        // NotificationManager.ShowNotification($"Фаза: {CurrentPhase}", NotificationType.Info);

        foreach (BattleCardController card in playerFieldCards)
            card.BattleCardView.ApplyBattleStyle(card.BattleState);

        CheckAttackAvailability();
    }

    public void RequestEndTurn()
    {
        ResetTurnFlagsForAllCards();
        RpcEndTurn();
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RpcEndTurn()
    {
        StartTurn();
    }

    public IEnumerable<BattleCardController> GetAttackableCards(bool forFirstPlayer)
    {
        List<BattleCardController> list = forFirstPlayer ? playerFieldCards : enemyFieldCards;
        return list.Where(c => c != null && c.CanAttack);
    }

    private void CheckAttackAvailability()
    {
        bool canAttack = GetAttackableCards(IsMyTurn()).Any();
        if (!canAttack) RequestEndTurn();
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
        {
            card.ResetTurnFlags();
        }
    }
}
