public static class SelectedDeckManager
{
    public static Deck SelectedDeck { get; private set; }

    public static void SetSelectedDeck(Deck deck)
    {
        SelectedDeck = deck;
    }

    public static void Clear()
    {
        SelectedDeck = null;
    }
}
