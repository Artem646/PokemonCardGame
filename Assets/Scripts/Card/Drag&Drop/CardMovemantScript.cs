// using UnityEngine;
// using UnityEngine.EventSystems;
// using DG.Tweening;

// [RequireComponent(typeof(CanvasGroup))]
// public class CardMovemantScript : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
// {
//     public Transform DefaultParent { get; set; }
//     public Transform DefaultTempCardParent { get; set; }
//     private GameObject tempCard;

//     private Canvas canvas;
//     private CanvasGroup canvasGroup;

//     private RectTransform rectTransform;
//     private Vector3 originalScale;
//     private Vector3 originalPosition;

//     private bool isRaised = false;
//     private static int raisedCardsCount = 0;
//     private static CardMovemantScript draggingCard = null;

//     private readonly float raiseAmount = 50f;
//     private readonly float scaleMultiplier = 1.4f;
//     private readonly float duration = 0.5f;
//     private readonly Ease raiseEase = Ease.OutBack;

//     void Awake()
//     {
//         rectTransform = GetComponent<RectTransform>();
//         canvasGroup = GetComponent<CanvasGroup>();
//         canvas = GetComponentInParent<Canvas>();

//         originalScale = rectTransform.localScale;
//         originalPosition = rectTransform.localPosition;

//         tempCard = GameObject.Find("TempSlot");
//     }

//     public void OnBeginDrag(PointerEventData eventData)
//     {
//         if (raisedCardsCount > 0) return;
//         if (draggingCard != null && draggingCard != this) return;

//         draggingCard = this;

//         DefaultParent = transform.parent;
//         DefaultTempCardParent = DefaultParent;

//         tempCard.transform.SetParent(DefaultTempCardParent);
//         tempCard.transform.SetSiblingIndex(transform.GetSiblingIndex());

//         transform.SetParent(canvas.transform, true);
//         canvasGroup.blocksRaycasts = false;

//         rectTransform.DOScale(originalScale * 1.1f, 0.15f);
//     }

//     public void OnDrag(PointerEventData eventData)
//     {
//         if (draggingCard != this) return;
//         if (raisedCardsCount > 0) return;

//         RectTransformUtility.ScreenPointToLocalPointInRectangle(
//             canvas.transform as RectTransform,
//             eventData.position,
//             canvas.worldCamera,
//             out Vector2 localPoint);

//         rectTransform.localPosition = localPoint;

//         if (tempCard.transform.parent != DefaultTempCardParent)
//             tempCard.transform.SetParent(DefaultTempCardParent);

//         CheckPosition();
//     }

//     public void OnEndDrag(PointerEventData eventData)
//     {
//         if (draggingCard != this) return;

//         transform.SetParent(DefaultParent, true);
//         canvasGroup.blocksRaycasts = true;

//         transform.SetSiblingIndex(tempCard.transform.GetSiblingIndex());

//         tempCard.transform.SetParent(canvas.transform);
//         tempCard.transform.localPosition = new Vector3(2600, 0);

//         rectTransform.DOScale(originalScale, 0.15f);

//         draggingCard = null;
//     }

//     void CheckPosition()
//     {
//         int newIndex = DefaultTempCardParent.childCount;

//         for (int i = 0; i < DefaultTempCardParent.childCount; i++)
//         {
//             if (transform.position.x < DefaultTempCardParent.GetChild(i).position.x)
//             {
//                 newIndex = i;

//                 if (tempCard.transform.GetSiblingIndex() < newIndex)
//                     newIndex--;

//                 break;
//             }
//         }

//         tempCard.transform.SetSiblingIndex(newIndex);
//     }

//     public void OnPointerClick(PointerEventData eventData)
//     {
//         if (isRaised) LowerCard();
//         else
//         {
//             originalPosition = rectTransform.localPosition;
//             RaiseCard();
//         }
//     }

//     private void RaiseCard()
//     {
//         rectTransform.localScale = originalScale;

//         rectTransform
//             .DOScale(originalScale * scaleMultiplier, duration)
//             .SetEase(raiseEase);

//         rectTransform
//             .DOLocalMove(originalPosition + Vector3.up * raiseAmount, duration)
//             .SetEase(raiseEase);

//         if (!isRaised)
//         {
//             isRaised = true;
//             raisedCardsCount++;
//         }
//     }

//     private void LowerCard()
//     {
//         rectTransform
//             .DOScale(originalScale, duration)
//             .SetEase(raiseEase);

