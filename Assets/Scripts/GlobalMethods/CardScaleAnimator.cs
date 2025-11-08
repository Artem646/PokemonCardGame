// using UnityEngine;
// using UnityEngine.UIElements;
// using System;
// using UnityEngine.UIElements.Experimental;

// public static class CardScaleAnimator
// {
//     private static Rect lastLocalRect;
//     public static void AnimateCardFromListToOverlay(VisualElement sourceCard, VisualElement clone, VisualElement overlay)
//     {
//         // 1) Исходный прямоугольник карточки
//         var worldRect = sourceCard.worldBound;
//         var topLeftLocal = overlay.WorldToLocal(new Vector2(worldRect.x, worldRect.y));
//         Rect localStart = new Rect(topLeftLocal.x, topLeftLocal.y, worldRect.width, worldRect.height);
//         lastLocalRect = localStart;

//         // 2) Подготовка overlay
//         overlay.Clear();
//         overlay.RemoveFromClassList("overlay-none");
//         overlay.AddToClassList("overlay-active");

//         // 3) Клон карточки
//         overlay.Add(clone);

//         // 4) Начальные стили
//         clone.style.position = Position.Absolute;
//         clone.style.left = localStart.x;
//         clone.style.top = localStart.y;
//         clone.style.width = localStart.width;
//         clone.style.height = localStart.height;
//         clone.style.transformOrigin = new TransformOrigin(50, 50, 0);
//         clone.style.scale = new Scale(Vector3.one);
//         clone.style.opacity = 0f;

//         // 5) Цель — центр окна
//         var rootBounds = overlay.panel.visualTree.worldBound;
//         var rootCenter = rootBounds.center;
//         var localCenter = overlay.WorldToLocal(rootCenter);

//         float targetScale = 1.5f;
//         float targetWidth = localStart.width * 1.5f;
//         float targetHeight = localStart.height * 1.5f;
//         float targetX = localCenter.x - targetWidth / 2f;
//         float targetY = localCenter.y - targetHeight / 2f;

//         // 6) Запускаем анимацию сразу
//         AnimateRectAndScale(clone, localStart, new Rect(targetX, targetY, targetWidth, targetHeight), 0.4f, 1f, targetScale, null);

//         // Плавное появление
//         clone.experimental.animation.Start(new StyleValues { opacity = 1f }, 400);
//     }

//     public static void AnimateCardBack(VisualElement overlay)
//     {
//         if (overlay.childCount == 0) return;
//         var clone = overlay[0];
//         if (clone == null) return;

//         AnimateRectAndScale(clone, CurrentRect(clone), lastLocalRect, 0.4f, clone.resolvedStyle.scale.value.x, 1f,
//             () =>
//             {
//                 overlay.Clear();
//                 overlay.RemoveFromClassList("overlay-active");
//                 overlay.AddToClassList("overlay-none");
//             }
//         );

//         // Плавное исчезновение
//         clone.experimental.animation.Start(new StyleValues { opacity = 0f }, 400);
//     }

//     private static void AnimateRectAndScale(VisualElement ve, Rect from, Rect to, float duration, float fromScale, float toScale, Action onComplete)
//     {
//         float t = 0f;
//         ve.schedule.Execute(() =>
//         {
//             t += Time.deltaTime;
//             float k = Mathf.Clamp01(t / duration);

//             ve.style.left = Mathf.Lerp(from.x, to.x, k);
//             ve.style.top = Mathf.Lerp(from.y, to.y, k);
//             ve.style.width = Mathf.Lerp(from.width, to.width, k);
//             ve.style.height = Mathf.Lerp(from.height, to.height, k);

//             float s = Mathf.Lerp(fromScale, toScale, k);
//             ve.style.scale = new Scale(new Vector3(s, s, 1f));

//             if (k >= 1f) onComplete?.Invoke();
//         }).Every(16).Until(() => t >= duration);
//     }

//     private static Rect CurrentRect(VisualElement ve)
//     {
//         return new Rect(
//             ve.resolvedStyle.left,
//             ve.resolvedStyle.top,
//             ve.resolvedStyle.width,
//             ve.resolvedStyle.height
//         );
//     }
// }




// using UnityEngine;
// using UnityEngine.UIElements;
// using System;
// using UnityEngine.UIElements.Experimental;

