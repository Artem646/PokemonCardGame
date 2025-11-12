using UnityEngine.UIElements;

public class CollectionCardController : BaseCardController
{
    private bool isAddedToDeck;
    public bool isInteractive;

    public CollectionCardController(CardModel model, VisualTreeAsset template)
        : base(model, template)
    {
        RegisterEvents();
        isAddedToDeck = CardDeck.Instance.IsCardInDeck(model.id);
        CardView.SetAddedToDeck(isAddedToDeck);
    }

    public override void RegisterEvents()
    {
        CardView.RegisterClickHandlers(OnCardElementClicked, OnAddToCardDeckButtonClicked);
    }

    public void UnregisterEvents()
    {
        CardView.UnregisterClickHandlers(OnCardElementClicked, OnAddToCardDeckButtonClicked);
    }

    private void OnCardElementClicked(ClickEvent evt)
    {
        CollectionCardController cloneCollectionCardController = new(CardModel, CardView.CardTemplate);
        cloneCollectionCardController.UnregisterEvents();
        CardView cloneCardView = cloneCollectionCardController.CardView;
        CardOverlayManager.Instance?.ShowCard(CardView, cloneCardView, evt);
    }

    private void OnAddToCardDeckButtonClicked(ClickEvent evt)
    {
        // isAddedToDeck = !isAddedToDeck;

        // if (isAddedToDeck)
        //     CardDeck.Instance.AddCardToDeck(CardModel.id);
        // else
        //     CardDeck.Instance.RemoveCardFromDeck(CardModel.id);

        // CardView.SetAddedToDeck(isAddedToDeck);

        if (!isAddedToDeck)
        {
            bool added = CardDeck.Instance.AddCardToDeck(CardModel.id);
            if (added)
            {
                isAddedToDeck = true;
                CardView.SetAddedToDeck(true);
            }
        }
        else
        {
            CardDeck.Instance.RemoveCardFromDeck(CardModel.id);
            isAddedToDeck = false;
            CardView.SetAddedToDeck(false);
        }
    }
}
