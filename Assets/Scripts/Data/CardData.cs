using System;
using System.Collections.Generic;

[Serializable]
public class Colors
{
    public string cardColor;
    public string borderColor1;
    public string borderColor2;
}

[Serializable]
public class CardData
{
    public string id;
    public string title;
    public string imageName;
    public string mainElement;
    public string secondaryElement;
    public Colors colors;

    // Раскомментируй по необходимости
    // public string ultimateDescription;
    // public int attack;
    // public int defense;
    // public int specialAttack;
    // public int physicalAttack;
    // public int health;
}

[Serializable]
public class CardList
{
    public List<CardData> cards;
}
