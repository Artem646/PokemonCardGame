using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public static class BattleCardScaleAnimator
{
    private static Vector3 startLocalPos;
    private static Quaternion startRot;
    private static Vector3 startScale;
    private static GameObject cloneCard;
    private static Sequence currentSequence;

    private const float DURATION = 0.5f;

    public static void ShowCard(GameObject clone, GameObject overlayBackground, Canvas canvas)
    {
        if (currentSequence != null) return;

        cloneCard = clone;
        cloneCard.transform.SetParent(canvas.transform);

        startLocalPos = cloneCard.transform.localPosition;
        startRot = cloneCard.transform.rotation;
        startScale = cloneCard.transform.localScale;

        overlayBackground.SetActive(true);

        currentSequence = DOTween.Sequence();
        currentSequence.Append(cloneCard.transform.DOLocalMove(new Vector3(0, -27f, -240f), DURATION).SetEase(Ease.OutQuad));
        currentSequence.Join(cloneCard.transform.DORotate(new Vector3(-45f, 0, 0), DURATION, RotateMode.WorldAxisAdd).SetEase(Ease.OutQuad));
        currentSequence.Join(cloneCard.transform.DOScale(new Vector3(6000f, 14260f, 13120f), DURATION).SetEase(Ease.OutQuad));
        currentSequence.Insert(DURATION * 0.62f, overlayBackground.GetComponent<Image>().DOFade(0.6f, DURATION).SetEase(Ease.OutQuad));
        currentSequence.OnComplete(() => { currentSequence = null; });
    }

    public static void HideCard(GameObject overlayBackground)
    {
        if (currentSequence != null) return;

        currentSequence = DOTween.Sequence();
        currentSequence.Append(cloneCard.transform.DOLocalMove(startLocalPos, DURATION).SetEase(Ease.OutQuad));
        currentSequence.Join(cloneCard.transform.DORotateQuaternion(startRot, DURATION).SetEase(Ease.OutQuad));
        currentSequence.Join(cloneCard.transform.DOScale(startScale, DURATION).SetEase(Ease.OutQuad));
        currentSequence.Join(overlayBackground.GetComponent<Image>().DOFade(0, DURATION).SetEase(Ease.OutQuad));
        currentSequence.OnComplete(() =>
        {
            overlayBackground.SetActive(false);
            CardStateInteractionManager.EndRaise();
            Object.Destroy(cloneCard);
            currentSequence = null;
        });
    }
}

