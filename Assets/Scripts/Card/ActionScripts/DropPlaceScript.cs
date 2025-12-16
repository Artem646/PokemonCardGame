using UnityEngine;
using UnityEngine.EventSystems;

public enum FieldType
{
    SELF_HAND,
    SELF_FIELD,
    ENEMY_HAND,
    ENEMY_FIELD,
    NONE
}

public class DropPlaceScript : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    public FieldType type;

    public void OnDrop(PointerEventData eventData)
    {
        if (type == FieldType.SELF_FIELD)
        {
            if (eventData.pointerDrag.TryGetComponent<CardMovemantScript>(out var card))
            {
                int effectiveCountCard = transform.childCount;
                if (card.DefaultTempCardParent == transform)
                    effectiveCountCard--;

                if (effectiveCountCard >= 3)
                {
                    if (card.DefaultTempCardParent == transform)
                        card.DefaultTempCardParent = card.DefaultParent;
                    return;
                }

                card.DefaultParent = transform;
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null && type == FieldType.SELF_FIELD)
        {
            if (eventData.pointerDrag.TryGetComponent<CardMovemantScript>(out var card))
            {
                int effectiveCountCard = transform.childCount;
                if (card.DefaultTempCardParent == transform)
                    effectiveCountCard--;

                if (effectiveCountCard >= 3)
                {
                    return;
                }

                card.DefaultTempCardParent = transform;
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            if (eventData.pointerDrag.TryGetComponent<CardMovemantScript>(out var card))
            {
                if (card.DefaultTempCardParent == transform)
                    card.DefaultTempCardParent = card.DefaultParent;
            }
        }
    }
}
