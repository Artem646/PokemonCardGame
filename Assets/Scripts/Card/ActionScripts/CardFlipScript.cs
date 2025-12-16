using System;
using UnityEngine;
using DG.Tweening;

public class CardFlipScript : MonoBehaviour
{
    [SerializeField] private RectTransform rectTransform;
    public event Action<bool> OnFlipStateChanged;

    public void FlipToFaceDown()
    {
        rectTransform.DORotate(new Vector3(0, 90, 0), 0.25f)
            .OnComplete(() =>
            {
                OnFlipStateChanged?.Invoke(true);
                rectTransform.DORotate(Vector3.zero, 0.25f);
            });
    }

    public void FlipToFaceUp()
    {
        rectTransform.DORotate(new Vector3(0, 90, 0), 0.25f)
            .OnComplete(() =>
            {
                OnFlipStateChanged?.Invoke(false);
                rectTransform.DORotate(Vector3.zero, 0.25f);
            });
    }
}
