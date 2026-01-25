using UnityEngine;
using UnityEngine.UIElements;
using DG.Tweening;

public static class CardScaleAnimatorUIToolkit
{
    private static VisualElement clone;
    private static Sequence currentSequence;
    private static Vector2 startOffset;

    public static float TargetScale { get; set; }
    public static float Duration { get; set; }

    public static void ShowCard(VisualElement sourceCard, VisualElement cloneCard, VisualElement overlay, float scale, float duration)
    {
        if (currentSequence != null)
            return;

        TargetScale = scale;
        Duration = duration;

        overlay.Clear();
        overlay.RemoveFromClassList("overlay-none");
        overlay.AddToClassList("overlay-active");
        overlay.Add(cloneCard);

        clone = cloneCard;

        clone.style.opacity = 0f;

        Vector2 localSourceCardCenter = overlay.WorldToLocal(sourceCard.worldBound.center);
        Vector2 localOverlayCenter = overlay.WorldToLocal(overlay.panel.visualTree.worldBound.center);

        startOffset = localSourceCardCenter - localOverlayCenter;

        clone.style.translate = new Translate(new Length(startOffset.x, LengthUnit.Pixel), new Length(startOffset.y, LengthUnit.Pixel));

        currentSequence = DOTween.Sequence();

        currentSequence.Join(DOTween.To(() => clone.style.translate.value.x.value,
            x => clone.style.translate = new Translate(new Length(x, LengthUnit.Pixel), clone.style.translate.value.y),
            0f, Duration).SetEase(Ease.OutQuad));

        currentSequence.Join(DOTween.To(() => clone.style.translate.value.y.value,
            y => clone.style.translate = new Translate(clone.style.translate.value.x, new Length(y, LengthUnit.Pixel)),
            0f, Duration).SetEase(Ease.OutQuad));

        currentSequence.Join(DOTween.To(() => clone.style.scale.value.value.x,
            s => clone.style.scale = new Scale(new Vector3(s, s, 1f)),
            TargetScale, Duration).SetEase(Ease.OutQuad));

        currentSequence.Join(DOTween.To(() => clone.style.opacity.value,
            o => clone.style.opacity = o,
            1f, Duration).SetEase(Ease.Linear));

        currentSequence.OnComplete(() => currentSequence = null);
    }

    public static void HideCard(VisualElement overlay)
    {
        if (overlay.childCount == 0 || clone == null || currentSequence != null)
            return;

        currentSequence = DOTween.Sequence();

        currentSequence.Join(DOTween.To(() => clone.style.translate.value.x.value,
            x => clone.style.translate = new Translate(new Length(x, LengthUnit.Pixel), clone.style.translate.value.y),
            startOffset.x, Duration).SetEase(Ease.InQuad));

        currentSequence.Join(DOTween.To(() => clone.style.translate.value.y.value,
            y => clone.style.translate = new Translate(clone.style.translate.value.x, new Length(y, LengthUnit.Pixel)),
            startOffset.y, Duration).SetEase(Ease.InQuad));

        currentSequence.Join(DOTween.To(() => clone.resolvedStyle.scale.value.x,
            s => clone.style.scale = new Scale(new Vector3(s, s, 1f)),
            1f, Duration).SetEase(Ease.InQuad));

        currentSequence.Join(DOTween.To(() => clone.resolvedStyle.opacity,
            o => clone.style.opacity = o,
            0f, Duration).SetEase(Ease.Linear));

        currentSequence.OnComplete(() =>
        {
            overlay.Clear();
            overlay.RemoveFromClassList("overlay-active");
            overlay.AddToClassList("overlay-none");
            currentSequence = null;
        });
    }
}
