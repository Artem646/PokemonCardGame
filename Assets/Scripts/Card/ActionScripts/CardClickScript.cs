using UnityEngine;
using UnityEngine.EventSystems;

public class CardClickScript : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        if (FindAnyObjectByType<CameraViewManager>().IsSwitching) return;
        if (CardStateInteractionManager.IsAnyCardAnimating) return;

        if (transform.parent.TryGetComponent<CardSlot>(out var slot))
        {
            if ((!ConnectionConfig.IsSpectator && slot.type == FieldSlotType.SelfHandSlot) ||
                slot.type == FieldSlotType.SelfFieldSlot ||
                slot.type == FieldSlotType.EnemyFieldSlot)
            {
                if (CardStateInteractionManager.IsRaisedBy(this))
                    FindAnyObjectByType<GameInterfaceController>().CloseZoomOnCardView();
                else if (CardStateInteractionManager.CanBeginNewInteraction())
                {
                    CardStateInteractionManager.EndRaise();
                    if (CardStateInteractionManager.TryRaise(this))
                        FindAnyObjectByType<GameInterfaceController>().MoveToZoomOnCard(slot);
                }
            }
        }
    }
}