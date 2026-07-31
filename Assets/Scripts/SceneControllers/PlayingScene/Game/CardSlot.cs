using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

public enum FieldSlotType
{
    SelfHandSlot,
    SelfFieldSlot,
    SelfResetStack,
    EnemyHandSlot,
    EnemyFieldSlot,
    EnemyResetStack,
    None
}

public enum FieldSlotIndex
{
    FirstSlot,
    SecondSlot,
    ThirdSlot,
    FourthSlot,
    FifthSlot
}

[Serializable]
public class CardLocalSettings
{
    public Vector3 cardLocalPosition;
    public Vector3 cardLocalRotation;
    public Vector3 cardLocalScale;

    public Vector3 colliderCenter;
    public Vector3 colliderSize;
}

public class CardSlot : MonoBehaviour
{
    [SerializeField] private MeshRenderer slotMeshRenderer;
    [SerializeField] private CardLocalSettings cardLocalSettings;

    [Header("Info")]
    public FieldSlotType type;
    public FieldSlotIndex indexSlotInField;
    public Transform zoomPoint;

    public GameObject CurrentCard { get; set; }

    public void AddCardInStartGame(BattleCardController cardController)
    {
        CurrentCard = cardController.BattleCardView.CardRoot;
        CurrentCard.transform.SetParent(transform);

        CurrentCard.GetComponent<BoxCollider>().center = cardLocalSettings.colliderCenter;
        CurrentCard.GetComponent<BoxCollider>().size = cardLocalSettings.colliderSize;

        float depthOffset = 8.0f;
        float startScaleMultiplier = 0.4f;
        float duration = 0.4f;

        Vector3 targetPos = cardLocalSettings.cardLocalPosition;
        Vector3 targetRot = cardLocalSettings.cardLocalRotation;
        Vector3 targetScale = cardLocalSettings.cardLocalScale;

        if (ConnectionConfig.IsSpectator && type == FieldSlotType.SelfHandSlot)
        {
            targetRot.y = 90f;
            targetScale.y = targetScale.z = 75f;
        }

        Vector3 startPos = targetPos;

        if (type == FieldSlotType.SelfHandSlot)
            startPos.y += depthOffset;
        else if (type == FieldSlotType.EnemyHandSlot)
            startPos.y -= depthOffset;

        CurrentCard.transform.localPosition = startPos;
        CurrentCard.transform.localEulerAngles = targetRot;
        CurrentCard.transform.localScale = targetScale * startScaleMultiplier;

        CurrentCard.SetActive(true);

        CurrentCard.transform.DOLocalMove(targetPos, duration).SetEase(Ease.OutCubic);
        CurrentCard.transform.DOScale(targetScale, duration).SetEase(Ease.OutCubic);
    }

    public IEnumerator PlaceCard(BattleCardController cardController)
    {
        CurrentCard = cardController.BattleCardView.CardRoot;
        CurrentCard.transform.SetParent(transform);

        CurrentCard.GetComponent<BoxCollider>().center = cardLocalSettings.colliderCenter;
        CurrentCard.GetComponent<BoxCollider>().size = cardLocalSettings.colliderSize;

        Sequence sequence = DOTween.Sequence();
        sequence.Join(CurrentCard.transform.DOLocalMove(cardLocalSettings.cardLocalPosition, 0.25f).SetEase(Ease.OutQuad));
        sequence.Join(CurrentCard.transform.DOLocalRotate(cardLocalSettings.cardLocalRotation, 0.25f).SetEase(Ease.OutQuad));
        sequence.Join(CurrentCard.transform.DOScale(cardLocalSettings.cardLocalScale, 0.25f).SetEase(Ease.OutQuad));
        yield return sequence.WaitForCompletion();
    }

