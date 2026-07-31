using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class AnimationCardMovemantScript : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private LayerMask dragSurfaceMask;

    private Camera mainCamera;

    private CardSlot startSlot;
    private CardSlot highlightedSlot;

    private Vector3 dragOffset;
    private float dragHeight = 0.2f;

    private Bounds tableBounds;
    private float cardHalfWidth;
    private float cardHalfLength;

    private static RaycastHit[] rayHits = new RaycastHit[10];

    public void OnBeginDrag(PointerEventData eventData)
    {
        mainCamera = Camera.main;

        if (transform.parent.TryGetComponent<CardSlot>(out var slot))
            startSlot = slot;

        if (CardStateInteractionManager.TryBeginDrag(this))
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
        if (CardStateInteractionManager.IsDraggingBy(this))
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
        if (CardStateInteractionManager.IsDraggingBy(this))
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

        if (targetSlot != null && ((startSlot.type == FieldSlotType.SelfFieldSlot && targetSlot.type == FieldSlotType.SelfFieldSlot) ||
            (startSlot.type == FieldSlotType.EnemyFieldSlot && targetSlot.type == FieldSlotType.EnemyFieldSlot)))
        {
            yield return PlaceToSlot(targetSlot);
            startSlot.Clear();
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
        bool isSlotValid = false;
        if (slot != null && startSlot != null)
        {
            if ((startSlot.type == FieldSlotType.SelfFieldSlot && slot.type == FieldSlotType.SelfFieldSlot) ||
                (startSlot.type == FieldSlotType.EnemyFieldSlot && slot.type == FieldSlotType.EnemyFieldSlot))
                isSlotValid = true;
        }
        if (!isSlotValid) slot = null;
        if (slot != highlightedSlot)
        {
            if (highlightedSlot != null) highlightedSlot.Highlight(false);
            if (slot != null) slot.Highlight(true);
            highlightedSlot = slot;
        }
    }
}
