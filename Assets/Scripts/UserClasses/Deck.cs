using System;
using System.Collections.Generic;

[Serializable]
public class Deck
{
    public string deckId;
    public string name;
    public List<int> cards = new();
}
