// using UnityEngine;
// using DG.Tweening;
// using System.Collections.Generic;
// using System.Linq;

// public enum CameraViewMode
// {
//     Default,
//     Top,
//     Left
// }

// public class CameraViewManager : MonoBehaviour
// {
//     [SerializeField] private Camera mainCamera;
//     [SerializeField] private List<Transform> cameraViews = new();

//     public Transform CurrentView { get; private set; }
//     public CameraViewMode CurrentViewMode { get; private set; }

//     public bool IsSwitching { get; private set; }
//     private const float DURATION = 0.6f;

//     public void SwitchTo(CameraViewMode viewMode)
//     {
//         CurrentViewMode = viewMode;
//         int index = (int)viewMode;
//         SwitchTo(cameraViews[index]);
//     }

//     private void SwitchTo(Transform targetView)
//     {
//         if (IsSwitching || targetView == CurrentView)
//             return;

//         IsSwitching = true;
//         CurrentView = targetView;

//         Sequence sequence = DOTween.Sequence();
//         sequence.Join(mainCamera.transform.DOMove(targetView.position, DURATION).SetEase(Ease.OutQuad));
//         sequence.Join(mainCamera.transform.DORotateQuaternion(targetView.rotation, DURATION).SetEase(Ease.OutQuad));
//         sequence.OnComplete(() => IsSwitching = false);
//     }

//     public void ZoomToSlot(CardSlot cardSlot) => SwitchTo(cardSlot.zoomPoint);
// }

using System;
using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public enum CameraViewMode
{
    Default,
    Top,
    Left,
    Zoom
}

public class CameraViewManager : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private List<Transform> cameraViews = new();

    public Transform CurrentView { get; private set; }
    public CameraViewMode CurrentViewMode { get; private set; }
    public CardSlot CurrentZoomedSlot { get; private set; }

    public bool IsSwitching { get; private set; }
    private const float DURATION = 0.6f;

    private Tween moveTween;
    private Tween rotateTween;

    public bool IsZoomView => CurrentViewMode == CameraViewMode.Zoom;

    public event Action<CameraViewMode> OnCameraMovementFinished;

    public void SwitchToView(CameraViewMode viewMode)
    {
        if (IsSwitching) return;

        CurrentViewMode = viewMode;
        CurrentZoomedSlot = null;

        int index = (int)viewMode;
        SwitchToViewTransform(cameraViews[index]);
    }

    private void SwitchToViewTransform(Transform targetView)
    {
        if (IsSwitching || targetView == CurrentView)
            return;

        IsSwitching = true;
        CurrentView = targetView;

        QualitySettings.shadowDistance = 0f;

        moveTween?.Kill();
        rotateTween?.Kill();

        moveTween = mainCamera.transform.DOMove(targetView.position, DURATION).SetEase(Ease.OutQuad).SetUpdate(UpdateType.Late);
        rotateTween = mainCamera.transform.DORotateQuaternion(targetView.rotation, DURATION).SetEase(Ease.OutQuad).SetUpdate(UpdateType.Late)
        .OnComplete(() =>
        {
            IsSwitching = false;
            OnCameraMovementFinished?.Invoke(CurrentViewMode);
        });
    }

    public void MoveToZoomView(CardSlot cardSlot)
    {
        if (IsSwitching || cardSlot == null) return;

        CurrentZoomedSlot = null;
        CurrentViewMode = CameraViewMode.Zoom;
        CurrentZoomedSlot = cardSlot;

        SwitchToViewTransform(cardSlot.zoomPoint);
    }

    public void ExitFromZoomView(CameraViewMode prevViewMode = CameraViewMode.Default)
    {
        CurrentZoomedSlot = null;
        SwitchToView(prevViewMode);
    }

    private void OnDestroy()
    {
        moveTween?.Kill();
        rotateTween?.Kill();
    }
}