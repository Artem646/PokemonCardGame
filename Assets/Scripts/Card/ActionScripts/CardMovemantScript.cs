using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;
using System;

public class CardMovemantScript : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Transform DefaultParent { get; set; }
    public Transform DefaultTempCardParent { get; set; }

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform rectTransform;

    private GameManagerScript gameManager;
    private BotTurnManager botTurnManager;
    private NetworkGameTurnManager networkTurnManager;
    private NetworkGameController networkGameController;

    private Canvas canvas;
    private Vector3 originalScale;
    private GameObject tempCard;

    private FieldType prevFieldType;
    private FieldType currentFieldType;
    private bool isDraggable;

    private GameType currentType = GameTypeConfig.CurrentType;

    private void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        originalScale = rectTransform.localScale;
        tempCard = GameObject.Find("TempSlot");
        gameManager = FindAnyObjectByType<GameManagerScript>();
        botTurnManager = FindAnyObjectByType<BotTurnManager>();
        networkTurnManager = FindAnyObjectByType<NetworkGameTurnManager>();
        networkGameController = FindAnyObjectByType<NetworkGameController>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        DefaultParent = DefaultTempCardParent = transform.parent;

        if (DefaultParent.TryGetComponent<DropPlaceScript>(out var originDrop))
        {
            currentFieldType = originDrop.type;
            prevFieldType = originDrop.type;
        }
        else
        {
            currentFieldType = FieldType.NONE;
            prevFieldType = FieldType.NONE;
        }

        isDraggable = gameManager.IsMyTurn && currentFieldType == FieldType.SELF_HAND
            && (currentType == GameType.Multiplayer ?
                networkTurnManager.CurrentPhase == TurnPhase.PlayCard :
                botTurnManager.CurrentPhase == TurnPhase.PlayCard);

        if (isDraggable && CardStateInteractionManager.TryBeginDrag(this))
        {
            tempCard.transform.SetParent(DefaultTempCardParent);
            tempCard.transform.SetSiblingIndex(transform.GetSiblingIndex());

            transform.SetParent(canvas.transform, true);

            canvasGroup.blocksRaycasts = false;
            rectTransform.DOScale(originalScale * 1.15f, 0.1f);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isDraggable && CardStateInteractionManager.IsDraggingBy(this))
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform, eventData.position,
            canvas.worldCamera, out Vector2 localPoint);

            rectTransform.localPosition = localPoint;

            if (tempCard.transform.parent != DefaultTempCardParent)
                tempCard.transform.SetParent(DefaultTempCardParent);

            UpdateTempCardPosition();
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isDraggable && CardStateInteractionManager.IsDraggingBy(this))
        {
            CardStateInteractionManager.EndDrag();
            canvasGroup.blocksRaycasts = true;
            StartCoroutine(OnEndDragRoutine());
        }
    }

    private IEnumerator OnEndDragRoutine()
    {
        yield return transform.DOMove(tempCard.transform.position, 0.3f).SetEase(Ease.OutQuad).WaitForCompletion();

        transform.SetParent(DefaultParent, true);
        transform.SetSiblingIndex(tempCard.transform.GetSiblingIndex());

        UpdateCardIndexInHandListController(transform.GetSiblingIndex());

        tempCard.transform.SetParent(canvas.transform);
        tempCard.transform.localPosition = new Vector3(2600, 0);

        rectTransform.DOScale(originalScale, 0.1f);

        if (DefaultParent.TryGetComponent<DropPlaceScript>(out var dropPlace))
        {
            if (prevFieldType != dropPlace.type)
            {
                if (TryGetComponent<CardControllerLink>(out var link))
                {
                    MoveCardOnField(link.Controller, transform.GetSiblingIndex(), false);
                }
            }
        }
    }

    public void MoveCardOnField(BattleCardController cardController, int siblingIndex, bool isAutoMoveCard)
    {
        gameManager.CurrentGame.PlayerHandListController.CardControllers.Remove(cardController);
        int index = Mathf.Clamp(siblingIndex, 0, gameManager.CurrentGame.PlayerFieldListController.CardControllers.Count);
        gameManager.CurrentGame.PlayerFieldListController.CardControllers.Insert(index, cardController);

        cardController.MarkAsPlayedInThisTurn(CardOwner.Player);
        cardController.BattleCardView.ApplyBattleStyle(cardController.BattleState);

        if (currentType == GameType.Multiplayer)
        {
            networkGameController.RpcRequestPlayCard(cardController.CardModel.id, index);
            if (!isAutoMoveCard) networkTurnManager.GoToAttackPhase();
        }
        else if (currentType == GameType.Bot && !isAutoMoveCard)
            botTurnManager.GoToAttackPhase();
    }

    private void UpdateTempCardPosition()
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

    private void UpdateCardIndexInHandListController(int newIndex)
    {
        if (TryGetComponent<CardControllerLink>(out var link))
        {
            List<BattleCardController> list = gameManager.CurrentGame.PlayerHandListController.CardControllers;

            list.Remove(link.Controller);
            newIndex = Mathf.Clamp(newIndex, 0, list.Count);
            list.Insert(newIndex, link.Controller);
        }
    }

    public IEnumerator MoveCardTransformToAnotherField(Transform fieldTransform, int siblingIndexInField, Transform attackingCardtransform = null)
    {
        GameObject placeholder = null;
        yield return CreatePlaceholder(fieldTransform, siblingIndexInField, p => placeholder = p);

        transform.SetParent(canvas.transform, true);

        if (attackingCardtransform != null)
            attackingCardtransform.SetSiblingIndex(transform.GetSiblingIndex() + 1);

        Sequence moveSequence = DOTween.Sequence();
        moveSequence.Append(transform.DOScale(1.15f, 0.1f).SetEase(Ease.OutQuad));
        moveSequence.Append(transform.DOMove(placeholder.transform.position, 0.5f).SetEase(Ease.OutQuad));
        moveSequence.Append(transform.DOScale(1f, 0.1f).SetEase(Ease.OutQuad));
        yield return moveSequence.WaitForCompletion();

        transform.SetParent(fieldTransform, false);
        transform.SetSiblingIndex(placeholder.transform.GetSiblingIndex());

        Destroy(placeholder);
    }

    private IEnumerator CreatePlaceholder(Transform fieldTransform, int siblingIndex, Action<GameObject> onCreated)
    {
        GameObject placeholder = Instantiate(tempCard);
        placeholder.GetComponent<Image>().enabled = false;

        placeholder.transform.SetParent(fieldTransform, false);
        placeholder.transform.SetSiblingIndex(siblingIndex);

        LayoutRebuilder.ForceRebuildLayoutImmediate(fieldTransform.GetComponent<RectTransform>());
        yield return null;

        onCreated?.Invoke(placeholder);
    }
}