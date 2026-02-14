using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class CardClickScript : MonoBehaviour, IPointerClickHandler
{
    public event Action OnCardClicked;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (transform.parent.TryGetComponent<DropPlaceScript>(out var dropPlace))
        {
            if (dropPlace.type == FieldType.SELF_HAND ||
                dropPlace.type == FieldType.SELF_FIELD ||
                dropPlace.type == FieldType.ENEMY_FIELD)
            {
                if (CardStateInteractionManager.TryRaise(this))
                    OnCardClicked?.Invoke();
            }
        }
    }
}
