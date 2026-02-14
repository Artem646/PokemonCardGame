using Fusion;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkGameController : NetworkBehaviour
{
    private GameManagerScript gameManager;

    [Networked] public PlayerRef FirstPlayerRef { get; private set; }
    [Networked] public PlayerRef SecondPlayerRef { get; private set; }

    [Networked] public NetworkString<_32> FirstPlayerName { get; private set; }
    [Networked] public NetworkString<_32> SecondPlayerName { get; private set; }

    [Networked] public NetworkString<_512> FirstPlayerDeckCsv { get; private set; }
    [Networked] public NetworkString<_512> SecondPlayerDeckCsv { get; private set; }

    [Networked] public bool FirstPlayerReady { get; private set; }
    [Networked] public bool SecondPlayerReady { get; private set; }

    public bool IsFirstPlayer => Runner.LocalPlayer == FirstPlayerRef;
    public bool IsSecondPlayer => Runner.LocalPlayer == SecondPlayerRef;

    private bool initialized = false;

    public void FindGameManager() => gameManager = FindAnyObjectByType<GameManagerScript>();

    public override void Spawned()
    {
        if (HasStateAuthority)
        {
            FirstPlayerName = UserSession.Instance.ActiveUser.userData.userName;
            FirstPlayerDeckCsv = SelectedDeckManager.GetSelectedDeckCsv();
            FirstPlayerReady = true;
        }
        else
        {
            string name = UserSession.Instance.ActiveUser.userData.userName;
            RpcSubmitName(name);

            string csv = SelectedDeckManager.GetSelectedDeckCsv();
            RpcSubmitDeck(csv);
        }

        NotificationManager.ShowNotification("Готов", NotificationType.Info);
    }

    public override void Render()
    {
        base.Render();

        if (!initialized && FirstPlayerReady && SecondPlayerReady)
        {
            initialized = true;

            if (HasStateAuthority)
            {
                int index = SceneUtility.GetBuildIndexByScenePath("Assets/Scenes/PlayingScene.unity");
                Runner.LoadScene(SceneRef.FromIndex(index));
                Debug.Log("[NetworkGameState] Both ready → LoadScene('PlayingScene')");
            }
        }
    }

    public void AssignPlayerRole(PlayerRef player)
    {
        if (FirstPlayerRef == default)
            FirstPlayerRef = player;
        else if (SecondPlayerRef == default)
            SecondPlayerRef = player;
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RpcSubmitName(string name, RpcInfo info = default)
    {
        SecondPlayerName = name;
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RpcSubmitDeck(string csv, RpcInfo info = default)
    {
        SecondPlayerDeckCsv = csv;
        SecondPlayerReady = true;
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RpcRequestPlayCard(int cardId, int siblingIndex, RpcInfo info = default)
    {
        RpcPlayCard(cardId, siblingIndex, info.Source);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RpcPlayCard(int playedCardId, int siblingIndex, PlayerRef initiator)
    {
        if (Runner.LocalPlayer != initiator)
            StartCoroutine(OnCardPlayedRoutine(playedCardId, siblingIndex));
    }

    private IEnumerator OnCardPlayedRoutine(int cardId, int siblingIndex)
    {
        Transform enemyHandTransform = gameManager.EnemyHandContainer;
        Transform enemyFieldTransform = gameManager.EnemyFieldContainer;

        foreach (Transform cardTransform in enemyHandTransform)
        {
            if (cardTransform.TryGetComponent<CardMovemantScript>(out var card) &&
                cardTransform.TryGetComponent<CardControllerLink>(out var link) &&
                link.Controller.CardModel.id == cardId)
            {
                yield return card.MoveCardTransformToAnotherField(enemyFieldTransform, siblingIndex);
                cardTransform.GetComponent<CardFlipScript>().FlipToFaceUp();

                gameManager.CurrentGame.EnemyHandListController.CardControllers.Remove(link.Controller);
                int cardIndexInField = Mathf.Clamp(cardTransform.GetSiblingIndex(), 0, gameManager.CurrentGame.EnemyFieldListController.CardControllers.Count);
                gameManager.CurrentGame.EnemyFieldListController.CardControllers.Insert(cardIndexInField, link.Controller);

                link.Controller.MarkAsPlayedInThisTurn(CardOwner.Enemy);

                break;
            }
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RpcRequestAttack(int attackerId, int defenderId, RpcInfo info = default)
    {
        RpcPerformAttack(attackerId, defenderId, info.Source);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RpcPerformAttack(int attackerId, int defenderId, PlayerRef initiator)
    {
        if (Runner.LocalPlayer != initiator)
            StartCoroutine(OnCardAttackedRoutine(attackerId, defenderId));
    }

    private IEnumerator OnCardAttackedRoutine(int attackerId, int defenderId)
    {
        BattleCardController attacker = gameManager.CurrentGame.EnemyFieldListController.CardControllers
            .First(c => c.CardModel.id == attackerId);
        BattleCardController defender = gameManager.CurrentGame.PlayerFieldListController.CardControllers
            .First(c => c.CardModel.id == defenderId);

        Transform cardTransform = attacker.BattleCardView.CardRoot.transform;
        Transform targetTransform = defender.BattleCardView.CardRoot.transform;

        if (cardTransform.TryGetComponent<CardAttackDragHandler>(out var card))
            yield return card.MoveEnemyCardTransformForAttack(targetTransform, attacker, defender);
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
}