// public static class CardScaleAnimator
// {
//     private static Rect lastLocalRect;

//     public static void AnimateCardFromListToOverlay(VisualElement sourceCard, VisualElement clone, VisualElement overlay, Vector2 clickPosition)
//     {
//         // Проверка: нажато ли в центре карточки
//         if (!IsClickInCenter(sourceCard, clickPosition))
//             return;

//         // 1) Исходный прямоугольник карточки
//         var worldRect = sourceCard.worldBound;
//         var topLeftLocal = overlay.WorldToLocal(new Vector2(worldRect.x, worldRect.y));
//         Rect localStart = new Rect(topLeftLocal.x, topLeftLocal.y, worldRect.width, worldRect.height);
//         lastLocalRect = localStart;

//         // 2) Подготовка overlay
//         overlay.Clear();
//         overlay.RemoveFromClassList("overlay-none");
//         overlay.AddToClassList("overlay-active");

//         // 3) Клон карточки
//         overlay.Add(clone);

//         // 4) Начальные стили
//         clone.style.position = Position.Absolute;
//         clone.style.left = localStart.x;
//         clone.style.top = localStart.y;
//         clone.style.width = localStart.width;
//         clone.style.height = localStart.height;
//         clone.style.transformOrigin = new TransformOrigin(50, 50, 0);
//         clone.style.scale = new Scale(Vector3.one);
//         clone.style.opacity = 0f;

//         // 5) Цель — центр окна
//         var rootBounds = overlay.panel.visualTree.worldBound;
//         var rootCenter = rootBounds.center;
//         var localCenter = overlay.WorldToLocal(rootCenter);

//         float targetScale = 1.5f;
//         float targetWidth = localStart.width * targetScale;
//         float targetHeight = localStart.height * targetScale;
//         float targetX = localCenter.x - targetWidth / 2f;
//         float targetY = localCenter.y - targetHeight / 2f;

//         // 6) Анимация
//         AnimateTransform(clone, localStart, new Rect(targetX, targetY, targetWidth, targetHeight), 1f, targetScale, 0.4f);

//         // Плавное появление
//         clone.experimental.animation.Start(new StyleValues { opacity = 1f }, 400);
//     }

//     public static void AnimateCardBack(VisualElement overlay)
//     {
//         if (overlay.childCount == 0) return;
//         var clone = overlay[0];
//         if (clone == null) return;

//         AnimateTransform(clone, CurrentRect(clone), lastLocalRect, clone.resolvedStyle.scale.value.x, 1f, 0.4f, () =>
//         {
//             overlay.Clear();
//             overlay.RemoveFromClassList("overlay-active");
//             overlay.AddToClassList("overlay-none");
//         });

//         clone.experimental.animation.Start(new StyleValues { opacity = 0f }, 400);
//     }

//     private static void AnimateTransform(
//         VisualElement element,
//         Rect fromRect,
//         Rect toRect,
//         float fromScale,
//         float toScale,
//         float duration,
//         Action onComplete = null)
//     {
//         float t = 0f;

//         element.schedule.Execute(() =>
//         {
//             t += Time.deltaTime;
//             float k = Mathf.Clamp01(t / duration);

//             element.style.left = Mathf.Lerp(fromRect.x, toRect.x, k);
//             element.style.top = Mathf.Lerp(fromRect.y, toRect.y, k);
//             element.style.width = Mathf.Lerp(fromRect.width, toRect.width, k);
//             element.style.height = Mathf.Lerp(fromRect.height, toRect.height, k);

//             float scale = Mathf.Lerp(fromScale, toScale, k);
//             element.style.scale = new Scale(new Vector3(scale, scale, 1f));

//             if (k >= 1f) onComplete?.Invoke();
//         }).Every(16).Until(() => t >= duration);
//     }

//     private static Rect CurrentRect(VisualElement ve)
//     {
//         return new Rect(
//             ve.resolvedStyle.left,
//             ve.resolvedStyle.top,
//             ve.resolvedStyle.width,
//             ve.resolvedStyle.height
//         );
//     }

//     private static bool IsClickInCenter(VisualElement card, Vector2 clickPosition)
//     {
//         var bounds = card.worldBound;

//         float centerX = bounds.x + bounds.width / 2f;
//         float centerY = bounds.y + bounds.height / 2f;

