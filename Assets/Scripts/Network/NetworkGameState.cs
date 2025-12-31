using Fusion;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkGameState : NetworkBehaviour
{
    [Networked] public NetworkString<_512> FirstPlayerDeckCsv { get; private set; }
    [Networked] public NetworkString<_512> SecondPlayerDeckCsv { get; private set; }

    [Networked] public bool FirstPlayerReady { get; set; }
    [Networked] public bool SecondPlayerReady { get; set; }

    [Networked] public PlayerRef FirstPlayerRef { get; set; }
    [Networked] public PlayerRef SecondPlayerRef { get; set; }

    [Networked] public NetworkString<_32> FirstPlayerName { get; set; }
    [Networked] public NetworkString<_32> SecondPlayerName { get; set; }

    [Networked] public bool IsFirstPlayerTurn { get; set; }
    [Networked] public bool IsSecondPlayerTurn { get; set; }
    [Networked] public int RoundNumber { get; set; }
    [Networked] public float RoundPhaseEndTime { get; set; }
    [Networked] public int MovesInRound { get; set; }


    public bool IsFirstPlayer => Runner.LocalPlayer == FirstPlayerRef;
    public bool IsSecondPlayer => Runner.LocalPlayer == SecondPlayerRef;

    private GameManagerScript gameManager;
    private bool initialized = false;

    public void BindGameManager(GameManagerScript gameManager) => this.gameManager = gameManager;

    public override void Spawned()
    {
        NotificationManager.ShowNotification("Готов", NotificationType.Info);
        if (HasStateAuthority)
        {
            List<int> ids = SelectedDeckManager.GetSelectedDeckIds();
            if (ids != null && ids.Count > 0)
                SetDeckCsv(true, ids);
        }
        else
        {
            string csv = SelectedDeckManager.GetSelectedDeckCsv();
            if (!string.IsNullOrEmpty(csv))
                RpcSubmitDeck(csv);
        }
    }

    public override void Render()
    {
        base.Render();

        if (FirstPlayerReady && SecondPlayerReady && !initialized)
        {
            initialized = true;

            if (HasStateAuthority)
            {
                StartTurnTimer(30f);
                int index = SceneUtility.GetBuildIndexByScenePath("Assets/Scenes/PlayingScene.unity");
                Runner.LoadScene(SceneRef.FromIndex(index));
                Debug.Log("[NetworkGameState] Both ready → LoadScene('PlayingScene')");
            }
        }
    }

    public void StartTurnTimer(float durationSeconds)
    {
        if (!HasStateAuthority) return;

        RoundPhaseEndTime = Runner.SimulationTime + durationSeconds;
        IsFirstPlayerTurn = true;
        IsSecondPlayerTurn = false;
        RoundNumber = 1;
        MovesInRound = 0;
        RpcNotifyRoundPhaseChanged(RoundNumber);
    }

    public void AssignPlayerRole(PlayerRef player)
    {
        if (HasStateAuthority)
        {
            if (FirstPlayerRef == default)
            {
                FirstPlayerRef = player;
                FirstPlayerName = UserSession.Instance.ActiveUser.userData.userName;
            }
            else if (SecondPlayerRef == default)
            {
                SecondPlayerRef = player;
                SecondPlayerName = UserSession.Instance.ActiveUser.userData.userName;
            }
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
    public void RpcSubmitDeck(string csv, RpcInfo info = default)
    {
        List<int> ids = SelectedDeckManager.ParseCsv(csv);
        if (info.Source == SecondPlayerRef)
            SetDeckCsv(false, ids);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RpcRequestEndRoundPhase(RpcInfo info = default)
    {
        if (!IsValidTurnRequest(info.Source)) return;
        if (MovesInRound == 2)
            EndRound();
    }

    public void SwitchRoundPhase()
    {
        IsFirstPlayerTurn = !IsFirstPlayerTurn;
        IsSecondPlayerTurn = !IsSecondPlayerTurn;
        RoundPhaseEndTime = Runner.SimulationTime + 30f;
        RpcNotifyRoundPhaseChanged(RoundNumber);
    }

    private async void EndRound()
    {
        RoundNumber++;
        MovesInRound = 0;
        SwitchRoundPhase();
        await Task.Delay(500);
        RpcRequestCardsFight();
        CheckGameEnd();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RpcNotifyRoundPhaseChanged(int turnNumber)
    {
        if (gameManager != null)
            gameManager.OnTurnChanged(turnNumber);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RpcRequestPlayCard(int cardId, int siblingIndex, RpcInfo info = default)
    {
        if (!IsValidTurnRequest(info.Source)) return;
        bool isFirstPlayer = info.Source == FirstPlayerRef;
        RpcPlayCard(cardId, siblingIndex, isFirstPlayer);
        MovesInRound++;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RpcPlayCard(int playedCardId, int siblingIndex, bool isFirstPlayer)
    {
        if (gameManager != null)
            gameManager.OnCardPlayed(playedCardId, siblingIndex, isFirstPlayer);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RpcRequestCardsFight()
    {
        if (gameManager != null)
        {
            TypeChart chart = gameManager.typeChart;
            BattleCardController attacker = gameManager.CurrentGame.PlayerFieldListController.CardControllers.LastOrDefault();
            BattleCardController defender = gameManager.CurrentGame.EnemyFieldListController.CardControllers.LastOrDefault();
            ResolveBattle(attacker, defender, chart);
        }
    }

    private void ResolveBattle(BattleCardController attacker, BattleCardController defender, TypeChart chart)
    {
        if (attacker == null || defender == null) return;

        float attackerMultiplier = chart.GetMultiplier(attacker.CardModel.mainElement, defender.CardModel.mainElement);
        float defenderMultiplier = chart.GetMultiplier(defender.CardModel.mainElement, attacker.CardModel.mainElement);

        string resultMessage;

        if (attackerMultiplier > defenderMultiplier)
        {
            gameManager.MoveCardToReset(defender, false);
            resultMessage = $"{attacker.CardModel.title} победил {defender.CardModel.title}, потому что {attacker.CardModel.mainElement} сильнее {defender.CardModel.mainElement}";
        }
        else if (attackerMultiplier < defenderMultiplier)
        {
            gameManager.MoveCardToReset(attacker, true);
            resultMessage = $"{defender.CardModel.title} победил {attacker.CardModel.title}, потому что {defender.CardModel.mainElement} сильнее {attacker.CardModel.mainElement}";
        }
        else
        {
            gameManager.MoveCardToReset(attacker, true);
            gameManager.MoveCardToReset(defender, false);
            resultMessage = $"{attacker.CardModel.title} и {defender.CardModel.title} равны по силе, оба отправлены в сброс";
        }

        NotificationManager.ShowNotification(resultMessage, NotificationType.Info);
    }

    private bool IsValidTurnRequest(PlayerRef player)
    {
        if (player == FirstPlayerRef) return IsFirstPlayerTurn;
        if (player == SecondPlayerRef) return IsSecondPlayerTurn;
        return false;
    }

    public List<int> GetPlayerDeckIds()
    {
        return IsFirstPlayer
            ? SelectedDeckManager.ParseCsv(FirstPlayerDeckCsv.Value)
            : SelectedDeckManager.ParseCsv(SecondPlayerDeckCsv.Value);
    }

    public List<int> GetEnemyDeckIds()
    {
        return IsFirstPlayer
            ? SelectedDeckManager.ParseCsv(SecondPlayerDeckCsv.Value)
            : SelectedDeckManager.ParseCsv(FirstPlayerDeckCsv.Value);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RpcGameOver()
    {
        if (gameManager != null)
        {
            gameManager.ShowGameOverOverlay();
            StartCoroutine(LoadSceneAfterDelay(3f));
        }
    }

    private IEnumerator LoadSceneAfterDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        SceneManager.LoadScene("CollectionScene");

        NetworkRunnerHandler networkRunnerHandler = FindAnyObjectByType<NetworkRunnerHandler>();
        if (networkRunnerHandler != null)
            networkRunnerHandler.ShutdownRunner();
    }

    private void CheckGameEnd()
    {
        int playerFieldCount = gameManager.CurrentGame.PlayerFieldListController.CardControllers.Count;
        int enemyFieldCount = gameManager.CurrentGame.EnemyFieldListController.CardControllers.Count;
        int playerHandCount = gameManager.CurrentGame.PlayerHandListController.CardControllers.Count;
        int enemyHandCount = gameManager.CurrentGame.EnemyHandListController.CardControllers.Count;

        if (playerFieldCount == 3 || enemyFieldCount == 3 || playerHandCount == 0 || enemyHandCount == 0)
        {
            RpcGameOver();
        }
    }
}