using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardMovemantScript : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private LayerMask dragSurfaceMask;

    private Camera mainCamera;

    private CardSlot startSlot;
    private CardSlot highlightedSlot;

    private bool isDraggable;
    private Vector3 dragOffset;
    private float dragHeight = 0.2f;

    private Bounds tableBounds;
    private float cardHalfWidth;
    private float cardHalfLength;

    private static RaycastHit[] rayHits = new RaycastHit[10];

    private GameManager gameManager;
    private GameInterfaceController gameInterfaceController;
    private CameraViewManager cameraViewManager;
    private BotTurnManager botTurnManager;
    private NetworkGameTurnManager networkGameTurnManager;
    private NetworkGameController networkGameController;

    private GameType currentType = GameTypeConfig.CurrentType;

    private void InitObjectsIfNeeded()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        if (cameraViewManager == null) cameraViewManager = FindAnyObjectByType<CameraViewManager>();
        if (gameManager == null) gameManager = FindAnyObjectByType<GameManager>();
        if (gameInterfaceController == null) gameInterfaceController = FindAnyObjectByType<GameInterfaceController>();
        if (currentType == GameType.Bot)
        {
            if (botTurnManager == null)
                botTurnManager = FindAnyObjectByType<BotTurnManager>();
        }
        else if (currentType == GameType.Multiplayer)
        {
            if (networkGameController == null || networkGameTurnManager == null)
            {
                networkGameController = FindAnyObjectByType<NetworkGameController>();
                networkGameTurnManager = FindAnyObjectByType<NetworkGameTurnManager>();
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        InitObjectsIfNeeded();

        if (transform.parent.TryGetComponent<CardSlot>(out var slot))
            startSlot = slot;
        else
            startSlot = null;

        isDraggable = gameManager.IsMyTurn && startSlot.type == FieldSlotType.SelfHandSlot
            && (currentType == GameType.Multiplayer ?
            networkGameTurnManager.CurrentPhase == TurnPhase.PlayCard :
            botTurnManager.CurrentPhase == TurnPhase.PlayCard);

        if (isDraggable && cameraViewManager.IsZoomView)
            return;

        if (isDraggable && cameraViewManager.CurrentViewMode != CameraViewMode.Default)
        {
            NotificationManager.ShowNotification(@"Перемещать объекты можно только в виде ""Под углом""", NotificationType.Info, 1f);
            return;
        }

        if (isDraggable && CardStateInteractionManager.TryBeginDrag(this))
        {
            Ray ray = mainCamera.ScreenPointToRay(eventData.position);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, dragSurfaceMask))
            {
                tableBounds = hit.collider.bounds;

                BoxCollider cardBoxCollider = gameObject.GetComponent<BoxCollider>();
                cardHalfWidth = cardBoxCollider.bounds.extents.x + 0.4f;
                cardHalfLength = cardBoxCollider.bounds.extents.z + 0.4f;

                dragOffset = transform.position - hit.point;
                dragOffset.y = 0;

                CardStateInteractionManager.LockCard(this, transform);

                transform.DOMove(transform.position + Vector3.up * dragHeight, 0.15f);
                transform.DOScale(transform.localScale * 1.1f, 0.1f);
            }
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isDraggable && CardStateInteractionManager.IsDraggingBy(this))
        {
            Plane tablePlane = new(Vector3.up, new Vector3(0, tableBounds.max.y, 0));

            Ray ray = mainCamera.ScreenPointToRay(eventData.position);
            if (tablePlane.Raycast(ray, out float distance))
            {
                Vector3 mouseWorldPoint = ray.GetPoint(distance);
                Vector3 desiredPos = mouseWorldPoint + dragOffset;

                float clampedX = Mathf.Clamp(desiredPos.x, tableBounds.min.x + cardHalfWidth, tableBounds.max.x - cardHalfWidth);
                float clampedZ = Mathf.Clamp(desiredPos.z, tableBounds.min.z + cardHalfLength, tableBounds.max.z - cardHalfLength);

                transform.position = new Vector3(clampedX, tableBounds.max.y + dragHeight, clampedZ);
            }

            UpdateSlotHighlight(eventData);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isDraggable && CardStateInteractionManager.IsDraggingBy(this))
            StartCoroutine(OnEndDragRoutine(eventData));
    }

    private IEnumerator OnEndDragRoutine(PointerEventData eventData)
    {
        if (highlightedSlot != null)
        {
            highlightedSlot.Highlight(false);
            highlightedSlot = null;
        }

        CardSlot targetSlot = TryGetSlotUnderCard(eventData);

        // if (targetSlot != null && targetSlot.IsEmpty &&
        //     (targetSlot.type == FieldSlotType.SELF_FIELD_SLOT || targetSlot.type == FieldSlotType.SELF_HAND_SLOT))
        // {
        //     yield return PlaceToSlot(targetSlot);
        //     startSlot.Clear();

        //     // OnCardPlayed(controller, targetSlot);

        //     if (startSlot.type != targetSlot.type)
        //     {
        //         if (TryGetComponent<Card3DControllerLink>(out var link))
        //             MoveCardOnField(link.Controller, (int)startSlot.indexSlotInField, (int)targetSlot.indexSlotInField, false);
        //     }
        // }
        // else
        //     yield return PlaceToSlot(startSlot);

        if (targetSlot != null && !targetSlot.IsEmpty && targetSlot.type == FieldSlotType.SelfHandSlot && startSlot != targetSlot)
        {
            yield return startSlot.SwapWith(targetSlot);

            int startSlotIndex = (int)startSlot.indexSlotInField;
            int targetSlotIndex = (int)targetSlot.indexSlotInField;

            if (TryGetComponent<CardControllerLink>(out var startLink) &&
                targetSlot.CurrentCard.TryGetComponent<CardControllerLink>(out var targetLink))
            {
                gameManager.CurrentGame.PlayerHandControllers[targetSlotIndex] = startLink.Controller;
                gameManager.CurrentGame.PlayerHandControllers[startSlotIndex] = targetLink.Controller;
            }
        }
        else if (targetSlot != null && targetSlot.IsEmpty && targetSlot.type == FieldSlotType.SelfHandSlot)
        {
            yield return PlaceToSlot(targetSlot);
            startSlot.Clear();

            int startSlotIndex = (int)startSlot.indexSlotInField;
            int targetSlotIndex = (int)targetSlot.indexSlotInField;

            if (TryGetComponent<CardControllerLink>(out var link))
            {
                gameManager.CurrentGame.PlayerHandControllers[startSlotIndex] = null;
                gameManager.CurrentGame.PlayerHandControllers[targetSlotIndex] = link.Controller;
            }
        }
        else if (targetSlot != null && targetSlot.IsEmpty && targetSlot.type == FieldSlotType.SelfFieldSlot)
        {
            yield return PlaceToSlot(targetSlot);
            startSlot.Clear();

            if (TryGetComponent<CardControllerLink>(out var link))
            {
                MoveCardOnField(link.Controller, (int)startSlot.indexSlotInField, (int)targetSlot.indexSlotInField, false);
                // yield return ReorganizeHandCoroutine();
            }
        }
        else yield return PlaceToSlot(startSlot);

        CardStateInteractionManager.UnlockCard(this, transform);
        CardStateInteractionManager.EndDrag();
    }

    private IEnumerator PlaceToSlot(CardSlot slot)
    {
        yield return transform.DOMove(slot.transform.position, 0.3f).SetEase(Ease.OutQuad).WaitForCompletion();
        if (TryGetComponent<CardControllerLink>(out var link))
            yield return slot.PlaceCard(link.Controller);
    }

    public void MoveCardOnField(BattleCardController cardController, int startSlotIndex, int targetSlotIndex, bool isAutoMoveCard)
    {
        InitObjectsIfNeeded();

        gameManager.CurrentGame.PlayerHandControllers[startSlotIndex] = null;
        gameManager.CurrentGame.PlayerFieldControllers[targetSlotIndex] = cardController;

        cardController.MarkAsPlayedInThisTurn(CardOwner.Player);
        cardController.BattleCardView.ApplyBattleStyle(cardController.BattleState);

        if (currentType == GameType.Multiplayer)
        {
            int cardId = cardController.CardModel.id;
            networkGameController.RequestPlayCard(startSlotIndex, targetSlotIndex, cardId);
            if (!isAutoMoveCard) networkGameTurnManager.GoToAttackPhase();
        }
        else if (currentType == GameType.Bot && !isAutoMoveCard)
            botTurnManager.GoToAttackPhase();
    }

    private CardSlot TryGetSlotUnderCard(PointerEventData eventData)
    {
        Ray ray = mainCamera.ScreenPointToRay(eventData.position);

        int hitCount = Physics.RaycastNonAlloc(ray, rayHits, 100f);
        if (hitCount == 0) return null;

        for (int i = 0; i < hitCount; i++)
        {
            RaycastHit hit = rayHits[i];
            if (hit.collider.TryGetComponent<CardSlot>(out var slot))
                return slot;
        }

        return null;
    }

    private void UpdateSlotHighlight(PointerEventData eventData)
    {
        CardSlot slot = TryGetSlotUnderCard(eventData);
        if (slot != highlightedSlot)
        {
            if (highlightedSlot != null) highlightedSlot.Highlight(false);
            if (slot != null) slot.Highlight(true);
            highlightedSlot = slot;
        }
    }

    public IEnumerator MoveCardTransformToField(CardSlot fieldSlot)
    {
        CardStateInteractionManager.LockCard(this, transform);

        if (TryGetComponent<CardControllerLink>(out var link))
        {
            if (ConnectionConfig.IsSpectator)
                yield return fieldSlot.PlaceCardWithMoveAndFlip(link.Controller);
            else
            {
                if (fieldSlot.type == FieldSlotType.SelfFieldSlot)
                    yield return fieldSlot.PlaceCardWithMove(link.Controller);
                else if (fieldSlot.type == FieldSlotType.EnemyFieldSlot)
                    yield return fieldSlot.PlaceCardWithMoveAndFlip(link.Controller);
            }
        }

        CardStateInteractionManager.UnlockCard(this, transform);
    }

    public IEnumerator ReorganizeHandCoroutine()
    {
        BattleCardController[] handControllers = gameManager.CurrentGame.PlayerHandControllers;
        List<BattleCardController> activeCards = new();

        for (int i = 0; i < handControllers.Length; i++)
        {
            if (handControllers[i] != null)
            {
                activeCards.Add(handControllers[i]);
                handControllers[i] = null;
            }
        }

        if (activeCards.Count == 0) yield break;

        CardSlot[] handSlots = gameManager.PlayerHandManager.HandSlots;
        foreach (CardSlot slot in handSlots)
            slot.Clear();

        int startIndex = (5 - activeCards.Count) / 2;

        List<Coroutine> moveCardsCoroutines = new();

        for (int i = 0; i < activeCards.Count; i++)
        {
            int newIndex = startIndex + i;
            BattleCardController card = activeCards[i];
            CardSlot targetSlot = handSlots[newIndex];

            handControllers[newIndex] = card;

            moveCardsCoroutines.Add(StartCoroutine(targetSlot.PlaceCard(card)));
        }

        foreach (var coroutine in moveCardsCoroutines)
            yield return coroutine;
    }
}
