using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Game
{
    public List<CardModel> PlayerDeck { get; set; }
    public List<CardModel> EnemyDeck { get; set; }

    public BattleCardListController PlayerHandListController { get; set; }
    public BattleCardListController EnemyHandListController { get; set; }
    public BattleCardListController PlayerFieldListController { get; set; }
    public BattleCardListController EnemyFieldListController { get; set; }
}

public class GameManagerScript : MonoBehaviour
{
    [SerializeField] private Transform enemyHandContainer, playerHandContainer;
    [SerializeField] private Transform enemyFieldContainer, playerFieldContainer;
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private TextMeshProUGUI turnTimeTxt;
    [SerializeField] private TextMeshProUGUI turnNumberTxt;
    [SerializeField] private Button endTurnButton;
    public TypeChart typeChart;

    public Game CurrentGame { get; private set; }
    private NetworkGameState networkGameState;
    public bool IsMyTurn =>
       (networkGameState.IsFirstPlayer && networkGameState.IsFirstPlayerTurn) ||
       (networkGameState.IsSecondPlayer && networkGameState.IsSecondPlayerTurn);

    public bool cardIsThrown = false;

    public void Init(NetworkGameState state)
    {
        networkGameState = state;
        CurrentGame = new Game();
        CardControllerFactory.Init(prefab: cardPrefab);

        CurrentGame.PlayerHandListController = new(playerHandContainer);
        CurrentGame.EnemyHandListController = new(enemyHandContainer);

        CurrentGame.PlayerFieldListController = new(playerFieldContainer);
        CurrentGame.EnemyFieldListController = new(enemyFieldContainer);

        CurrentGame.PlayerDeck = CurrentGame.PlayerHandListController.CardModels;
        CurrentGame.EnemyDeck = CurrentGame.EnemyHandListController.CardModels;

        if (networkGameState != null)
        {
            if (networkGameState.Runner.IsServer)
            {
                LoadCardsToPlayerHand(networkGameState.GetPlayerDeckIds());
                LoadCardsToEnemyHand(networkGameState.GetEnemyDeckIds());
            }
            else
            {
                LoadCardsToPlayerHand(networkGameState.GetEnemyDeckIds());
                LoadCardsToEnemyHand(networkGameState.GetPlayerDeckIds());
            }
        }

        // foreach (var card in CurrentGame.PlayerHandListController.CardControllers)
        // {
        //     card.CardView.CardRootGameObject.GetComponent<AttackedCardScript>().enabled = false;
        // }

        if (networkGameState.IsFirstPlayer)
        {
            networkGameState.IsFirstPlayerTurn = true;
            networkGameState.IsSecondPlayerTurn = false;
        }

        UpdateTurnUI();
        StartCoroutine(TurnFunc());

        endTurnButton.onClick.AddListener(OnEndTurnButtonClicked);
    }

    private void OnEndTurnButtonClicked()
    {
        cardIsThrown = false;

        // foreach (var card in CurrentGame.PlayerFieldListController.CardControllers)
        // {
        //     card.ChangeAttackState(false);
        //     card.DeHighlightCard();
        // }

        if (IsMyTurn)
        {
            if (networkGameState.IsFirstPlayer)
            {
                networkGameState.SwitchTurn();
            }
            else if (networkGameState.IsSecondPlayer)
            {
                networkGameState.RpcRequestEndTurn();
            }
        }
    }

    public void OnTurnChanged(int turnNumber)
    {
        StopAllCoroutines();
        UpdateTurnUI();
        StartCoroutine(TurnFunc());

        turnNumberTxt.text = $"Тур {turnNumber}";

        // EnableAttackForCurrentPlayer();
    }

    // private void EnableAttackForCurrentPlayer()
    // {
    //     List<BattleCardController> fieldCards = IsMyTurn
    //         ? CurrentGame.PlayerFieldListController.CardControllers
    //         : CurrentGame.EnemyFieldListController.CardControllers;

    //     if (IsMyTurn)
    //     {
    //         foreach (var card in fieldCards)
    //         {
    //             card.ChangeAttackState(true);
    //             card.HighlightCard();
    //         }
    //     }
    // }

    private void UpdateTurnUI()
    {
        endTurnButton.interactable = IsMyTurn;
    }

    IEnumerator TurnFunc()
    {
        float duration = 30f;
        float endTime = Time.time + duration;

        while (Time.time < endTime)
        {
            int remaining = Mathf.CeilToInt(endTime - Time.time);
            turnTimeTxt.text = remaining.ToString();
            yield return null;
        }

        // foreach (var card in CurrentGame.PlayerFieldListController.CardControllers)
        // {
        //     card.ChangeAttackState(false);
        //     card.DeHighlightCard();
        // }

        cardIsThrown = false;

        if (networkGameState.IsFirstPlayer)
            networkGameState.SwitchTurn();
    }

    public async void LoadCardsToPlayerHand(List<int> playerIds)
    {
        await CurrentGame.PlayerHandListController.LoadCardsByIds(playerIds);
    }

    public async void LoadCardsToEnemyHand(List<int> enemyIds)
    {
        await CurrentGame.EnemyHandListController.LoadCardsByIds(enemyIds);
    }

