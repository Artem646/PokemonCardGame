using Fusion;
using System.Collections.Generic;

public class NetworkGameState : NetworkBehaviour
{
    [Networked] public NetworkString<_512> FirstPlayerDeckCsv { get; private set; }
    [Networked] public NetworkString<_512> SecondPlayerDeckCsv { get; private set; }

    [Networked] public bool FirstPlayerReady { get; set; }
    [Networked] public bool SecondPlayerReady { get; set; }

    [Networked] public bool IsFirstPlayerTurn { get; set; }
    [Networked] public bool IsSecondPlayerTurn { get; set; }

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
        RpcNotifyTurnChanged(IsFirstPlayerTurn, IsSecondPlayerTurn);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RpcNotifyTurnChanged(bool firstTurn, bool secondTurn)
    {
        FindAnyObjectByType<GameManagerScript>().OnTurnChanged(firstTurn, secondTurn);
    }

    // Клиент -> Сервер
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RpcRequestPlayCard(int cardId, int siblingIndex, bool toField, RpcInfo info = default)
    {
        bool isFirstPlayer = info.Source.RawEncoded == 0;
        RpcPlayCard(cardId, siblingIndex, isFirstPlayer, toField);
    }

    // Сервер -> Всем
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RpcPlayCard(int playedCardId, int siblingIndex, bool isFirstPlayer, bool toField)
    {
        FindAnyObjectByType<GameManagerScript>().OnCardPlayed(playedCardId, siblingIndex, isFirstPlayer, toField);
    }

    public List<int> GetPlayerDeckIds() => SelectedDeckManager.ParseCsv(FirstPlayerDeckCsv.Value);
    public List<int> GetEnemyDeckIds() => SelectedDeckManager.ParseCsv(SecondPlayerDeckCsv.Value);
}