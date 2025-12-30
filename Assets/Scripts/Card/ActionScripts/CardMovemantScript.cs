using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using System.Linq;

public class CardMovemantScript : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Transform DefaultParent { get; set; }
    public Transform DefaultTempCardParent { get; set; }

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform rectTransform;

    private GameObject tempCard;
    private Canvas canvas;
    private Vector3 originalScale;
    private GameManagerScript gameManager;
    private static CardMovemantScript draggingCard = null;
    private bool isDraggable;
    private FieldType prevFieldType;

    public int CardId { get; set; }

    void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        originalScale = rectTransform.localScale;
        tempCard = GameObject.Find("TempSlot");
        gameManager = FindAnyObjectByType<GameManagerScript>();
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
            isDraggable = gameManager.IsMyTurn && ((fieldType == FieldType.SELF_HAND && !gameManager.cardIsThrown) || fieldType == FieldType.SELF_FIELD);
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
            if (!isDraggable && draggingCard != this) return;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, eventData.position,
                canvas.worldCamera, out Vector2 localPoint);

            rectTransform.localPosition = localPoint;

            if (tempCard.transform.parent != DefaultTempCardParent)
                tempCard.transform.SetParent(DefaultTempCardParent);

            CheckPosition();
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDraggable && draggingCard != this) return;

        transform.SetParent(DefaultParent, true);
        canvasGroup.blocksRaycasts = true;

        transform.SetSiblingIndex(tempCard.transform.GetSiblingIndex());

        tempCard.transform.SetParent(canvas.transform);
        tempCard.transform.localPosition = new Vector3(2600, 0);

        rectTransform.DOScale(originalScale, 0.15f);

        if (DefaultParent.TryGetComponent<DropPlaceScript>(out var dropPlace))
        {
            if (prevFieldType != dropPlace.type)
            {
                int siblingIndex = transform.GetSiblingIndex();
                gameManager.RequestPlayCard(CardId, siblingIndex);

                BattleCardController cardController = gameManager.CurrentGame.PlayerHandListController.CardControllers.FirstOrDefault(c => c.CardModel.id == CardId);
                gameManager.CurrentGame.PlayerHandListController.CardControllers.Remove(cardController);
                gameManager.CurrentGame.PlayerFieldListController.CardControllers.Add(cardController);
                gameManager.cardIsThrown = true;
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

