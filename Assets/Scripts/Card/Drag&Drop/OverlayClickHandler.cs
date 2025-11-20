using UnityEngine;
using UnityEngine.EventSystems;

public class OverlayClickHandler : MonoBehaviour, IPointerClickHandler
{
    public static System.Action OnOverlayClicked;

    public void OnPointerClick(PointerEventData eventData)
    {
        OnOverlayClicked?.Invoke();
    }
}