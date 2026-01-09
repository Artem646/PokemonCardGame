using UnityEngine;
using UnityEngine.UIElements;
using DG.Tweening;

public static class CardScaleAnimatorUIToolkit
{
    private static VisualElement clone;
    private static Rect lastLocalRect;
    private static Sequence currentSequence;

    public static float TargetScale { get; set; }
    public static float Duration { get; set; }

    public static void ShowCard(VisualElement sourceCard, VisualElement cloneCard, VisualElement overlay, float scale, float duration)
    {
        if (currentSequence != null)
            return;

        TargetScale = scale;
        Duration = duration;

        Rect worldRect = sourceCard.worldBound;
        Vector2 topLeftLocal = overlay.WorldToLocal(new Vector2(worldRect.x, worldRect.y));
        Rect localStart = new(topLeftLocal.x, topLeftLocal.y, worldRect.width, worldRect.height);
        lastLocalRect = localStart;

        overlay.Clear();
        overlay.RemoveFromClassList("overlay-none");
        overlay.AddToClassList("overlay-active");
        overlay.Add(cloneCard);

        clone = cloneCard;

        clone.style.position = Position.Absolute;
        clone.style.left = localStart.x;
        clone.style.top = localStart.y;
        clone.style.width = localStart.width * TargetScale;
        clone.style.height = localStart.height * TargetScale;
        clone.style.transformOrigin = new TransformOrigin(50, 50, 0);
        clone.style.scale = new Scale(new Vector3(TargetScale, TargetScale, 1f));
        clone.style.opacity = 0f;

        Rect rootBounds = overlay.panel.visualTree.worldBound;
        Vector2 rootCenter = rootBounds.center;
        Vector2 localCenter = overlay.WorldToLocal(rootCenter);

        float targetX = localCenter.x - clone.style.width.value.value / 2f;
        float targetY = localCenter.y - clone.style.height.value.value / 2f;

        currentSequence = DOTween.Sequence();

        currentSequence.Append(DOTween.To(() => clone.style.left.value.value, x => clone.style.left = x, targetX, Duration).SetEase(Ease.OutQuad));
        currentSequence.Join(DOTween.To(() => clone.style.top.value.value, y => clone.style.top = y, targetY, Duration).SetEase(Ease.OutQuad));
        currentSequence.Join(DOTween.To(() => clone.style.opacity.value, o => clone.style.opacity = o, 1f, Duration).SetEase(Ease.Linear));

        currentSequence.OnComplete(() => currentSequence = null);
    }

    public static void HideCard(VisualElement overlay)
    {
        if (overlay.childCount == 0 || clone == null || currentSequence != null)
            return;

        currentSequence = DOTween.Sequence();

        currentSequence.Append(DOTween.To(() => clone.resolvedStyle.left, x => clone.style.left = x, lastLocalRect.x, Duration).SetEase(Ease.InQuad));
        currentSequence.Join(DOTween.To(() => clone.resolvedStyle.top, y => clone.style.top = y, lastLocalRect.y, Duration).SetEase(Ease.InQuad));
        currentSequence.Join(DOTween.To(() => clone.resolvedStyle.width, w => clone.style.width = w, lastLocalRect.width, Duration).SetEase(Ease.InQuad));
        currentSequence.Join(DOTween.To(() => clone.resolvedStyle.height, h => clone.style.height = h, lastLocalRect.height, Duration).SetEase(Ease.InQuad));
        currentSequence.Join(DOTween.To(() => clone.resolvedStyle.scale.value.x,
            s => clone.style.scale = new Scale(new Vector3(s, s, 1f)), 1f, Duration).SetEase(Ease.InQuad));
        currentSequence.Join(DOTween.To(() => clone.resolvedStyle.opacity, o => clone.style.opacity = o, 0f, Duration).SetEase(Ease.Linear));

        currentSequence.OnComplete(() =>
        {
            overlay.Clear();
            overlay.RemoveFromClassList("overlay-active");
            overlay.AddToClassList("overlay-none");
            currentSequence = null;
        });
    }
}