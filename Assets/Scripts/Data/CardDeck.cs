using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class CardDeck
{
    public static CardDeck Instance { get; } = new CardDeck();

    private List<VisualElement> cardDeck = new List<VisualElement>();

    public void ToggleAddToCardDeckButton(Button addToCardDeckButton, VisualElement cardVisualElement)
    {
        if (cardDeck.Contains(cardVisualElement))
        {
            cardDeck.Remove(cardVisualElement);
            addToCardDeckButton.style.unityBackgroundImageTintColor = new Color(0.5f, 0.5f, 0.5f);
        }
        else
        {
            cardDeck.Add(cardVisualElement);
            addToCardDeckButton.style.unityBackgroundImageTintColor = Color.white;
        }
    }

    public List<VisualElement> GetDeckCards() => cardDeck;
}

