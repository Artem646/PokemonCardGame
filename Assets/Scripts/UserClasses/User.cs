using System;
using System.Collections.Generic;

[Serializable]
public class User
{
    public UserData userData;
    public List<int> cardsInCollection = new();
    public List<Deck> decks = new();
}
