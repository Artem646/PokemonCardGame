using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(RectTransform))]
public class CardMovemantScript : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Transform DefaultParent { get; set; }
    public Transform DefaultTempCardParent { get; set; }
    private GameObject tempCard;

    private Canvas canvas;
    private CanvasGroup canvasGroup;

    private RectTransform rectTransform;
    private Vector3 originalScale;

    private static CardMovemantScript draggingCard = null;

    public bool isDraggable;

    private FieldType prevFieldType;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();

        originalScale = rectTransform.localScale;

        tempCard = GameObject.Find("TempSlot");
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!CardStateManager.IsCardRaised)
        {
            if (draggingCard != null && draggingCard != this) return;

            DefaultParent = DefaultTempCardParent = transform.parent;

            if (DefaultParent.TryGetComponent<DropPlaceScript>(out var originDrop))
                prevFieldType = originDrop.type;
            else
                prevFieldType = FieldType.NONE;

            FieldType fieldType = DefaultParent.GetComponent<DropPlaceScript>().type;
            isDraggable = fieldType == FieldType.SELF_HAND || fieldType == FieldType.SELF_FIELD;
            if (isDraggable)
            {
                draggingCard = this;

                tempCard.transform.SetParent(DefaultTempCardParent);
                tempCard.transform.SetSiblingIndex(transform.GetSiblingIndex());

                transform.SetParent(canvas.transform, true);
                canvasGroup.blocksRaycasts = false;

                rectTransform.DOScale(originalScale * 1.1f, 0.15f);
            }
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!CardStateManager.IsCardRaised)
        {
            if (!isDraggable) return;
            if (draggingCard != this) return;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                eventData.position,
                canvas.worldCamera,
                out Vector2 localPoint);

            rectTransform.localPosition = localPoint;

            if (tempCard.transform.parent != DefaultTempCardParent)
                tempCard.transform.SetParent(DefaultTempCardParent);

            CheckPosition();
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDraggable) return;
        if (draggingCard != this) return;

        transform.SetParent(DefaultParent, true);
        canvasGroup.blocksRaycasts = true;

        transform.SetSiblingIndex(tempCard.transform.GetSiblingIndex());

        tempCard.transform.SetParent(canvas.transform);
        tempCard.transform.localPosition = new Vector3(2600, 0);

        rectTransform.DOScale(originalScale, 0.15f);

        if (DefaultParent.TryGetComponent<DropPlaceScript>(out var dropPlace))
        {
            if (TryGetComponent<CardFlipScript>(out var cardFlip))
            {
                if (prevFieldType != dropPlace.type)
                {
                    if (dropPlace.type == FieldType.SELF_FIELD)
                        cardFlip.FlipToFaceDown();
                    else if (dropPlace.type == FieldType.SELF_HAND)
                        cardFlip.FlipToFaceUp();
                }
            }
        }

        draggingCard = null;
    }

    void CheckPosition()
    {
        int newIndex = DefaultTempCardParent.childCount;

        for (int i = 0; i < DefaultTempCardParent.childCount; i++)
        {
            if (transform.position.x < DefaultTempCardParent.GetChild(i).position.x)
            {
                newIndex = i;

                if (tempCard.transform.GetSiblingIndex() < newIndex)
                    newIndex--;

                break;
            }
        }

        tempCard.transform.SetSiblingIndex(newIndex);
    }
}

