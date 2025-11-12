using System.Collections.Generic;
using System.Linq;

public class CardDeck
{
    public static CardDeck Instance { get; } = new CardDeck();
    private readonly HashSet<int> deckCardIds = new();
    private const int MAX_COUNT = 5;

    // public void AddCardToDeck(int cardId)
    // {
    //     if (deckCardIds.Count >= MAX_COUNT)
    //         return;

    //     if (!deckCardIds.Contains(cardId))
    //     {
    //         deckCardIds.Add(cardId);
    //     }
    // }

    public bool AddCardToDeck(int id)
    {
        if (deckCardIds.Count >= MAX_COUNT)
            return false;

        deckCardIds.Add(id);
        return true;
    }

    public void RemoveCardFromDeck(int cardId)
    {
        // if (deckCardIds.Contains(cardId))
        // {
        deckCardIds.Remove(cardId);
        // }
    }

    public bool IsCardInDeck(int cardId) => deckCardIds.Contains(cardId);
    public List<int> GetDeckCards() => deckCardIds.ToList();
}

