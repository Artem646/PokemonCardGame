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
        if (type == FieldType.SELF_FIELD || type == FieldType.SELF_HAND)
        {
            CardMovemantScript card = eventData.pointerDrag.GetComponent<CardMovemantScript>();
            if (card)
                card.DefaultParent = transform;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null && type != FieldType.ENEMY_FIELD && type != FieldType.ENEMY_HAND)
        {
            CardMovemantScript card = eventData.pointerDrag.GetComponent<CardMovemantScript>();
            if (card)
            {
                card.DefaultTempCardParent = transform;
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            CardMovemantScript card = eventData.pointerDrag.GetComponent<CardMovemantScript>();
            if (card && card.DefaultTempCardParent == transform)
            {
                card.DefaultTempCardParent = card.DefaultParent;
            }
        }
    }
}