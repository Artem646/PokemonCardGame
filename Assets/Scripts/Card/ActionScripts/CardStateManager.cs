using UnityEngine;
using System.Collections.Generic;

public static class CardStateInteractionManager
{
    private static MonoBehaviour dragOwner = null;
    private static MonoBehaviour raisedOwner = null;
    private static HashSet<MonoBehaviour> animatingCards = new();

    public static void LockCard(MonoBehaviour card, Transform cardTransform)
    {
        EndRaise();

        GameInterfaceController gameInterfaceController = Object.FindAnyObjectByType<GameInterfaceController>();
        if (gameInterfaceController != null) gameInterfaceController.CloseZoomOnCardView();

        if (animatingCards.Add(card))
        {
            if (cardTransform.TryGetComponent<Collider>(out var collider))
                collider.enabled = false;
        }
    }

    public static void UnlockCard(MonoBehaviour card, Transform cardTransform)
    {
        if (animatingCards.Remove(card))
        {
            if (cardTransform.TryGetComponent<Collider>(out var collider))
                collider.enabled = true;
        }
    }

    public static bool CanBeginNewInteraction()
    {
        if (dragOwner != null || animatingCards.Count > 0) return false;
        return true;
    }

    public static bool TryBeginDrag(MonoBehaviour card)
    {
        if (IsAnyInteractionActive) return false;
        dragOwner = card;
        return true;
    }

    public static bool TryRaise(MonoBehaviour card)
    {
        if (IsAnyInteractionActive) return false;
        raisedOwner = card;
        return true;
    }

    public static bool IsCardLocked(MonoBehaviour card) => animatingCards.Contains(card);

    public static bool IsAnyCardAnimating => animatingCards.Count > 0;
    public static bool IsAnyInteractionActive => dragOwner != null || raisedOwner != null || animatingCards.Count > 0;
    public static bool IsRaised => raisedOwner != null;

    public static bool IsDraggingBy(MonoBehaviour card) => dragOwner == card;
    public static bool IsRaisedBy(MonoBehaviour card) => raisedOwner == card;

    public static void EndDrag() => dragOwner = null;
    public static void EndRaise() => raisedOwner = null;
}