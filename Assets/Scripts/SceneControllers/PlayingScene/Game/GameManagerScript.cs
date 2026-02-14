using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManagerScript : MonoBehaviour
{
    [SerializeField] private Transform enemyHandContainer, playerHandContainer;
    [SerializeField] private Transform enemyFieldContainer, playerFieldContainer;
    [SerializeField] private Transform enemyResetStack, playerResetStack;

    [Header("UI")]
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private TextMeshProUGUI turnTimeTxt;
    [SerializeField] private TextMeshProUGUI roundNumberTxt;
    [SerializeField] private TextMeshProUGUI phaseNameTxt;
    [SerializeField] private Button endTurnButton;
    [SerializeField] private GameObject cardOverlay;
    [SerializeField] private GameObject gameOverOverlay;

    [Header("Managers")]
    [SerializeField] private BotTurnManager botTurnManager;
    [SerializeField] private BotController botController;

    private NetworkGameTurnManager networkGameTurnManager;
    private NetworkGameController networkGameController;

    public Game CurrentGame { get; private set; }
    private GameType currentType = GameTypeConfig.CurrentType;
    public bool isGameSetup;

    public bool IsMyTurn =>
        currentType == GameType.Multiplayer ? networkGameTurnManager.IsMyTurn() :
        currentType == GameType.Bot && botTurnManager.IsPlayerTurn;

    public Transform PlayerHandContainer => playerHandContainer;
    public Transform PlayerFieldContainer => playerFieldContainer;
    public Transform PlayerResetStack => playerResetStack;
    public Transform EnemyHandContainer => enemyHandContainer;
    public Transform EnemyFieldContainer => enemyFieldContainer;
    public Transform EnemyResetStack => enemyResetStack;

    public void FindObjects()
    {
        networkGameController = FindAnyObjectByType<NetworkGameController>();
        networkGameTurnManager = FindAnyObjectByType<NetworkGameTurnManager>();
    }

    public void InitBotGame()
    {
        botController.GenerateDeck();

        SetupGame();

        LoadCardsToPlayerHand(SelectedDeckManager.GetSelectedDeckIds());
        LoadCardsToEnemyHand(botController.BotDeckIds);

        botTurnManager.OnTurnStarted += OnTurnStarted;
        botTurnManager.OnPhaseChanged += OnPhaseChanged;

        endTurnButton.onClick.AddListener(OnEndTurnButtonClicked);

        botTurnManager.StartFirstTurn();
    }

    public void InitNetworkGame()
    {
        SetupGame();

        LoadCardsToPlayerHand(networkGameController.GetPlayerDeckIds());
        LoadCardsToEnemyHand(networkGameController.GetEnemyDeckIds());

        networkGameTurnManager.OnTurnStarted += OnTurnStarted;
        networkGameTurnManager.OnPhaseChanged += OnPhaseChanged;

        endTurnButton.onClick.AddListener(OnEndTurnButtonClicked);

        networkGameTurnManager.StartFirstTurn();
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

        isGameSetup = true;
    }

    private void OnTurnStarted(int round)
    {
        roundNumberTxt.text = $"Раунд {round}";
    }

    private void OnPhaseChanged(TurnPhase phase)
    {
        if (phase == TurnPhase.Waiting)
            phaseNameTxt.text = "Ход противника";
        else if (phase == TurnPhase.PlayCard)
            phaseNameTxt.text = "Фаза выкладывания карты";
        else if (phase == TurnPhase.Attack)
            phaseNameTxt.text = "Фаза атаки";
    }

    private void UpdateTurnUI()
    {
        bool canEndTurn = IsMyTurn &&
        (
            (currentType == GameType.Bot && botTurnManager.CurrentPhase == TurnPhase.Attack) ||
            (currentType == GameType.Multiplayer && networkGameTurnManager.CurrentPhase == TurnPhase.Attack)
        );

        endTurnButton.interactable = canEndTurn;
        turnTimeTxt.color = IsMyTurn ? Color.green : Color.red;
    }

    private void Update()
    {
        if (currentType == GameType.Bot) UpdateBotTimer();
        else if (currentType == GameType.Multiplayer) UpdateNetworkTimer();

        if (isGameSetup)
        {
            HealthPanelController[] panels = FindObjectsByType<HealthPanelController>(FindObjectsSortMode.None);
            foreach (HealthPanelController panel in panels)
                panel.UpdateSlots();

            CheckGameEnd();
        }
    }

    private void UpdateBotTimer()
    {
        float remaining = botTurnManager.TurnEndTime - Time.time;
        int seconds = Mathf.Max(0, Mathf.CeilToInt(remaining));
        turnTimeTxt.text = seconds.ToString();

        UpdateTurnUI();

        if (remaining <= 0 && IsMyTurn)
        {
            if (botTurnManager.CurrentPhase == TurnPhase.PlayCard)
                StartCoroutine(AutoMoveCard());
            botTurnManager.EndTurn();
        }
    }

    private void UpdateNetworkTimer()
    {
        if (networkGameTurnManager == null || !networkGameTurnManager.TurnManagerInitialized ||
            networkGameController == null || networkGameController.Runner == null)
            return;

        float remaining = networkGameTurnManager.TurnEndTime - networkGameController.Runner.SimulationTime;
        int seconds = Mathf.Max(0, Mathf.CeilToInt(remaining));
        turnTimeTxt.text = seconds.ToString();

        UpdateTurnUI();

        if (remaining <= 0 && IsMyTurn)
        {
            if (networkGameTurnManager.CurrentPhase == TurnPhase.PlayCard)
                StartCoroutine(AutoMoveCard());
            networkGameTurnManager.RequestEndTurn();
        }
    }

    private void OnEndTurnButtonClicked()
    {
        if (currentType == GameType.Bot)
            botTurnManager.EndTurn();
        else if (currentType == GameType.Multiplayer)
            networkGameTurnManager.RequestEndTurn();
    }

    private IEnumerator AutoMoveCard()
    {
        int cardIndexInHand = Random.Range(0, playerHandContainer.childCount - 1);
        Transform cardTransform = playerHandContainer.GetChild(cardIndexInHand);

        if (cardTransform.TryGetComponent<CardMovemantScript>(out var card) &&
            cardTransform.TryGetComponent<CardControllerLink>(out var link))
        {
            int siblingIndexInField = Random.Range(0, playerFieldContainer.childCount + 1);
            yield return card.MoveCardTransformToAnotherField(playerFieldContainer, siblingIndexInField);
            card.MoveCardOnField(link.Controller, cardTransform.GetSiblingIndex(), true);
        }

        if (currentType == GameType.Bot)
            botTurnManager.ResetTurnFlagsForAllCards();
        else if (currentType == GameType.Multiplayer)
            networkGameTurnManager.ResetTurnFlagsForAllCards();
    }

    public async void LoadCardsToPlayerHand(List<int> playerIds)
    {
        await CurrentGame.PlayerHandListController.LoadCardsByIds(playerIds);
    }

    public async void LoadCardsToEnemyHand(List<int> enemyIds)
    {
        await CurrentGame.EnemyHandListController.LoadCardsByIds(enemyIds);
    }

    public void CheckGameEnd()
    {
        int playerHandCount = CurrentGame.PlayerHandListController.CardControllers.Count;
        int playerFieldCount = CurrentGame.PlayerFieldListController.CardControllers.Count;
        int enemyHandCount = CurrentGame.EnemyHandListController.CardControllers.Count;
        int enemyFieldCount = CurrentGame.EnemyFieldListController.CardControllers.Count;

        if ((playerHandCount == 0 && playerFieldCount == 0) || (enemyHandCount == 0 && enemyFieldCount == 0))
        {
            StartCoroutine(LoadSceneAfterEndGame());
        }
    }

    private IEnumerator LoadSceneAfterEndGame()
    {
        BattleCardScaleAnimator.HideCard(cardOverlay);
        yield return new WaitForSeconds(1.5f);

        gameOverOverlay.SetActive(true);
        yield return new WaitForSeconds(2f);

        if (currentType == GameType.Multiplayer)
        {
            NetworkRunnerHandler networkRunnerHandler = FindAnyObjectByType<NetworkRunnerHandler>();
            networkRunnerHandler.ShutdownRunner();
        }

        SceneManager.LoadScene("CollectionScene");
    }
}