using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class CardClickScript : MonoBehaviour, IPointerClickHandler
{
    public event Action<CardClickScript> OnCardClicked;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (transform.parent.TryGetComponent(out DropPlaceScript dropPlace))
        {
            if (dropPlace.type == FieldType.SELF_HAND || dropPlace.type == FieldType.SELF_FIELD || dropPlace.type == FieldType.ENEMY_FIELD)
            {
                if (!CardStateManager.IsCardRaised)
                {
                    OnCardClicked?.Invoke(this);
                }
            }
        }
    }
}
