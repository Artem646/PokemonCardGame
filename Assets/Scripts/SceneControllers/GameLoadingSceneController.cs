using DG.Tweening;
using Fusion;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class GameLoadingSceneController : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private GameObject cardPrefab3D;

    [SerializeField] private NetworkPrefabRef networkGameControllerPrefab;
    [SerializeField] private NetworkPrefabRef networkGameTurnManagerPrefab;

    private VisualElement root;
    private VisualElement firstOpponentImage;
    private Label firstOpponentName;
    private VisualElement secondOpponentImage;
    private Label secondOpponentName;
    private Label progressLabel;
    private ProgressBar progressBar;
    private string baseUploadingText;
    private Tween progressTween;
    private Sequence dotsSequence;

    private NetworkGameController networkGameController;

    private void Awake()
    {
        if (ConnectionConfig.IsLateSpectator)
        {
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }
    }

    private async void Start()
    {
        InitializeUI();
        StartDotsAnimation();

        CardRepository.Instance.OnProgressChanged += HandleProgress;

        if (GameTypeConfig.CurrentType == GameType.Bot)
        {
            FillOpponentsInfoInBotGame();
            await SetupBotGame();
        }
        else if (GameTypeConfig.CurrentType == GameType.Multiplayer)
        {
            if (ConnectionConfig.IsLateSpectator)
            {
                Debug.Log("Опоздавший зритель");
                await SetupNetworkLateSpectatorGame();
            }
            else
            {
                Debug.Log("Обычный зритель");
                await SetupNetworkGame();
            }
        }
    }

    private void InitializeUI()
    {
        root = uiDocument.rootVisualElement;
        firstOpponentImage = root.Q<VisualElement>("firstOpponentImage");
        firstOpponentName = root.Q<Label>("firstOpponentName");
        secondOpponentImage = root.Q<VisualElement>("secondOpponentImage");
        secondOpponentName = root.Q<Label>("secondOpponentName");
        progressBar = root.Q<ProgressBar>("progressBar");
        progressLabel = root.Q<Label>("progressLabel");
    }

    private void FillOpponentsInfoInBotGame()
    {
        Texture2D photoTexture = BytesToTexture(UserSession.Instance.ActiveUser.userData.profilePhotoData);
        if (photoTexture != null) firstOpponentImage.style.backgroundImage = new StyleBackground(photoTexture);
        firstOpponentName.text = UserSession.Instance.ActiveUser.userData.userName;

        secondOpponentImage.style.backgroundImage = new StyleBackground(Resources.Load<Sprite>("Sprites/defaultAvatar"));
        secondOpponentName.text = "BOT";
    }

    private Texture2D BytesToTexture(byte[] bytes)
    {
        Texture2D texture = new(2, 2);
        if (texture.LoadImage(bytes)) return texture;
        return null;
    }

    private async Task SetupBotGame()
    {
        List<int> playerDeck = SelectedDeckManager.GetSelectedDeckIds();

        static List<int> GenerateBotDeck(int cardCount)
        {
            GameCardModelList allGameCards = CardRepository.Instance.GetGameCardsList();
            List<int> botDeck = allGameCards.cards.Where(card => card.evolutions.prev == null)
                .OrderBy(card => Random.value).Take(cardCount).Select(card => card.id).ToList();
            return botDeck;
        }

        List<int> botDeck = GenerateBotDeck(5);
        BotDeckData.BotDeckIds = botDeck;
        List<int> cardsToLoad = playerDeck.Concat(botDeck).ToList();

        ChangeLabelText(Localizer.GetLocalizedText("CreatingModelsLabel"));

        await CardRepository.Instance.PreBuildBattleCardControllers(cardPrefab3D, cardsToLoad);

        ChangeLabelText(Localizer.GetLocalizedText("ArenaPreparationLabel"));
        await Task.Delay(4000);

        SceneManager.LoadScene("GameScene");
    }

    private async Task SetupNetworkGame()
    {
        firstOpponentName.text = NetworkGamePlayersInfoCache.HostName;
        Texture2D hostTexture = AvatarStorage.GetAvatar(NetworkGamePlayersInfoCache.HostPlayerId);
        firstOpponentImage.style.backgroundImage = new StyleBackground(hostTexture);

        secondOpponentName.text = NetworkGamePlayersInfoCache.ClientName;
        Texture2D clientTexture = AvatarStorage.GetAvatar(NetworkGamePlayersInfoCache.ClientPlayerId);
        secondOpponentImage.style.backgroundImage = new StyleBackground(clientTexture);

        ChangeLabelText(Localizer.GetLocalizedText("WaitingServerLable"));

        NetworkRunner networkRunner = NetworkRunnerController.Instance.NetworkRunner;
        while (networkRunner == null || !networkRunner.IsRunning)
        {
            await Task.Yield();
            networkRunner = NetworkRunnerController.Instance.NetworkRunner;
        }

        await Task.Delay(100);

        if (networkRunner.IsSharedModeMasterClient && FindAnyObjectByType<NetworkGameController>() == null)
        {
            await networkRunner.SpawnAsync(networkGameControllerPrefab);
            await networkRunner.SpawnAsync(networkGameTurnManagerPrefab);
        }

        NetworkGameTurnManager networkGameTurnManager = null;
        while (networkGameController == null || networkGameTurnManager == null)
        {
            networkGameController = FindAnyObjectByType<NetworkGameController>();
            networkGameTurnManager = FindAnyObjectByType<NetworkGameTurnManager>();
            await Task.Yield();
        }

        DontDestroyOnLoad(networkGameController.gameObject);
        DontDestroyOnLoad(networkGameTurnManager.gameObject);

        ChangeLabelText(Localizer.GetLocalizedText("WaitingOpponentLabel"));

        while (!networkGameController.FirstPlayerReady || !networkGameController.SecondPlayerReady)
            await Task.Yield();

        ChangeLabelText(Localizer.GetLocalizedText("SyncDataLabel"));

        List<int> cardsToLoad = networkGameController.GetPlayerDeckIds()
            .Concat(networkGameController.GetEnemyDeckIds()).ToList();

        ChangeLabelText(Localizer.GetLocalizedText("CreatingModelsLabel"));

        await CardRepository.Instance.PreBuildBattleCardControllers(cardPrefab3D, cardsToLoad);

        ChangeLabelText(Localizer.GetLocalizedText("EnteringGameLabel"));

        if (networkRunner.IsSharedModeMasterClient)
        {
            await Task.Delay(4000);
            int gameSceneIndex = SceneUtility.GetBuildIndexByScenePath("Assets/Scenes/GameScene.unity");
            await networkRunner.LoadScene(SceneRef.FromIndex(gameSceneIndex));
        }
    }

    private async Task SetupNetworkLateSpectatorGame()
    {
        ChangeLabelText("ConnectToMatchLabel");

        NetworkGameTurnManager networkGameTurnManager = null;
        while (networkGameController == null || networkGameTurnManager == null || NetworkAvatarSyncer.Instance == null)
        {
            networkGameController = FindAnyObjectByType<NetworkGameController>();
            networkGameTurnManager = FindAnyObjectByType<NetworkGameTurnManager>();
            await Task.Yield();
        }

        DontDestroyOnLoad(networkGameController.gameObject);
        DontDestroyOnLoad(networkGameTurnManager.gameObject);

        ChangeLabelText(Localizer.GetLocalizedText("SyncDataLabel"));

        NetworkAvatarSyncer.Instance.OnAvatarReceived += HandleAvatarReceived;
        NetworkAvatarSyncer.Instance.RequestAvatars();

        firstOpponentName.text = networkGameController.FirstPlayerName.ToString();
        secondOpponentName.text = networkGameController.SecondPlayerName.ToString();

        NetworkGamePlayersInfoCache.HostName = networkGameController.FirstPlayerName.ToString();
        NetworkGamePlayersInfoCache.ClientName = networkGameController.SecondPlayerName.ToString();

        PlayerPresence[] allPresences = FindObjectsByType<PlayerPresence>(FindObjectsInactive.Exclude);
        List<PlayerPresence> realPlayers = allPresences.Where(p => !p.IsSpectator).OrderBy(p => p.Object.InputAuthority.PlayerId).ToList();

        NetworkGamePlayersInfoCache.HostPlayerId = realPlayers[0].Object.InputAuthority.PlayerId;
        NetworkGamePlayersInfoCache.ClientPlayerId = realPlayers[1].Object.InputAuthority.PlayerId;

        float waitTime = 0;
        while (AvatarStorage.AvatarCache.Count < 2 && waitTime < 3f)
        {
            waitTime += Time.deltaTime;
            await Task.Yield();
        }

        foreach (KeyValuePair<int, Texture2D> cachedPair in AvatarStorage.AvatarCache)
            HandleAvatarReceived(cachedPair.Key, cachedPair.Value);

        List<int> cardsToLoad = networkGameController.GetPlayerDeckIds()
            .Concat(networkGameController.GetEnemyDeckIds()).ToList();

        ChangeLabelText(Localizer.GetLocalizedText("CreatingModelsLabel"));

        await CardRepository.Instance.PreBuildBattleCardControllers(cardPrefab3D, cardsToLoad);

        ChangeLabelText(Localizer.GetLocalizedText("EnteringSpectratorModeLabel"));
        await Task.Delay(4000);

        while (SceneManager.GetActiveScene().name != "GameScene")
            await Task.Yield();

        ConnectionConfig.IsLateSpectator = false;
        Destroy(gameObject);
    }

    private void HandleAvatarReceived(int realPlayerId, Texture2D image)
    {
        if (networkGameController == null) return;

        if (realPlayerId == networkGameController.FirstPlayerRef.PlayerId)
            firstOpponentImage.style.backgroundImage = new StyleBackground(image);
        else if (realPlayerId == networkGameController.SecondPlayerRef.PlayerId)
            secondOpponentImage.style.backgroundImage = new StyleBackground(image);
    }

    private void ChangeLabelText(string text)
    {
        baseUploadingText = text;
        progressLabel.text = text;
    }

    private void StartDotsAnimation()
    {
        baseUploadingText = progressLabel.text;
        dotsSequence = DOTween.Sequence();
        dotsSequence.AppendCallback(() => progressLabel.text = baseUploadingText).AppendInterval(0.5f)
        .AppendCallback(() => progressLabel.text = baseUploadingText + ".").AppendInterval(0.5f)
        .AppendCallback(() => progressLabel.text = baseUploadingText + "..").AppendInterval(0.5f)
        .AppendCallback(() => progressLabel.text = baseUploadingText + "...").AppendInterval(0.5f)
        .SetLoops(-1, LoopType.Restart);
    }

    private void HandleProgress(float progress)
    {
        float targetValue = progress * 100f;
        progressTween?.Kill();
        progressTween = DOTween.To(() => progressBar.value, x => progressBar.value = x, targetValue, 0.2f).SetEase(Ease.OutQuad);
    }

    private void OnDestroy()
    {
        CardRepository.Instance.OnProgressChanged -= HandleProgress;
        dotsSequence?.Kill();
        progressTween?.Kill();
    }
}