//         float centerWidth = bounds.width * 0.4f;
//         float centerHeight = bounds.height * 0.4f;

//         Rect centerRect = new Rect(
//             centerX - centerWidth / 2f,
//             centerY - centerHeight / 2f,
//             centerWidth,
//             centerHeight
//         );

//         return centerRect.Contains(clickPosition);
//     }
// }


using UnityEngine;
using UnityEngine.UIElements;
using System;
using UnityEngine.UIElements.Experimental;

public static class CardScaleAnimator
{
    private static Rect lastLocalRect;
    private static bool isAnimating = false;
    private static readonly AnimationCurve easeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    public static void AnimateCardFromListToOverlay(VisualElement sourceCard, VisualElement clone, VisualElement overlay, Vector2 clickPosition, float targetScale = 1f)
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
        overlay.Add(clone);

        clone.style.position = Position.Absolute;
        clone.style.left = localStart.x;
        clone.style.top = localStart.y;
        clone.style.width = localStart.width;
        clone.style.height = localStart.height;
        clone.style.transformOrigin = new TransformOrigin(50, 50, 0);
        clone.style.scale = new Scale(Vector3.one);
        clone.style.opacity = 0f;

        var rootBounds = overlay.panel.visualTree.worldBound;
        var rootCenter = rootBounds.center;
        var localCenter = overlay.WorldToLocal(rootCenter);

        float targetWidth = localStart.width * targetScale;
        float targetHeight = localStart.height * targetScale;
        float targetX = localCenter.x - targetWidth / 2f;
        float targetY = localCenter.y - targetHeight / 2f;

        AnimateTransform(clone, localStart, new Rect(targetX, targetY, targetWidth, targetHeight), 1f, targetScale, 0.4f, () =>
        {
            isAnimating = false;
        });

        clone.experimental.animation.Start(new StyleValues { opacity = 1f }, 400);
    }

    public static void AnimateCardBack(VisualElement overlay)
    {
        if (isAnimating || overlay.childCount == 0) return;
        var clone = overlay[0];
        if (clone == null) return;

        isAnimating = true;

        AnimateTransform(clone, CurrentRect(clone), lastLocalRect, clone.resolvedStyle.scale.value.x, 1f, 0.4f, () =>
        {
            overlay.Clear();
            overlay.RemoveFromClassList("overlay-active");
            overlay.AddToClassList("overlay-none");
            isAnimating = false;
        });

        clone.experimental.animation.Start(new StyleValues { opacity = 0f }, 400);
    }

    private static void AnimateTransform(VisualElement element, Rect fromRect, Rect toRect, float fromScale, float toScale, float duration, Action onComplete = null)
    {
        float t = 0f;

        element.schedule.Execute(() =>
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / duration);
            float easedK = easeCurve.Evaluate(k);

            element.style.left = Mathf.Lerp(fromRect.x, toRect.x, easedK);
            element.style.top = Mathf.Lerp(fromRect.y, toRect.y, easedK);
            element.style.width = Mathf.Lerp(fromRect.width, toRect.width, easedK);
            element.style.height = Mathf.Lerp(fromRect.height, toRect.height, easedK);

            float scale = Mathf.Lerp(fromScale, toScale, easedK);
            element.style.scale = new Scale(new Vector3(scale, scale, 1f));

            if (k >= 1f) onComplete?.Invoke();
        }).Every(16).Until(() => t >= duration);
    }

    private static Rect CurrentRect(VisualElement visualElement)
    {
        return new Rect(visualElement.resolvedStyle.left, visualElement.resolvedStyle.top,
            visualElement.resolvedStyle.width, visualElement.resolvedStyle.height);
    }

    private static bool IsClickInCenter(VisualElement card, Vector2 clickPosition)
    {
        var bounds = card.worldBound;

        float centerX = bounds.x + bounds.width / 2f;
        float centerY = bounds.y + bounds.height / 2f;

        float centerWidth = bounds.width * 0.4f;
        float centerHeight = bounds.height * 0.4f;

        Rect centerRect = new Rect(
            centerX - centerWidth / 2f,
            centerY - centerHeight / 2f,
            centerWidth,
            centerHeight
        );

        return centerRect.Contains(clickPosition);
    }
}

