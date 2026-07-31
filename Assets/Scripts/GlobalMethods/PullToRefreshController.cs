using UnityEngine;
using UnityEngine.UIElements;
using System.Threading.Tasks;
using DG.Tweening;

public class PullToRefreshController : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private RoomManagerSceneController roomManagerSceneController;

    private VisualElement root;
    private ScrollView roomList;
    private VisualElement refreshIndicator;
    private Label refreshLabel;

    private const float MAX_PULL_DISTANCE = 160f;
    private const float REFRESH_THRESHOLD = 100f;
    private const float INDICATOR_HEIGHT_WHEN_REFRESH = 95f;
    private const float ANIMATION_DURATION = 0.35f;

    private bool isReadyToPull;
    private bool isPulling;
    private bool isRefreshing;
    private Vector2 startPos;
    private Tween bounceTween;

    private void OnEnable()
    {
        root = uiDocument.rootVisualElement;
        roomList = root.Q<ScrollView>("roomList");
        refreshIndicator = root.Q<VisualElement>("refreshIndicator");
        refreshLabel = root.Q<Label>("refreshLabel");

        EnsureIndicatorIsFirst();

        refreshIndicator.style.position = Position.Relative;
        refreshIndicator.style.translate = StyleKeyword.Null;
        refreshIndicator.style.top = StyleKeyword.Null;

        refreshIndicator.style.height = 0f;
        refreshIndicator.style.minHeight = 0f;
        refreshIndicator.style.overflow = Overflow.Hidden;
        refreshIndicator.style.justifyContent = Justify.FlexEnd;

        roomList.RegisterCallback<PointerDownEvent>(OnPointerDown, TrickleDown.TrickleDown);
        roomList.RegisterCallback<PointerMoveEvent>(OnPointerMove, TrickleDown.TrickleDown);
        roomList.RegisterCallback<PointerUpEvent>(OnPointerUp, TrickleDown.TrickleDown);
        roomList.RegisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut, TrickleDown.TrickleDown);
    }

    private void OnDisable() => bounceTween?.Kill();

    private void EnsureIndicatorIsFirst()
    {
        if (refreshIndicator.parent != roomList.contentContainer || roomList.contentContainer.IndexOf(refreshIndicator) != 0)
        {
            refreshIndicator.RemoveFromHierarchy();
            roomList.contentContainer.Insert(0, refreshIndicator);
        }
    }

    private void OnPointerDown(PointerDownEvent evt)
    {
        if (isRefreshing) return;

        if (roomList.scrollOffset.y <= 0)
        {
            EnsureIndicatorIsFirst();

            isReadyToPull = true;
            isPulling = false;
            startPos = evt.position;
            bounceTween?.Kill();
        }
    }

    private void OnPointerMove(PointerMoveEvent evt)
    {
        if (!isReadyToPull || isRefreshing) return;

        float deltaY = evt.position.y - startPos.y;

        if (!isPulling && deltaY > 5f && roomList.scrollOffset.y <= 0)
        {
            isPulling = true;
            roomList.CapturePointer(evt.pointerId);
        }

        if (isPulling)
        {
            evt.StopPropagation();

            if (deltaY < 0) deltaY = 0;

            float pullDistance = Mathf.Pow(deltaY, 0.8f) * 1.5f;

            if (pullDistance > MAX_PULL_DISTANCE) pullDistance = MAX_PULL_DISTANCE;

            refreshIndicator.style.height = pullDistance;

            if (pullDistance >= REFRESH_THRESHOLD) refreshLabel.text = "Отпустите для обновления";
            else refreshLabel.text = "Потяните вниз...";
        }
    }

    private void OnPointerUp(PointerUpEvent evt)
    {
        if (!isReadyToPull) return;
        isReadyToPull = false;

        if (isPulling)
        {
            roomList.ReleasePointer(evt.pointerId);
            ReleaseDrag(evt.position.y - startPos.y);
        }
    }

    private void OnPointerCaptureOut(PointerCaptureOutEvent evt)
    {
        if (!isReadyToPull) return;
        isReadyToPull = false;
        if (isPulling) ReleaseDrag(0);
    }

    private async void ReleaseDrag(float rawDeltaY)
    {
        isPulling = false;

        float deltaY = Mathf.Max(0, rawDeltaY);
        float pullDistance = Mathf.Pow(deltaY, 0.8f) * 1.5f;
        float currentHeight = Mathf.Clamp(pullDistance, 0, MAX_PULL_DISTANCE);

        if (currentHeight >= REFRESH_THRESHOLD)
        {
            isRefreshing = true;

            refreshLabel.text = "Обновление списка...";

            AnimateHeight(currentHeight, INDICATOR_HEIGHT_WHEN_REFRESH);

            await SimulateNetworkRequest();

            isRefreshing = false;

            AnimateHeight(INDICATOR_HEIGHT_WHEN_REFRESH, 0f);
        }
        else
            AnimateHeight(currentHeight, 0f);
    }

    private void AnimateHeight(float startHeight, float endHeight)
    {
        bounceTween?.Kill();

        bounceTween = DOVirtual.Float(
            startHeight,
            endHeight,
            ANIMATION_DURATION,
            (value) => { refreshIndicator.style.height = Mathf.Max(0, value); })
        .SetEase(Ease.OutQuad);
    }

    private async Task SimulateNetworkRequest()
    {
        await Task.Delay(1500);
        roomManagerSceneController.RefreshRooms();
    }
}