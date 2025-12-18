using Fusion;
using System;
using System.Collections.Generic;
using System.Linq;

public class NetworkGameState : NetworkBehaviour
{
    [Networked] public NetworkString<_512> FirstPlayerDeckCsv { get; private set; }
    [Networked] public NetworkString<_512> SecondPlayerDeckCsv { get; private set; }

    [Networked] public bool FirstPlayerReady { get; set; }
    [Networked] public bool SecondPlayerReady { get; set; }

    [Networked] public bool IsFirstPlayerTurn { get; set; }
    [Networked] public bool IsSecondPlayerTurn { get; set; }

    [Networked] public int TurnNumber { get; set; }

    public bool IsFirstPlayer => Runner.IsServer;
    public bool IsSecondPlayer => !Runner.IsServer;

    private bool initialized = false;
    private GameManagerScript gameManager;

    public override void Spawned()
    {
        NotificationManager.ShowNotification("Готов");

        gameManager = FindAnyObjectByType<GameManagerScript>();

        if (Runner.IsServer)
        {
            List<int> ids = SelectedDeckManager.GetSelectedDeckIds();
            SetDeckCsv(true, ids);
        }
        else
        {
            string csv = SelectedDeckManager.GetSelectedDeckCsv();
            RpcSubmitDeck(csv);
        }
    }

    public override void Render()
    {
        base.Render();

        if (FirstPlayerReady && SecondPlayerReady && !initialized)
        {
            initialized = true;
            gameManager.Init(this);
        }
    }

    public void SetDeckCsv(bool isFirstPlayer, List<int> ids)
    {
        string csv = ids == null || ids.Count == 0 ? string.Empty : string.Join(",", ids);

        if (isFirstPlayer)
        {
            FirstPlayerDeckCsv = csv;
            FirstPlayerReady = true;
        }
        else
        {
            SecondPlayerDeckCsv = csv;
            SecondPlayerReady = true;
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RpcSubmitDeck(string csv)
    {
        List<int> ids = SelectedDeckManager.ParseCsv(csv);
        SetDeckCsv(false, ids);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RpcRequestEndTurn(RpcInfo info = default)
    {
        SwitchTurn();
    }

    public void SwitchTurn()
    {
        IsFirstPlayerTurn = !IsFirstPlayerTurn;
        IsSecondPlayerTurn = !IsSecondPlayerTurn;

        if (IsFirstPlayerTurn)
            TurnNumber++;

        RpcNotifyTurnChanged(TurnNumber);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RpcNotifyTurnChanged(int turnNumber)
    {
        FindAnyObjectByType<GameManagerScript>().OnTurnChanged(turnNumber);
    }

    // Клиент -> Сервер
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RpcRequestPlayCard(int cardId, int siblingIndex, RpcInfo info = default)
    {
        bool isFirstPlayer = info.Source.RawEncoded == 0;
        RpcPlayCard(cardId, siblingIndex, isFirstPlayer);
    }

    // Сервер -> Всем
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RpcPlayCard(int playedCardId, int siblingIndex, bool isFirstPlayer)
    {
        FindAnyObjectByType<GameManagerScript>().OnCardPlayed(playedCardId, siblingIndex, isFirstPlayer);
    }




    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RpcRequestCardsFight(int attackerId, int defenderId)
    {
        GameManagerScript gameManager = FindAnyObjectByType<GameManagerScript>();
        TypeChart chart = gameManager.typeChart;

        BattleCardController attacker = gameManager.CurrentGame.PlayerFieldListController.CardControllers.FirstOrDefault(c => c.CardModel.id == attackerId)
            ?? gameManager.CurrentGame.EnemyFieldListController.CardControllers
            .FirstOrDefault(c => c.CardModel.id == attackerId);

        BattleCardController defender = gameManager.CurrentGame.PlayerFieldListController.CardControllers.FirstOrDefault(c => c.CardModel.id == defenderId)
            ?? gameManager.CurrentGame.EnemyFieldListController.CardControllers
            .FirstOrDefault(c => c.CardModel.id == defenderId);

        ResolveBattle(attacker, defender, chart);
    }

    // public void CheckForBattle()
    // {
    //     GameManagerScript gameManager = FindAnyObjectByType<GameManagerScript>();
    //     TypeChart chart = gameManager.typeChart;

    //     BattleCardController playerCard = gameManager.CurrentGame.PlayerFieldListController.CardControllers.LastOrDefault();
    //     BattleCardController enemyCard = gameManager.CurrentGame.EnemyFieldListController.CardControllers.LastOrDefault();

    //     ResolveBattle(playerCard, enemyCard, chart);
    // }


    private void ResolveBattle(BattleCardController attacker, BattleCardController defender, TypeChart chart)
    {
        if (attacker == null || defender == null) return;

        // float attackerMultiplier = chart.GetMultiplier(attacker.CardModel.mainElement, defender.CardModel.mainElement);
        // float defenderMultiplier = chart.GetMultiplier(defender.CardModel.mainElement, attacker.CardModel.mainElement);

        // if (attackerMultiplier > defenderMultiplier)
        // {
        //     RpcDestroyCard(defender.CardModel.id);
        // }
        // else if (attackerMultiplier < defenderMultiplier)
        // {
        //     RpcDestroyCard(attacker.CardModel.id);
        // }
        // else
        // {
        RpcDestroyCard(attacker.CardModel.id);
        RpcDestroyCard(defender.CardModel.id);
        // }
    }


    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RpcDestroyCard(int cardId)
    {
        GameManagerScript gameManager = FindAnyObjectByType<GameManagerScript>();
        gameManager.DestroyCardById(cardId);
    }



    public List<int> GetPlayerDeckIds() => SelectedDeckManager.ParseCsv(FirstPlayerDeckCsv.Value);
    public List<int> GetEnemyDeckIds() => SelectedDeckManager.ParseCsv(SecondPlayerDeckCsv.Value);
}