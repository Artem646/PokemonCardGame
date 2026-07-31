using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] private HandManager playerHandManager, enemyHandManager;
    [SerializeField] private FieldManager playerFieldManager, enemyFieldManager;
    [SerializeField] private CardSlot[] playerResetSlots, enemyResetSlots;

    [Header("UI Canvas")]
    [SerializeField] private TextMeshProUGUI turnTimeTxt;
    [SerializeField] private TextMeshProUGUI roundNumberTxt;
    [SerializeField] private TextMeshProUGUI phaseNameTxt;
    [SerializeField] private Button endTurnButton;
    [SerializeField] private GameObject gameOverOverlay;

    [Header("Managers")]
    [SerializeField] private BotController botController;
    [SerializeField] private BotTurnManager botTurnManager;
    [SerializeField] private GameInterfaceController gameInterfaceController;
    [SerializeField] private NetworkRunnerHandler networkRunnerHandler;

    private NetworkGameTurnManager networkGameTurnManager;
    private NetworkGameController networkGameController;

    public Game CurrentGame { get; private set; }
    private GameType currentType = GameTypeConfig.CurrentType;

    private bool isGameSetup = false;
    private bool isGameOver = false;
    public bool isHandsReady = false;

    public HandManager PlayerHandManager => playerHandManager;
    public HandManager EnemyHandManager => enemyHandManager;
    public FieldManager PlayerFieldManager => playerFieldManager;
    public FieldManager EnemyFieldManager => enemyFieldManager;

    public CardSlot[] PlayerResetSlots => playerResetSlots;
    public CardSlot[] EnemyResetSlots => enemyResetSlots;

    const float DELAY_BETWEEN_CARDS = 0.2f;

    public bool IsMyTurn =>
        currentType == GameType.Multiplayer ? networkGameTurnManager.IsMyTurn() :
        currentType == GameType.Bot && botTurnManager.IsPlayerTurn;

    private async void Start()
    {
        if (currentType == GameType.Bot)
        {
            gameInterfaceController.SetStartCameraView();
            InitBotGame();
        }
        else if (currentType == GameType.Multiplayer)
        {
            networkRunnerHandler.UpdateInfoPanel();

            while (FindAnyObjectByType<GameLoadingSceneController>() != null)
                await Task.Yield();

            FindObjects();
            networkGameController.FindObjects();
            networkGameTurnManager.FindObjects();

            gameInterfaceController.SetStartCameraView();

            InitNetworkGame();
        }
    }

    public void FindObjects()
    {
        networkGameController = FindAnyObjectByType<NetworkGameController>();
        networkGameTurnManager = FindAnyObjectByType<NetworkGameTurnManager>();
    }

    public void InitBotGame()
    {
        SetupGame();
        StartCoroutine(InitBotGameRoutine());
    }

    private IEnumerator InitBotGameRoutine()
    {
        yield return DealCardsToHandsRoutine(SelectedDeckManager.GetSelectedDeckIds(), BotDeckData.BotDeckIds);

        isGameSetup = true;

        botTurnManager.OnTurnStarted += OnTurnStarted;
        botTurnManager.OnPhaseChanged += OnPhaseChanged;
        endTurnButton.onClick.AddListener(OnEndTurnButtonClicked);

        botTurnManager.StartFirstTurn();
    }

    public void InitNetworkGame()
    {
        SetupGame();
        StartCoroutine(InitNetworkGameRoutine());
    }

    private IEnumerator InitNetworkGameRoutine()
    {
        if (!ConnectionConfig.IsSpectator)
        {
            bool isFirst = networkGameController.IsFirstPlayer;
            NetworkArray<int> myNetworkHand = isFirst ? networkGameController.FirstPlayerHand : networkGameController.SecondPlayerHand;

            bool myHandExistsInNetwork = false;
            for (int i = 0; i < myNetworkHand.Length; i++)
            {
                if (myNetworkHand[i] != 0)
                {
                    myHandExistsInNetwork = true;
                    break;
                }
            }

            if (!myHandExistsInNetwork)
            {
                List<int> myHandIds = networkGameController.GetPlayerDeckIds();
                List<int> enemyHandIds = networkGameController.GetEnemyDeckIds();

                networkGameController.RpcSyncInitialHand(networkGameController.Runner.LocalPlayer, myHandIds.ToArray());

                yield return new WaitUntil(() => myNetworkHand[0] != 0);

                yield return DealCardsToHandsRoutine(myHandIds, enemyHandIds);
            }
            else
            {
                Debug.Log("Обнаружен реконнект! Восстановление стола.");
                yield return networkGameController.RestoreBoardRoutine();
            }

            networkGameTurnManager.OnPhaseChanged += OnPhaseChanged;
            endTurnButton.onClick.AddListener(OnEndTurnButtonClicked);
        }
        else
        {
            Debug.Log("Восстановление стола.");
            yield return networkGameController.RestoreBoardRoutine();

            roundNumberTxt.text = $"{networkGameTurnManager.RoundNumber}";
            endTurnButton.gameObject.SetActive(false);
            phaseNameTxt.text = "Spectator mode";
        }

        yield return new WaitForSeconds(1.0f);

        isHandsReady = true;
        isGameSetup = true;

        networkGameTurnManager.InitializeVariables();
        networkGameTurnManager.OnTurnStarted += OnTurnStarted;

        if (!ConnectionConfig.IsSpectator) networkGameController.SetPlayerLocalGameReady();
    }

    private void SetupGame()
    {
        CurrentGame = new Game
        {
            PlayerHandControllers = new BattleCardController[playerHandManager.HandSlots.Length],
            PlayerFieldControllers = new BattleCardController[playerFieldManager.FieldSlots.Length],
            EnemyHandControllers = new BattleCardController[enemyHandManager.HandSlots.Length],
            EnemyFieldControllers = new BattleCardController[enemyFieldManager.FieldSlots.Length]
        };
    }

    private void OnTurnStarted(int round)
    {
        roundNumberTxt.text = $"{round}";

        TurnPhase currentPhase = (currentType == GameType.Bot) ? botTurnManager.CurrentPhase : networkGameTurnManager.CurrentPhase;
        gameInterfaceController.RefreshCameraButtons(IsMyTurn, currentPhase);
    }

    private void OnPhaseChanged(TurnPhase phase)
    {
        gameInterfaceController.RefreshCameraButtons(IsMyTurn, phase);

        string key = "";

        if (phase == TurnPhase.PlayCard) key = "PlayCardPhaseKey";
        if (phase == TurnPhase.Attack) key = "AttackPhaseKey";
        if (phase == TurnPhase.Waiting) key = "OpponentTurnKey";

        Localizer.LocalizeGameObjectElement(phaseNameTxt, key, "ElementsText");

        NotificationManager.ShowNotification(phaseNameTxt.text, NotificationType.Info, 1f);
    }

    private void UpdateTurnUI()
    {
        if (ConnectionConfig.IsSpectator) return;

        bool canEndTurn = IsMyTurn &&
        (
            (currentType == GameType.Bot && botTurnManager.CurrentPhase == TurnPhase.Attack) ||
            (currentType == GameType.Multiplayer && networkGameTurnManager.CurrentPhase == TurnPhase.Attack)
        );

        endTurnButton.interactable = canEndTurn;
        turnTimeTxt.color = IsMyTurn ? Color.green : Color.red;

        TurnPhase currentPhase = (currentType == GameType.Bot) ? botTurnManager.CurrentPhase : networkGameTurnManager.CurrentPhase;
        gameInterfaceController.RefreshCameraButtons(IsMyTurn, currentPhase);
    }

    private void Update()
    {
        if (currentType == GameType.Bot) UpdateBotTimer();
        else if (currentType == GameType.Multiplayer) UpdateNetworkTimer();

        if (isGameSetup && !isGameOver)
            CheckGameEnd();
    }

    private void UpdateBotTimer()
    {
        float remaining = botTurnManager.TurnEndTime - Time.time;
        int seconds = Mathf.Max(0, Mathf.CeilToInt(remaining));
        System.TimeSpan timeSpan = System.TimeSpan.FromSeconds(seconds);
        turnTimeTxt.text = timeSpan.ToString(@"mm\:ss");

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
            networkGameController == null || networkGameController.Runner == null ||
            !networkGameController.Runner.IsRunning)
            return;

        float remaining = networkGameTurnManager.TurnEndTime - networkGameController.Runner.SimulationTime;
        int seconds = Mathf.Max(0, Mathf.CeilToInt(remaining));
        System.TimeSpan timeSpan = System.TimeSpan.FromSeconds(seconds);
        turnTimeTxt.text = timeSpan.ToString(@"mm\:ss");

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
        CardSlot filledHandSlot = playerHandManager.GetRandomFilledSlot();
        Transform cardTransform = filledHandSlot.CurrentCard.transform;

        if (cardTransform.TryGetComponent<CardMovemantScript>(out var card) &&
            cardTransform.TryGetComponent<CardControllerLink>(out var link))
        {
            CardSlot emptyFieldSlot = playerFieldManager.GetRandomEmptySlot();
            yield return card.MoveCardTransformToField(emptyFieldSlot);
            filledHandSlot.Clear();

            card.MoveCardOnField(link.Controller, (int)filledHandSlot.indexSlotInField, (int)emptyFieldSlot.indexSlotInField, true);
        }

        if (currentType == GameType.Bot)
            botTurnManager.ResetTurnFlagsForAllCards();
        else if (currentType == GameType.Multiplayer)
            networkGameTurnManager.ResetTurnFlagsForAllCards();
    }

    private IEnumerator DealCardsToHandsRoutine(List<int> playerCardsIds, List<int> enemyCardsIds)
    {
        for (int i = 0; i < playerCardsIds.Count; i++)
        {
            BattleCardController battleCardController = CardRepository.Instance.GetBattleCardControllerById(playerCardsIds[i]);
            CurrentGame.PlayerHandControllers[i] = battleCardController;
            playerHandManager.TryAddCardInStart(battleCardController);
            yield return new WaitForSeconds(DELAY_BETWEEN_CARDS);
        }

        for (int i = 0; i < enemyCardsIds.Count; i++)
        {
            BattleCardController battleCardController = CardRepository.Instance.GetBattleCardControllerById(enemyCardsIds[i]);
            CurrentGame.EnemyHandControllers[i] = battleCardController;
            enemyHandManager.TryAddCardInStart(battleCardController);
            yield return new WaitForSeconds(DELAY_BETWEEN_CARDS);
        }
    }

    public void CheckGameEnd()
    {
        if (ConnectionConfig.IsSpectator) return;

        if (!isGameSetup || isGameOver) return;
        if (currentType == GameType.Multiplayer) if (!isHandsReady) return;

        if (Time.timeSinceLevelLoad < 5f) return;

        if (CurrentGame.PlayerHandControllers != null || CurrentGame.PlayerFieldControllers != null ||
            CurrentGame.EnemyHandControllers != null || CurrentGame.EnemyFieldControllers != null)
        {
            bool isPlayerEmpty = CurrentGame.PlayerHandControllers.IsEmpty() && CurrentGame.PlayerFieldControllers.IsEmpty();
            bool isEnemyEmpty = CurrentGame.EnemyHandControllers.IsEmpty() && CurrentGame.EnemyFieldControllers.IsEmpty();

            if (isPlayerEmpty || isEnemyEmpty)
            {
                isGameOver = true;
                StartCoroutine(LoadSceneAfterEndGame());
            }
        }
    }

    private IEnumerator LoadSceneAfterEndGame()
    {
        gameInterfaceController.CloseZoomOnCardView();
        yield return new WaitForSeconds(1.5f);

        gameOverOverlay.SetActive(true);
        yield return new WaitForSeconds(2f);

        if (currentType == GameType.Multiplayer)
        {
            Task disconnectTask = NetworkRunnerController.Instance.DisconnectAndRejoinLobby();
            yield return new WaitUntil(() => disconnectTask.IsCompleted);
        }

        SceneManager.LoadScene("CollectionScene");
    }

    private void OnDestroy()
    {
        if (botTurnManager != null)
        {
            botTurnManager.OnTurnStarted -= OnTurnStarted;
            botTurnManager.OnPhaseChanged -= OnPhaseChanged;
        }

        if (networkGameTurnManager != null)
        {
            networkGameTurnManager.OnTurnStarted -= OnTurnStarted;
            networkGameTurnManager.OnPhaseChanged -= OnPhaseChanged;
        }

        endTurnButton.onClick.RemoveListener(OnEndTurnButtonClicked);
    }

    // public T[] Combine<T>(params T[][] arrays)
    // {
    //     int total = 0;
    //     foreach (var arr in arrays)
    //         total += arr.Length;

    //     var result = new T[total];

    //     int offset = 0;
    //     foreach (var arr in arrays)
    //     {
    //         Array.Copy(arr, 0, result, offset, arr.Length);
    //         offset += arr.Length;
    //     }

    //     return result;
    // }

    // [SerializeField] private Transform test3DContainer;
    // [SerializeField] private int cardsPerRow = 10; // Сколько карт в одном ряду
    // [SerializeField] private float spacingX = 6f;  // Расстояние между картами по горизонтали
    // [SerializeField] private float spacingZ = 4f;  // Расстояние между рядами (в глубину)
    // [SerializeField] private GameObject cardPrefab3D;

    // [ContextMenu("DEBUG: Спавн ВСЕХ 3D КАРТ")]
    // public void SpawnAll3DCardsForTest()
    // {
    //     CardControllerFactory.Init(prefab3D: cardPrefab3D);

    //     List<CardModel> allGameCards = CardRepository.Instance.GetGameCardsList().cards;
    //     Debug.Log($"[DEBUG] Спавн {allGameCards.Count} 3D карт...");

    //     int currentColumn = 0;
    //     int currentRow = 0;

    //     foreach (CardModel model in allGameCards)
    //     {
    //         BattleCard3DController cardController = CardControllerFactory.Create<BattleCard3DController>(model, parent: test3DContainer);
    //         cardController.AddToContainer(test3DContainer);
    //         Transform cardTransform = cardController.BattleCardView.CardRoot.transform;
    //         cardTransform.name = $"TEST_3D_{model.id}_{model.titleKey}";

    //         float posX = currentColumn * spacingX;
    //         float posZ = currentRow * spacingZ;

    //         cardTransform.SetLocalPositionAndRotation(new Vector3(posX, 0, posZ), Quaternion.Euler(-90, 90, 0));

    //         currentColumn++;
    //         if (currentColumn >= cardsPerRow)
    //         {
    //             currentColumn = 0;
    //             currentRow++;
    //         }
    //     }

    //     Debug.Log("[DEBUG] 3D Стенд успешно построен!");
    // }

    // [Header("Photo Studio Settings")]
    // [SerializeField] private int photoResolution = 1024;
    // [SerializeField] private Vector3 cameraOffset = new(0, 0, -3f); // 2.2-y для скрина верхней части
    // [SerializeField] private float orthographicSize = 3.5f;

    // [ContextMenu("DEBUG: Сделать скриншоты всех 3D карт")]
    // public void TakePhotosOfAllCards()
    // {
    //     if (test3DContainer == null || test3DContainer.childCount == 0)
    //     {
    //         Debug.LogError("Сначала отспавните карты через 'DEBUG: Спавн ВСЕХ 3D КАРТ'!");
    //         return;
    //     }

    //     StartCoroutine(PhotoStudioRoutine());
    // }

    // private IEnumerator PhotoStudioRoutine()
    // {
    //     Debug.Log("[PhotoStudio] Подготовка камеры...");

    //     string folderPath = Path.Combine(Application.dataPath, "../CardScreenshots");
    //     if (!Directory.Exists(folderPath))
    //         Directory.CreateDirectory(folderPath);

    //     GameObject camObj = new("PhotoCamera");
    //     Camera photoCam = camObj.AddComponent<Camera>();

    //     photoCam.clearFlags = CameraClearFlags.SolidColor;
    //     photoCam.backgroundColor = new Color(0, 0, 0, 0);
    //     photoCam.orthographic = true;
    //     photoCam.orthographicSize = orthographicSize; // Размер захвата камеры (ПОДСТРОЙТЕ, если карта не влезает)

    //     RenderTexture renderTexture = new(photoResolution, photoResolution, 24);
    //     photoCam.targetTexture = renderTexture;

    //     Texture2D screenShot = new(photoResolution, photoResolution, TextureFormat.ARGB32, false);

    //     int count = 0;

    //     foreach (Transform cardTransform in test3DContainer)
    //     {
    //         photoCam.transform.position = cardTransform.position + cameraOffset;
    //         photoCam.transform.LookAt(cardTransform.position);

    //         yield return new WaitForEndOfFrame();

    //         RenderTexture.active = renderTexture;
    //         photoCam.Render();
    //         screenShot.ReadPixels(new Rect(0, 0, photoResolution, photoResolution), 0, 0);
    //         screenShot.Apply();

    //         byte[] bytes = screenShot.EncodeToPNG();
    //         string fileName = $"{cardTransform.name}.png";
    //         string filePath = Path.Combine(folderPath, fileName);
    //         File.WriteAllBytes(filePath, bytes);

    //         count++;
    //         Debug.Log($"[PhotoStudio] Снято: {fileName}");
    //     }

    //     photoCam.targetTexture = null;
    //     RenderTexture.active = null;
    //     Destroy(renderTexture);
    //     Destroy(camObj);
    //     Destroy(screenShot);

    //     Debug.Log($"[PhotoStudio] ГОТОВО! {count} карт сохранено в папку: {folderPath}");
    // }
}