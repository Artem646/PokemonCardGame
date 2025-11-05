using UnityEngine.UIElements;
using System.Collections.Generic;
using UnityEngine;

public class CardCollectionPresenter
{
    private ScrollView cardsContainer;
    private VisualTreeAsset cardTemplate;
    // private CardView cardView;
    private VisualElement overlay;
    private List<CardView> cardViews = new List<CardView>();

    public CardCollectionPresenter(ScrollView container, VisualTreeAsset template, VisualElement overlay)
    {
        cardsContainer = container;
        cardTemplate = template;
        this.overlay = overlay;
    }

    public void ReloadCards(List<CardData> cards)
    {
        cardsContainer.Clear();
        cardViews.Clear();

        foreach (CardData card in cards)
        {
            if (card != null)
            {
                CardView cardView = new(card, cardTemplate);
                cardView.BindOverlay(overlay);

                cardViews.Add(cardView);
                cardsContainer.Add(cardView);
            }
        }


    }



    //     List<CardData> cards = CardRepository.Instance.GetCachedCards();

    //     cardBuilder = new CardBuilder(uiDocument, cardTemplate);

    //     foreach (CardData card in cards)
    //     {
    //         if (card != null)
    //         {
    //             VisualElement cardElement = cardBuilder.CreateCardVisualElement(card);
    //             AddCardVisualElementToList(cardElement);
    //             AddCardToScrollView(cardElement);
    //         }
    //     }
    // }

    // public void AddCardVisualElementToList(VisualElement cardElement)
    // {
    //     cardsVisualElements.Add(cardElement);
    // }

    // public void AddCardToScrollView(VisualElement cardElement)
    // {
    //     cardsContainer.Add(cardElement);
    // }

    public List<CardView> GetAllCardViews() => cardViews;
}