//         rectTransform
//             .DOLocalMove(originalPosition, duration)
//             .SetEase(raiseEase);

//         if (isRaised)
//         {
//             isRaised = false;
//             raisedCardsCount = Mathf.Max(0, raisedCardsCount - 1);
//         }
//     }
// }

// using UnityEngine;
// using UnityEngine.EventSystems;
// using DG.Tweening;

// [RequireComponent(typeof(CanvasGroup))]
// public class CardMovemantScript : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
// {
//     public Transform DefaultParent { get; set; }
//     public Transform DefaultTempCardParent { get; set; }
//     private GameObject tempCard;

//     private Canvas canvas;
//     private CanvasGroup canvasGroup;

//     private RectTransform rectTransform;
//     private Vector3 originalScale;
//     private Vector3 originalPosition;

//     private bool isRaised = false;
//     private static CardMovemantScript raisedCard = null;   // ссылка на поднятую карту
//     private static CardMovemantScript draggingCard = null; // ссылка на перетаскиваемую карту

//     private readonly float raiseAmount = 50f;
//     private readonly float scaleMultiplier = 1.4f;
//     private readonly float duration = 0.5f;
//     private readonly Ease raiseEase = Ease.OutBack;

//     void Awake()
//     {
//         rectTransform = GetComponent<RectTransform>();
//         canvasGroup = GetComponent<CanvasGroup>();
//         canvas = GetComponentInParent<Canvas>();

//         originalScale = rectTransform.localScale;
//         originalPosition = rectTransform.localPosition;

//         tempCard = GameObject.Find("TempSlot");
//     }

//     public void OnBeginDrag(PointerEventData eventData)
//     {
//         // запрещаем перетаскивание, если какая-то карта поднята
//         if (raisedCard != null) return;
//         if (draggingCard != null && draggingCard != this) return;

//         draggingCard = this;

//         DefaultParent = transform.parent;
//         DefaultTempCardParent = DefaultParent;

//         tempCard.transform.SetParent(DefaultTempCardParent);
//         tempCard.transform.SetSiblingIndex(transform.GetSiblingIndex());

//         transform.SetParent(canvas.transform, true);
//         canvasGroup.blocksRaycasts = false;

//         rectTransform.DOScale(originalScale * 1.1f, 0.15f);
//     }

//     public void OnDrag(PointerEventData eventData)
//     {
//         if (draggingCard != this) return;
//         if (raisedCard != null) return; // блокируем, если карта поднята

//         RectTransformUtility.ScreenPointToLocalPointInRectangle(
//             canvas.transform as RectTransform,
//             eventData.position,
//             canvas.worldCamera,
//             out Vector2 localPoint);

//         rectTransform.localPosition = localPoint;

//         if (tempCard.transform.parent != DefaultTempCardParent)
//             tempCard.transform.SetParent(DefaultTempCardParent);

//         CheckPosition();
//     }

//     public void OnEndDrag(PointerEventData eventData)
//     {
//         if (draggingCard != this) return;

//         transform.SetParent(DefaultParent, true);
//         canvasGroup.blocksRaycasts = true;

//         transform.SetSiblingIndex(tempCard.transform.GetSiblingIndex());

//         tempCard.transform.SetParent(canvas.transform);
//         tempCard.transform.localPosition = new Vector3(2600, 0);

//         rectTransform.DOScale(originalScale, 0.15f);

//         draggingCard = null;
//     }

//     void CheckPosition()
//     {
//         int newIndex = DefaultTempCardParent.childCount;

//         for (int i = 0; i < DefaultTempCardParent.childCount; i++)
//         {
//             if (transform.position.x < DefaultTempCardParent.GetChild(i).position.x)
//             {
//                 newIndex = i;

//                 if (tempCard.transform.GetSiblingIndex() < newIndex)
//                     newIndex--;

//                 break;
//             }
//         }

//         tempCard.transform.SetSiblingIndex(newIndex);
//     }

//     public void OnPointerClick(PointerEventData eventData)
//     {
//         if (isRaised)
//         {
//             LowerCard();
//             raisedCard = null;
//         }
//         else
//         {
//             // если уже есть другая поднятая карта — не даём поднять
//             if (raisedCard != null) return;

//             originalPosition = rectTransform.localPosition;
//             RaiseCard();
//             raisedCard = this;
//         }
//     }

//     private void RaiseCard()
//     {
//         rectTransform.localScale = originalScale;

//         rectTransform
//             .DOScale(originalScale * scaleMultiplier, duration)
//             .SetEase(raiseEase);

