public static class CardStateManager
{
    public static bool IsCardRaised { get; private set; } = false;

    public static void RaiseCard()
    {
        IsCardRaised = true;
    }

    public static void ResetCardState()
    {
        IsCardRaised = false;
    }
}