using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class AnimationCardClickScript : MonoBehaviour, IPointerClickHandler
{
    public event Action OnCardClicked;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (CardStateInteractionManager.TryRaise(this))
            OnCardClicked?.Invoke();
    }
}