//         rectTransform
//             .DOLocalMove(originalPosition + Vector3.up * raiseAmount, duration)
//             .SetEase(raiseEase);

//         isRaised = true;
//     }

//     private void LowerCard()
//     {
//         rectTransform
//             .DOScale(originalScale, duration)
//             .SetEase(raiseEase);

//         rectTransform
//             .DOLocalMove(originalPosition, duration)
//             .SetEase(raiseEase);

//         isRaised = false;
//     }
// }


using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class CardMovemantScript : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Transform DefaultParent { get; set; }
    public Transform DefaultTempCardParent { get; set; }
    private GameObject tempCard;

    private Canvas canvas;
    private CanvasGroup canvasGroup;

    private RectTransform rectTransform;
    private Vector3 originalScale;
    private Vector3 originalPosition;

    // private bool isRaised = false;
    // private static CardMovemantScript raisedCard = null;
    private static CardMovemantScript draggingCard = null;

    // private readonly float scaleMultiplier = 1.6f;
    // private readonly float duration = 0.4f;
    // private readonly Ease easeShow = Ease.OutQuad;
    // private readonly Ease easeHidePos = Ease.InQuad;
    // private readonly Ease easeHideScale = Ease.InBack;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();

        originalScale = rectTransform.localScale;
        originalPosition = rectTransform.localPosition;

        tempCard = GameObject.Find("TempSlot");
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (CardStateManager.IsCardRaised) return;

        if (draggingCard != null && draggingCard != this) return;

        draggingCard = this;

        DefaultParent = transform.parent;
        DefaultTempCardParent = DefaultParent;

        tempCard.transform.SetParent(DefaultTempCardParent);
        tempCard.transform.SetSiblingIndex(transform.GetSiblingIndex());

        transform.SetParent(canvas.transform, true);
        canvasGroup.blocksRaycasts = false;

        rectTransform.DOScale(originalScale * 1.1f, 0.15f);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (draggingCard != this) return;
        if (CardStateManager.IsCardRaised) return;

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

    public void OnEndDrag(PointerEventData eventData)
    {
        if (draggingCard != this) return;

        transform.SetParent(DefaultParent, true);
        canvasGroup.blocksRaycasts = true;

        transform.SetSiblingIndex(tempCard.transform.GetSiblingIndex());

        tempCard.transform.SetParent(canvas.transform);
        tempCard.transform.localPosition = new Vector3(2600, 0);

        rectTransform.DOScale(originalScale, 0.15f);

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

    // public void OnPointerClick(PointerEventData eventData)
    // {
    //     if ((object)CardStateManager.RaisedCardName == )
    //     {
    //         CardStateManager.ClearRaisedCard();
    //         // LowerCard();
    //         // raisedCard = null;
    //     }
    //     else
    //     {
    //         // if (raisedCard != null) return;

    //         // originalPosition = rectTransform.localPosition;
    //         // RaiseCard();
    //         // raisedCard = this;

    //         if (CardStateManager.HasRaisedCard) return;
    //         originalPosition = rectTransform.localPosition;
    //         CardStateManager.SetRaisedCard(this);
    //     }
    // }

    // private void RaiseCard()
    // {
    //     rectTransform.localScale = originalScale;

    //     Vector3 center = (canvas.transform as RectTransform).rect.center;

    //     rectTransform
    //         .DOScale(originalScale * scaleMultiplier, duration)
    //         .SetEase(Ease.OutBack);

    //     rectTransform
    //         .DOLocalMove(center, duration)
    //         .SetEase(easeShow);

    //     rectTransform
    //         .DORotate(Vector3.zero, duration);

    //     if (overlayImage != null)
    //     {
    //         overlayImage.gameObject.SetActive(true);
    //         overlayImage.DOFade(0.6f, duration);
    //     }

    //     isRaised = true;
    // }

    // private void LowerCard()
    // {
    //     rectTransform
    //         .DOScale(originalScale, duration)
    //         .SetEase(easeHideScale);

    //     rectTransform
    //         .DOLocalMove(originalPosition, duration)
    //         .SetEase(easeHidePos);

    //     rectTransform
    //         .DORotate(new Vector3(0, 0, 10f), duration * 0.5f)
    //         .SetLoops(2, LoopType.Yoyo);

    //     if (overlayImage != null)
    //     {
    //         overlayImage.DOFade(0f, duration)
    //             .OnComplete(() => overlayImage.gameObject.SetActive(false));
    //     }

    //     isRaised = false;
    // }
}

