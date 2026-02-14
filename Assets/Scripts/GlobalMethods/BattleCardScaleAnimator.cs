using UnityEngine;
using DG.Tweening;
using UGUI = UnityEngine.UI;

public static class BattleCardScaleAnimator
{
    private static RectTransform clone;
    private static Vector2 lastLocalPos;
    private static Sequence currentSequence;

    private const float TARGET_SCALE = 3.7f;
    private const float DURATION = 0.45f;

    public static void ShowCard(RectTransform sourceCard, RectTransform cloneCard, GameObject overlay, Canvas canvas)
    {
        if (currentSequence != null) return;

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

        currentSequence = DOTween.Sequence();

        currentSequence.Join(clone.DOAnchorPos(targetPos, DURATION).SetEase(Ease.InQuad));
        currentSequence.Join(clone.DOScale(TARGET_SCALE, DURATION).SetEase(Ease.InQuad));
        currentSequence.Join(overlay.GetComponent<UGUI.Image>().DOFade(0.6f, DURATION));

        currentSequence.OnComplete(() =>
        {
            currentSequence = null;
        });
    }

    public static void HideCard(GameObject overlay)
    {
        RectTransform overlayRectTransform = overlay.GetComponent<RectTransform>();

        if (overlayRectTransform.childCount == 0 || clone == null || currentSequence != null) return;

        currentSequence = DOTween.Sequence();

        currentSequence.Join(clone.DOAnchorPos(lastLocalPos, DURATION).SetEase(Ease.InQuad));
        currentSequence.Join(clone.DOScale(1f, DURATION).SetEase(Ease.InQuad));
        currentSequence.Join(clone.DORotate(new Vector3(0, 0, 10f), DURATION * 0.5f).SetLoops(2, LoopType.Yoyo));
        currentSequence.Join(overlayRectTransform.GetComponent<UGUI.Image>().DOFade(0f, DURATION));

        currentSequence.OnComplete(() =>
        {
            overlay.SetActive(false);
            CardStateInteractionManager.EndRaise();
            Object.Destroy(clone.gameObject);
            currentSequence = null;
        });
    }
}

