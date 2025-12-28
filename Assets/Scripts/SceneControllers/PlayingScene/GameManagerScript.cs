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
    [SerializeField] private Transform enemyResetStack, playerResetStack;
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private TextMeshProUGUI turnTimeTxt;
    [SerializeField] private TextMeshProUGUI turnNumberTxt;
    [SerializeField] private Button endTurnButton;
    [SerializeField] private GameObject gameOverOverlay;
    public TypeChart typeChart;

    public Game CurrentGame { get; private set; }
    private NetworkGameState networkGameState = null;
    private BotGameState botGameState = null;
    private float botTurnDuration = 30f;
    private float botTurnEndTime = 0f;

    public bool IsMyTurn
    {
        get
        {
            if (networkGameState != null)
            {
                return (networkGameState.IsFirstPlayer && networkGameState.IsFirstPlayerTurn)
                    || (networkGameState.IsSecondPlayer && networkGameState.IsSecondPlayerTurn);
            }
            else if (botGameState != null)
            {
                return botGameState.IsPlayerTurn;
            }
            return false;
        }
    }

    public bool cardIsThrown = false;
    private bool turnEnded = false;

    public void Init(NetworkGameState state)
    {
        if (state == null) return;
        networkGameState = state;
        SetupGame();

        LoadCardsToPlayerHand(networkGameState.GetPlayerDeckIds());
        LoadCardsToEnemyHand(networkGameState.GetEnemyDeckIds());

        turnNumberTxt.text = $"Раунд {networkGameState.RoundNumber}";

        UpdateTurnUI();

        endTurnButton.onClick.AddListener(OnEndTurnButtonClicked);
    }

    public void Init(BotGameState state)
    {
        if (state == null) return;

        botGameState = state;
        SetupGame();

        LoadCardsToPlayerHand(SelectedDeckManager.GetSelectedDeckIds());
        LoadCardsToEnemyHand(botGameState.BotDeckIds);

        turnNumberTxt.text = $"Раунд {botGameState.RoundNumber}";
        botTurnEndTime = Time.time + botTurnDuration;

        UpdateTurnUI();

        endTurnButton.onClick.AddListener(OnEndTurnButtonClicked);
    }

    private void SetupGame()
    {
        CurrentGame = new Game();
        CardControllerFactory.Init(prefab: cardPrefab);

        CurrentGame.PlayerHandListController = new(playerHandContainer);
        CurrentGame.EnemyHandListController = new(enemyHandContainer);

        CurrentGame.PlayerFieldListController = new(playerFieldContainer);
        CurrentGame.EnemyFieldListController = new(enemyFieldContainer);

        CurrentGame.PlayerDeck = CurrentGame.PlayerHandListController.CardModels;
        CurrentGame.EnemyDeck = CurrentGame.EnemyHandListController.CardModels;
    }

    private void OnEndTurnButtonClicked()
    {
        if (!cardIsThrown) AutoPlayFirstCard();
        cardIsThrown = false;

        // foreach (var card in CurrentGame.PlayerFieldListController.CardControllers)
        // {
        //     card.ChangeAttackState(false);
        //     card.DeHighlightCard();
        // }

        if (IsMyTurn)
        {
            if (networkGameState != null)
            {
                if (networkGameState.HasStateAuthority)
                    networkGameState.SwitchRoundPhase();
                else
                    networkGameState.RpcRequestEndRoundPhase();
            }
            else
            {
                botGameState.EndPlayerTurn();
                turnEnded = false;
                botTurnEndTime = Time.time + botTurnDuration;
            }
        }
    }

    public void OnTurnChanged(int turnNumber)
    {
        turnEnded = false;
        turnNumberTxt.text = $"Раунд {turnNumber}";
        UpdateTurnUI();

        if (botGameState != null)
            botTurnEndTime = Time.time + botTurnDuration;
    }

    public void MoveCardToReset(BattleCardController cardController, bool isFirstPlayerCard)
    {
        if (cardController == null) return;

        Transform resetStack = isFirstPlayerCard ? playerResetStack : enemyResetStack;

        cardController.BattleCardView.CardRoot.transform.SetParent(resetStack, false);
        cardController.BattleCardView.CardRoot.transform.SetSiblingIndex(resetStack.childCount - 1);

        if (isFirstPlayerCard)
            CurrentGame.PlayerFieldListController.CardControllers.Remove(cardController);
        else
            CurrentGame.EnemyFieldListController.CardControllers.Remove(cardController);
    }

    private void UpdateTurnUI()
    {
        endTurnButton.interactable = IsMyTurn;
        turnTimeTxt.color = IsMyTurn ? Color.green : Color.red;
    }

    private void Update()
    {
        if (networkGameState != null && networkGameState.Runner != null)
        {
            float remaining = networkGameState.RoundPhaseEndTime - networkGameState.Runner.SimulationTime;
            int seconds = Mathf.Max(0, Mathf.CeilToInt(remaining));
            turnTimeTxt.text = seconds.ToString();
            UpdateTurnUI();

            if (remaining <= 0 && IsMyTurn && !turnEnded)
            {
                OnEndTurnButtonClicked();
                turnEnded = true;
            }
        }
        else if (botGameState != null)
        {
            float remaining = botTurnEndTime - Time.time;
            int seconds = Mathf.Max(0, Mathf.CeilToInt(remaining));
            turnTimeTxt.text = seconds.ToString();
            UpdateTurnUI();

            if (remaining <= 0 && IsMyTurn && !turnEnded)
            {
                OnEndTurnButtonClicked();
                turnEnded = true;
            }
        }
    }

    private void AutoPlayFirstCard()
    {
        BattleCardController firstCard = CurrentGame.PlayerHandListController.CardControllers.FirstOrDefault();
        if (firstCard != null)
        {
            int siblingIndex = 0;

            CurrentGame.PlayerHandListController.CardControllers.Remove(firstCard);
            CurrentGame.PlayerFieldListController.CardControllers.Add(firstCard);

            firstCard.BattleCardView.CardRoot.transform.SetParent(playerFieldContainer, false);
            firstCard.BattleCardView.CardRoot.transform.SetSiblingIndex(siblingIndex);

            if (networkGameState != null)
                RequestPlayCard(firstCard.CardModel.id, siblingIndex);
            else if (botGameState != null)
                OnCardPlayed(firstCard.CardModel.id, siblingIndex, true);

            cardIsThrown = true;
        }
    }

    public async void LoadCardsToPlayerHand(List<int> playerIds)
    {
        await CurrentGame.PlayerHandListController.LoadCardsByIds(playerIds);
    }

    public async void LoadCardsToEnemyHand(List<int> enemyIds)
    {
        await CurrentGame.EnemyHandListController.LoadCardsByIds(enemyIds);
    }

    public async void OnCardPlayed(int cardId, int siblingIndex, bool isFirstPlayer)
    {
        if (networkGameState != null)
        {
            if (networkGameState.IsFirstPlayer != isFirstPlayer)
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
                CurrentGame.EnemyFieldListController.CardControllers.Add(newController.CardControllers[0]);
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
        if (networkGameState != null)
            networkGameState.RpcRequestPlayCard(cardId, siblingIndex);
    }

    public void ShowGameOverOverlay()
    {
        StartCoroutine(ShowGameOverOverlayForSeconds(3f));
    }

    private IEnumerator ShowGameOverOverlayForSeconds(float seconds)
    {
        gameOverOverlay.SetActive(true);
        yield return new WaitForSeconds(seconds);
        gameOverOverlay.SetActive(false);
    }

    public Transform EnemyFieldContainer => enemyFieldContainer;
    public Transform PlayerFieldContainer => playerFieldContainer;
}