using UnityEngine;

public static class CardStateInteractionManager
{
    private static MonoBehaviour dragOwner = null;
    private static MonoBehaviour raisedOwner = null;

    public static bool TryBeginDrag(MonoBehaviour card)
    {
        if (dragOwner != null || raisedOwner != null) return false;
        dragOwner = card;
        return true;
    }

    public static bool TryRaise(MonoBehaviour card)
    {
        if (dragOwner != null || raisedOwner != null) return false;
        raisedOwner = card;
        return true;
    }

    public static bool IsDragging => dragOwner != null;
    public static bool IsRaised => raisedOwner != null;

    public static bool IsDraggingBy(MonoBehaviour card) => dragOwner == card;
    public static bool IsRaisedBy(MonoBehaviour card) => raisedOwner == card;

    public static void EndDrag() => dragOwner = null;
    public static void EndRaise() => raisedOwner = null;
}