    public IEnumerator PlaceCardWithMoveAndFlip(BattleCardController cardController)
    {
        CurrentCard = cardController.BattleCardView.CardRoot;
        CurrentCard.transform.SetParent(transform);

        CurrentCard.GetComponent<BoxCollider>().center = cardLocalSettings.colliderCenter;
        CurrentCard.GetComponent<BoxCollider>().size = cardLocalSettings.colliderSize;

        Vector3 middlePosition = new(0, 0.12f, -3f);

        Sequence sequence = DOTween.Sequence();
        sequence.Append(CurrentCard.transform.DOLocalMove(middlePosition, 0.6f).SetEase(Ease.OutQuad));
        sequence.Join(CurrentCard.transform.DOScale(cardLocalSettings.cardLocalScale, 0.6f).SetEase(Ease.OutQuad));
        sequence.Append(CurrentCard.transform.DOLocalRotate(cardLocalSettings.cardLocalRotation, 0.8f).SetEase(Ease.OutQuad));
        sequence.Append(CurrentCard.transform.DOLocalMove(cardLocalSettings.cardLocalPosition, 0.4f).SetEase(Ease.OutQuad));
        yield return sequence.WaitForCompletion();
    }

    public IEnumerator PlaceCardWithMove(BattleCardController cardController)
    {
        CurrentCard = cardController.BattleCardView.CardRoot;
        CurrentCard.transform.SetParent(transform);

        CurrentCard.GetComponent<BoxCollider>().center = cardLocalSettings.colliderCenter;
        CurrentCard.GetComponent<BoxCollider>().size = cardLocalSettings.colliderSize;

        Vector3 middlePosition = new(0, 0.12f, -3f);

        Sequence sequence = DOTween.Sequence();
        sequence.Append(CurrentCard.transform.DOLocalMove(middlePosition, 0.6f).SetEase(Ease.OutQuad));
        sequence.Join(CurrentCard.transform.DOScale(cardLocalSettings.cardLocalScale, 0.6f).SetEase(Ease.OutQuad));
        sequence.Append(CurrentCard.transform.DOLocalMove(cardLocalSettings.cardLocalPosition, 0.4f).SetEase(Ease.OutQuad));
        yield return sequence.WaitForCompletion();
    }

    public IEnumerator SwapWith(CardSlot otherSlot)
    {
        if (CurrentCard != null && otherSlot.CurrentCard != null)
        {
            GameObject thisCard = CurrentCard;
            GameObject otherCard = otherSlot.CurrentCard;

            CurrentCard = otherCard;
            otherSlot.CurrentCard = thisCard;

            thisCard.transform.SetParent(otherSlot.transform);
            otherCard.transform.SetParent(transform);

            Vector3 middlePositionOtherCard = new(0, 0.05f, -0.1f);

            Sequence sequence = DOTween.Sequence();

            sequence.Append(otherCard.transform.DOLocalMove(middlePositionOtherCard, 0.35f).SetEase(Ease.OutQuad));

            sequence.Append(thisCard.transform.DOLocalMove(otherSlot.cardLocalSettings.cardLocalPosition, 0.25f).SetEase(Ease.OutQuad));
            sequence.Join(thisCard.transform.DOLocalRotate(otherSlot.cardLocalSettings.cardLocalRotation, 0.25f).SetEase(Ease.OutQuad));
            sequence.Join(thisCard.transform.DOScale(otherSlot.cardLocalSettings.cardLocalScale, 0.25f).SetEase(Ease.OutQuad));

            sequence.Join(otherCard.transform.DOLocalMove(cardLocalSettings.cardLocalPosition, 0.25f).SetEase(Ease.OutQuad));
            sequence.Join(otherCard.transform.DOLocalRotate(cardLocalSettings.cardLocalRotation, 0.25f).SetEase(Ease.OutQuad));
            sequence.Join(otherCard.transform.DOScale(cardLocalSettings.cardLocalScale, 0.25f).SetEase(Ease.OutQuad));

            yield return sequence.WaitForCompletion();
        }
    }

    public void Highlight(bool state) => slotMeshRenderer.materials[2].color = state ? new Color32(255, 7, 137, 255) : new Color32(192, 255, 244, 255);

    public bool IsEmpty => CurrentCard == null;
    public void Clear() => CurrentCard = null;
}
