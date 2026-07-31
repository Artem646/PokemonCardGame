using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameInterfaceController : MonoBehaviour
{
    [SerializeField] private Button defaultCameraViewButton;
    [SerializeField] private Button topCameraViewButton;
    [SerializeField] private Button leftCameraViewButton;
    [SerializeField] private Button closeZoomOnCardCameraViewButton;
    [SerializeField] private Button exitButton;

    [SerializeField] private GameObject leftTableFence;
    [SerializeField] private GameObject bottomTableFence;

    [SerializeField] private TextMeshProUGUI userName;
    [SerializeField] private Image userImage;
    [SerializeField] private TextMeshProUGUI botName;
    [SerializeField] private Image botImage;
    [SerializeField] private GameObject spectatorsCountPanel;

    [SerializeField] private CanvasGroup playerInfoPanelCanvasGroup;
    [SerializeField] private CanvasGroup zoomLeftPanelCanvasGroup;
    [SerializeField] private CanvasGroup zoomRightPanelCanvasGroup;
    [SerializeField] private CanvasGroup attackTargetingPanelCanvasGroup;

    [SerializeField] private ZoomElementsInteractionPanelController zoomElementsInteractionPanelController;
    [SerializeField] private ZoomEnemyFieldCardsElementsPanelController zoomEnemyFieldCardsElementsPanelController;
    [SerializeField] private AttackTargetingPanelController attackTargetingPanelController;

    [SerializeField] private CameraViewManager cameraViewManager;
    [SerializeField] private GameManager gameManager;

    private BotTurnManager botTurnManager;
    private NetworkGameTurnManager networkGameTurnManager;

    private CameraViewMode prevGlobalViewMode = CameraViewMode.Default;
    private CardSlot pendingZoomedSlot;

    private void Start()
    {
        if (GameTypeConfig.CurrentType == GameType.Bot)
        {
            FillInfoPanel();
            if (botTurnManager == null) botTurnManager = FindAnyObjectByType<BotTurnManager>();
            botTurnManager.OnPhaseChanged += HandlePhaseChanged;
        }
        else if (GameTypeConfig.CurrentType == GameType.Multiplayer)
        {
            if (networkGameTurnManager == null) networkGameTurnManager = FindAnyObjectByType<NetworkGameTurnManager>();
            networkGameTurnManager.OnPhaseChanged += HandlePhaseChanged;
        }

        cameraViewManager.OnCameraMovementFinished += HandleCameraMovementFinished;

        RegisterCallbacks();
    }

    public void SetStartCameraView()
    {
        if (ConnectionConfig.IsSpectator)
        {
            ChangeGlobalView(CameraViewMode.Left);
            leftCameraViewButton.gameObject.SetActive(true);
        }
        else
        {
            ChangeGlobalView(CameraViewMode.Default);
            leftCameraViewButton.gameObject.SetActive(false);
        }
    }

    private void RegisterCallbacks()
    {
        defaultCameraViewButton.onClick.AddListener(() => ChangeGlobalView(CameraViewMode.Default));
        topCameraViewButton.onClick.AddListener(() => ChangeGlobalView(CameraViewMode.Top));
        leftCameraViewButton.onClick.AddListener(() => ChangeGlobalView(CameraViewMode.Left));
        closeZoomOnCardCameraViewButton.onClick.AddListener(CloseZoomOnCardView);
        exitButton.onClick.AddListener(OnBackButtonClickedAsync);
    }

    private void HandlePhaseChanged(TurnPhase phase) => RefreshCameraButtons(gameManager.IsMyTurn, phase);

    private TurnPhase GetCurrentPhase()
    {
        if (GameTypeConfig.CurrentType == GameType.Bot)
        {
            if (botTurnManager == null) botTurnManager = FindAnyObjectByType<BotTurnManager>();
            return botTurnManager.CurrentPhase;
        }
        else if (GameTypeConfig.CurrentType == GameType.Multiplayer)
        {
            if (networkGameTurnManager == null) networkGameTurnManager = FindAnyObjectByType<NetworkGameTurnManager>();
            return networkGameTurnManager.CurrentPhase;
        }

        return TurnPhase.Waiting;
    }

    public void RefreshCameraButtons(bool isMyTurn, TurnPhase phase)
    {
        if (cameraViewManager.IsZoomView)
            SetButtonsInteractable(false, false, false, true);
        else
        {
            bool canControl = ConnectionConfig.IsSpectator || (isMyTurn && phase != TurnPhase.Waiting);
            bool def = canControl && cameraViewManager.CurrentViewMode != CameraViewMode.Default;
            bool top = canControl && cameraViewManager.CurrentViewMode != CameraViewMode.Top;
            bool left = canControl && cameraViewManager.CurrentViewMode != CameraViewMode.Left;
            SetButtonsInteractable(def, top, left, false);
        }
    }

    private void SetButtonsInteractable(bool def, bool top, bool left, bool close)
    {
        defaultCameraViewButton.interactable = def;
        topCameraViewButton.interactable = top;
        leftCameraViewButton.interactable = left;
        closeZoomOnCardCameraViewButton.interactable = close;

        closeZoomOnCardCameraViewButton.gameObject.SetActive(close);
    }

    private void ChangeGlobalView(CameraViewMode targetViewMode)
    {
        if (cameraViewManager.IsSwitching) return;

        prevGlobalViewMode = targetViewMode;
        cameraViewManager.SwitchToView(targetViewMode);

        leftTableFence.SetActive(targetViewMode == CameraViewMode.Default || targetViewMode == CameraViewMode.Top);
        bottomTableFence.SetActive(targetViewMode == CameraViewMode.Top || targetViewMode == CameraViewMode.Left);

        RefreshCameraButtons(gameManager.IsMyTurn, GetCurrentPhase());
    }

    public void MoveToZoomOnCard(CardSlot cardSlot)
    {
        if (cameraViewManager.IsSwitching) return;

        CardSlot currentZoomedSlot = cameraViewManager.CurrentZoomedSlot;
        if (currentZoomedSlot != null && currentZoomedSlot != cardSlot)
        {
            if (currentZoomedSlot.CurrentCard.TryGetComponent<CardControllerLink>(out var link))
            {
                link.Controller.ResetAbilityWithoutConfirmation();
                link.Controller.UpdateActivitiesPlatesColor();
            }
        }

        pendingZoomedSlot = cardSlot;

        SetButtonsInteractable(false, false, false, true);

        leftTableFence.SetActive(true);
        bottomTableFence.SetActive(false);

        playerInfoPanelCanvasGroup.alpha = 0;
        zoomLeftPanelCanvasGroup.alpha = 0;
        zoomRightPanelCanvasGroup.alpha = 0;
        attackTargetingPanelCanvasGroup.alpha = 0;

        cameraViewManager.MoveToZoomView(cardSlot);
    }

    private void HandleCameraMovementFinished(CameraViewMode viewMode)
    {
        if (viewMode == CameraViewMode.Zoom && pendingZoomedSlot != null)
        {
            if (pendingZoomedSlot.CurrentCard != null && pendingZoomedSlot.CurrentCard.TryGetComponent<CardControllerLink>(out var link))
            {
                link.Controller.UpdateActivitiesPlatesColor();

                zoomLeftPanelCanvasGroup.alpha = 1;
                zoomElementsInteractionPanelController.UpdatePanel(link.Controller.CardModel);

                if (pendingZoomedSlot.type == FieldSlotType.SelfFieldSlot || pendingZoomedSlot.type == FieldSlotType.SelfHandSlot)
                {
                    zoomRightPanelCanvasGroup.alpha = 1;
                    zoomEnemyFieldCardsElementsPanelController.UpdatePanel(pendingZoomedSlot, link.Controller);
                }
            }

            attackTargetingPanelCanvasGroup.alpha = 0;
            pendingZoomedSlot = null;
        }
    }

    public void CloseZoomOnCardView()
    {
        if (!cameraViewManager.IsZoomView) return;

        ResetCardStateInZoom(cameraViewManager.CurrentZoomedSlot);

        zoomLeftPanelCanvasGroup.alpha = 0;
        zoomRightPanelCanvasGroup.alpha = 0;
        playerInfoPanelCanvasGroup.alpha = 1;

        TurnPhase currentPhase = TurnPhase.Waiting;
        if (GameTypeConfig.CurrentType == GameType.Bot) currentPhase = botTurnManager.CurrentPhase;
        else if (GameTypeConfig.CurrentType == GameType.Multiplayer) currentPhase = networkGameTurnManager.CurrentPhase;

        if (currentPhase == TurnPhase.Attack)
        {
            attackTargetingPanelCanvasGroup.alpha = 1;
            attackTargetingPanelController.UpdatePanel();
        }

        ChangeGlobalView(prevGlobalViewMode);
    }

    private void ResetCardStateInZoom(CardSlot slot)
    {
        CardStateInteractionManager.EndRaise();

        if (slot.CurrentCard != null)
        {
            DOTween.Kill(slot.CurrentCard.transform);

            if (slot.CurrentCard.TryGetComponent<CardControllerLink>(out var link))
            {
                link.Controller.ResetAbilityWithoutConfirmation();
                link.Controller.UpdateActivitiesPlatesColor();
            }
        }
    }

    private void FillInfoPanel()
    {
        static Texture2D BytesToTexture(byte[] bytes)
        {
            Texture2D texture = new(2, 2);
            if (texture.LoadImage(bytes)) return texture;
            return null;
        }

        Texture2D texture = BytesToTexture(UserSession.Instance.ActiveUser.userData.profilePhotoData);
        if (texture != null)
        {
            userImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            userImage.color = Color.white;
        }
        else
        {
            userImage.sprite = null;
            userImage.color = new Color(0, 0, 0, 0.2f);
        }

        userName.text = UserSession.Instance.ActiveUser.userData.userName;

        botImage.sprite = Resources.Load<Sprite>("Sprites/defaultAvatar");
        botImage.color = Color.white;
        botName.text = "BOT";

        spectatorsCountPanel.SetActive(false);
    }

    private async void OnBackButtonClickedAsync()
    {
        if (GameTypeConfig.CurrentType == GameType.Multiplayer)
        {
            await NetworkRunnerController.Instance.DisconnectAndRejoinLobby();
            SceneManager.LoadScene("RoomManagerScene");
        }
        else if (GameTypeConfig.CurrentType == GameType.Bot)
            SceneManager.LoadScene("CollectionScene");
    }

    private void OnDestroy()
    {
        if (cameraViewManager != null)
            cameraViewManager.OnCameraMovementFinished -= HandleCameraMovementFinished;
    }
}