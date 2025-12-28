using UnityEngine;
using UnityEngine.UIElements;
using DG.Tweening;

public static class CollectionCardScaleAnimator
{
    private static VisualElement clone;
    private static Rect lastLocalRect;
    private static bool isAnimating = false;
    private const float TARGET_SCALE = 2f;
    private const float DURATION = 0.35f;

    // public static void ShowCard(VisualElement sourceCard, VisualElement cloneCard, VisualElement overlay, Vector2 clickPosition)
    // {
    //     if (isAnimating || !IsClickInCenter(sourceCard, clickPosition))
    //         return;

    //     isAnimating = true;

    //     var worldRect = sourceCard.worldBound;
    //     var topLeftLocal = overlay.WorldToLocal(new Vector2(worldRect.x, worldRect.y));
    //     Rect localStart = new(topLeftLocal.x, topLeftLocal.y, worldRect.width, worldRect.height);
    //     lastLocalRect = localStart;

    //     overlay.Clear();
    //     overlay.RemoveFromClassList("overlay-none");
    //     overlay.AddToClassList("overlay-active");
    //     overlay.Add(cloneCard);

    //     clone = cloneCard;

    //     clone.style.position = Position.Absolute;
    //     clone.style.left = localStart.x;
    //     clone.style.top = localStart.y;
    //     clone.style.width = localStart.width;
    //     clone.style.height = localStart.height;
    //     clone.style.transformOrigin = new TransformOrigin(50, 50, 0);
    //     clone.style.scale = new Scale(Vector3.one);
    //     clone.style.opacity = 0f;

    //     var rootBounds = overlay.panel.visualTree.worldBound;
    //     var rootCenter = rootBounds.center;
    //     var localCenter = overlay.WorldToLocal(rootCenter);

    //     float targetWidth = localStart.width * TARGET_SCALE;
    //     float targetHeight = localStart.height * TARGET_SCALE;
    //     float targetX = localCenter.x - targetWidth / 2f;
    //     float targetY = localCenter.y - targetHeight / 2f;

    //     float currentLeft = localStart.x;
    //     float currentTop = localStart.y;
    //     float currentWidth = localStart.width;
    //     float currentHeight = localStart.height;
    //     float currentScale = 1f;
    //     float currentOpacity = 0f;

    //     DOTween.To(() => currentLeft, x => { currentLeft = x; clone.style.left = x; }, targetX, DURATION).SetEase(Ease.InQuad);
    //     DOTween.To(() => currentTop, y => { currentTop = y; clone.style.top = y; }, targetY, DURATION).SetEase(Ease.InQuad);
    //     DOTween.To(() => currentWidth, w => { currentWidth = w; clone.style.width = w; }, targetWidth, DURATION).SetEase(Ease.InQuad);
    //     DOTween.To(() => currentHeight, h => { currentHeight = h; clone.style.height = h; }, targetHeight, DURATION).SetEase(Ease.InQuad);
    //     DOTween.To(() => currentScale, s => { currentScale = s; clone.style.scale = new Scale(new Vector3(s, s, 1f)); },
    //                TARGET_SCALE, DURATION).SetEase(Ease.OutBack);
    //     DOTween.To(() => currentOpacity, o => { currentOpacity = o; clone.style.opacity = o; }, 1f, DURATION).SetEase(Ease.Linear)
    //                .OnComplete(() => { isAnimating = false; });
    // }

    public static void ShowCard(VisualElement sourceCard, VisualElement cloneCard, VisualElement overlay, Vector2 clickPosition)
    {
        if (isAnimating || !IsClickInCenter(sourceCard, clickPosition))
            return;

        isAnimating = true;

        var worldRect = sourceCard.worldBound;
        var topLeftLocal = overlay.WorldToLocal(new Vector2(worldRect.x, worldRect.y));
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
        clone.style.width = localStart.width * TARGET_SCALE;   // сразу увеличенный размер
        clone.style.height = localStart.height * TARGET_SCALE; // сразу увеличенный размер
        clone.style.transformOrigin = new TransformOrigin(50, 50, 0);
        clone.style.scale = new Scale(new Vector3(TARGET_SCALE, TARGET_SCALE, 1f)); // сразу увеличенный масштаб
        clone.style.opacity = 0f;

        var rootBounds = overlay.panel.visualTree.worldBound;
        var rootCenter = rootBounds.center;
        var localCenter = overlay.WorldToLocal(rootCenter);

        float targetX = localCenter.x - clone.style.width.value.value / 2f;
        float targetY = localCenter.y - clone.style.height.value.value / 2f;

        float currentLeft = localStart.x;
        float currentTop = localStart.y;
        float currentOpacity = 0f;

        // Летим в центр
        DOTween.To(() => currentLeft, x => { currentLeft = x; clone.style.left = x; }, targetX, DURATION).SetEase(Ease.OutQuad);
        DOTween.To(() => currentTop, y => { currentTop = y; clone.style.top = y; }, targetY, DURATION).SetEase(Ease.OutQuad);

        // Плавное появление
        DOTween.To(() => currentOpacity, o => { currentOpacity = o; clone.style.opacity = o; }, 1f, DURATION).SetEase(Ease.Linear)
               .OnComplete(() => { isAnimating = false; });
    }


    public static void HideCard(VisualElement overlay)
    {
        if (isAnimating || overlay.childCount == 0) return;
        if (clone == null) return;

        isAnimating = true;

        DOTween.To(() => clone.resolvedStyle.left, x => clone.style.left = x, lastLocalRect.x, DURATION).SetEase(Ease.InQuad);
        DOTween.To(() => clone.resolvedStyle.top, y => clone.style.top = y, lastLocalRect.y, DURATION).SetEase(Ease.InQuad);
        DOTween.To(() => clone.resolvedStyle.width, w => clone.style.width = w, lastLocalRect.width, DURATION).SetEase(Ease.InQuad);
        DOTween.To(() => clone.resolvedStyle.height, h => clone.style.height = h, lastLocalRect.height, DURATION).SetEase(Ease.InQuad);
        DOTween.To(() => clone.resolvedStyle.scale.value.x,
            s => clone.style.scale = new Scale(new Vector3(s, s, 1f)), 1f, DURATION).SetEase(Ease.InQuad);

        DOTween.To(() => clone.resolvedStyle.opacity, o => clone.style.opacity = o, 0f, DURATION).SetEase(Ease.Linear)
                   .OnComplete(() =>
                   {
                       overlay.Clear();
                       overlay.RemoveFromClassList("overlay-active");
                       overlay.AddToClassList("overlay-none");
                       isAnimating = false;
                   });
    }

    private static bool IsClickInCenter(VisualElement card, Vector2 clickPosition)
    {
        var bounds = card.worldBound;

        float centerX = bounds.x + bounds.width / 2f;
        float centerY = bounds.y + bounds.height / 2f;

        float centerWidth = bounds.width * 0.6f;
        float centerHeight = bounds.height * 0.6f;

        Rect centerRect = new(
            centerX - centerWidth / 2f,
            centerY - centerHeight / 2f,
            centerWidth,
            centerHeight
        );

        return centerRect.Contains(clickPosition);
    }
}