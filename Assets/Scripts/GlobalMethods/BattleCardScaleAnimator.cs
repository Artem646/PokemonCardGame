using UnityEngine;
using DG.Tweening;
using UGUI = UnityEngine.UI;

public static class BattleCardScaleAnimator
{
    private static RectTransform clone;
    private static Vector2 lastLocalPos;
    private static bool isAnimating = false;
    private const float TARGET_SCALE = 2.5f;
    private const float DURATION = 0.5f;

    public static void ShowCard(RectTransform sourceCard, RectTransform cloneCard, GameObject overlay, Canvas canvas)
    {
        if (isAnimating) return;
        isAnimating = true;

        RectTransform overlayRectTransform = overlay.GetComponent<RectTransform>();

        Vector3 worldPos = sourceCard.position;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            overlayRectTransform,
            RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, worldPos),
            canvas.worldCamera,
            out Vector2 localPos
        );

        clone = cloneCard;
        cloneCard.transform.SetParent(overlayRectTransform, false);
        clone.anchoredPosition = localPos;
        clone.sizeDelta = sourceCard.sizeDelta;
        clone.localScale = Vector3.one;

        lastLocalPos = localPos;

        Vector2 targetPos = Vector2.zero;

        overlay.SetActive(true);

        Sequence seq = DOTween.Sequence();
        seq.Join(clone.DOAnchorPos(targetPos, DURATION).SetEase(Ease.InQuad));
        seq.Join(clone.DOScale(TARGET_SCALE, DURATION).SetEase(Ease.InQuad));
        seq.Join(overlay.GetComponent<UGUI.Image>().DOFade(0.6f, DURATION));
        seq.OnComplete(() =>
        {
            CardStateManager.RaiseCard();
            isAnimating = false;
        });
    }

    public static void HideCard(GameObject overlay)
    {
        RectTransform overlayRectTransform = overlay.GetComponent<RectTransform>();

        if (isAnimating || overlayRectTransform.childCount == 0) return;
        if (clone == null) return;

        isAnimating = true;

        Sequence seq = DOTween.Sequence();
        seq.Join(clone.DOAnchorPos(lastLocalPos, DURATION).SetEase(Ease.InQuad));
        seq.Join(clone.DOScale(1f, DURATION).SetEase(Ease.InQuad));
        seq.Join(clone.DORotate(new Vector3(0, 0, 10f), DURATION * 0.5f).SetLoops(2, LoopType.Yoyo));
        seq.Join(overlayRectTransform.GetComponent<UGUI.Image>().DOFade(0f, DURATION));
        seq.OnComplete(() =>
        {
            overlay.SetActive(false);
            Object.Destroy(clone.gameObject);
            CardStateManager.ResetCardState();
            isAnimating = false;
        });
    }
}

