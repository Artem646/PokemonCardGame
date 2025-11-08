using System.Collections.Generic;
using System.Linq;

public class CardDeck
{
    public static CardDeck Instance { get; } = new CardDeck();

    private readonly HashSet<int> deckCardIds = new();

    public void AddCardToDeck(int cardId)
    {
        if (!deckCardIds.Contains(cardId))
        {
            deckCardIds.Add(cardId);
        }
    }

    public void RemoveCardFromDeck(int cardId)
    {
        if (deckCardIds.Contains(cardId))
        {
            deckCardIds.Remove(cardId);
        }
    }

    public bool IsCardInDeck(int cardId) => deckCardIds.Contains(cardId);

    public List<int> GetDeckCards() => deckCardIds.ToList();
}