    // public async void OnCardPlayed(int cardId, int siblingIndex, bool isFirstPlayer, bool toField)
    // {
    //     bool firstPlayer = networkGameState.Runner.IsServer;
    //     if (firstPlayer != isFirstPlayer)
    //     {
    //         Transform containerTo, containerFrom;
    //         List<BattleCardController> listFrom;

    //         if (toField)
    //         {
    //             containerTo = enemyFieldContainer;
    //             containerFrom = enemyHandContainer;
    //             listFrom = CurrentGame.EnemyHandListController.CardControllers;
    //         }
    //         else
    //         {
    //             containerTo = enemyHandContainer;
    //             containerFrom = enemyFieldContainer;
    //             listFrom = CurrentGame.EnemyFieldListController.CardControllers;
    //         }

    //         RemoveCardFromContainer(cardId, containerFrom, listFrom);

    //         BattleCardListController newController = new(containerTo);
    //         await newController.LoadCardsByIds(new List<int> { cardId });

    //         Transform newCard = containerTo.GetChild(containerTo.childCount - 1);
    //         newCard.SetSiblingIndex(siblingIndex);

    //         if (toField)
    //         {
    //             newCard.GetComponent<CardFlipScript>().FlipToFaceUp();
    //             BattleCardController lastController = newController.CardControllers[0];
    //             // lastController.ChangeAttackState(false);
    //             networkGameState.CheckForBattle();
    //             CurrentGame.EnemyFieldListController.CardControllers.Add(lastController);
    //         }
    //         else
    //         {
    //             newCard.GetComponent<CardFlipScript>().FlipToFaceDown();
    //             CurrentGame.EnemyHandListController.CardControllers.Add(newController.CardControllers[0]);
    //         }
    //     }
    // }

    public async void OnCardPlayed(int cardId, int siblingIndex, bool isFirstPlayer)
    {
        bool firstPlayer = networkGameState.Runner.IsServer;
        if (firstPlayer != isFirstPlayer)
        {
            Transform containerTo = enemyFieldContainer;
            Transform containerFrom = enemyHandContainer;
            List<BattleCardController> listFrom = CurrentGame.EnemyHandListController.CardControllers;
            RemoveCardFromContainer(cardId, containerFrom, listFrom);

            BattleCardListController newController = new(containerTo);
            await newController.LoadCardsByIds(new List<int> { cardId });

            Transform newCard = containerTo.GetChild(containerTo.childCount - 1);
            newCard.SetSiblingIndex(siblingIndex);

            newCard.GetComponent<CardFlipScript>().FlipToFaceUp();
            // networkGameState.CheckForBattle();
            CurrentGame.EnemyFieldListController.CardControllers.Add(newController.CardControllers[0]);

            var playerCard = CurrentGame.PlayerFieldListController.CardControllers.LastOrDefault();
            var enemyCard = CurrentGame.EnemyFieldListController.CardControllers.LastOrDefault();

            if (playerCard != null && enemyCard != null)
            {
                Debug.Log("Боооой");
                RequestCardsFight(playerCard.CardModel.id, enemyCard.CardModel.id);
            }
        }
    }

    private void RemoveCardFromContainer(int cardId, Transform container, List<BattleCardController> list)
    {
        foreach (Transform child in container)
        {
            CardMovemantScript cardScript = child.GetComponent<CardMovemantScript>();
            if (cardScript != null && cardScript.CardId == cardId)
            {
                Destroy(child.gameObject);
                break;
            }
        }

        RemoveCardFromList(cardId, list);
    }

    public void RemoveCardFromList(int cardId, List<BattleCardController> list)
    {
        BattleCardController cardRemove = list.FirstOrDefault(c => c.CardModel.id == cardId);
        if (cardRemove != null)
            list.Remove(cardRemove);
    }

    public void RequestPlayCard(int cardId, int siblingIndex)
    {
        networkGameState.RpcRequestPlayCard(cardId, siblingIndex);
    }




    public void RequestCardsFight(int attackerId, int defenderId)
    {
        if (IsMyTurn)
        {
            networkGameState.RpcRequestCardsFight(attackerId, defenderId);
        }
    }

    public void DestroyCardById(int cardId)
    {
        BattleCardController card = CurrentGame.PlayerFieldListController.CardControllers.FirstOrDefault(c => c.CardModel.id == cardId);
        card ??= CurrentGame.EnemyFieldListController.CardControllers.FirstOrDefault(c => c.CardModel.id == cardId);

        if (card != null)
        {
            Destroy(card.BattleCardView.CardRoot);
            CurrentGame.PlayerFieldListController.CardControllers.Remove(card);
            CurrentGame.EnemyFieldListController.CardControllers.Remove(card);
        }
    }

    // public void CardsFight(BattleCardController playerCard, BattleCardController enemyCard)
    // {
    //     DestoyCard(enemyCard);
    // }

    // private void DestoyCard(BattleCardController card)
    // {
    //     card.BattleCardView.CardRoot.GetComponent<CardMovemantScript>().OnEndDrag(null);

    //     if (CurrentGame.EnemyFieldListController.CardControllers.Exists(c => c.CardModel.id == card.CardModel.id))
    //         CurrentGame.EnemyFieldListController.CardControllers.Remove(card);

    //     if (CurrentGame.PlayerFieldListController.CardControllers.Exists(c => c.CardModel.id == card.CardModel.id))
    //         CurrentGame.PlayerFieldListController.CardControllers.Remove(card);

    //     Destroy(card.BattleCardView.CardRoot);
    // }
}