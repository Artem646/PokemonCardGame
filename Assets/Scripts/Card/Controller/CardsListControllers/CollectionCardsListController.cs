using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

public class CollectionCardsListController : CardsListController<CollectionCardController>, IFilterableCardsListContainer
{
    public CollectionCardsListController(VisualElement container)
        : base(container) { }

    public async Task LoadUserCardsToScrollView()
    {
        Clear();
        UserCardsModelList userCards = CardRepository.Instance.GetUserCards();
        if (userCards == null || userCards.cards.Count == 0)
        {
            Debug.Log("[P][CardsCollectionController] Коллекция пуста.");
            return;
        }

        await LoadCardsToContainer(userCards.cards);
        Debug.Log($"[P][CardsCollectionController] Загружено {userCards.cards.Count} карт.");
    }

    public async Task LoadGameCardsToScrollView()
    {
        Clear();
        GameCardsModelList gameCards = CardRepository.Instance.GetGameCards();
        await LoadCardsToContainer(gameCards.cards);
    }

    public void ApplyElementFilter(List<PokemonElement> activeFilters)
    {
        foreach (CollectionCardController cardController in activeCardControllers)
        {
            PokemonElement mainElement = cardController.CardModel.mainElement;
            PokemonElement? secondaryElement = cardController.CardModel.secondaryElement;

            bool IsFilterElement = activeFilters.Count == 0 || activeFilters.Any(filter => filter == mainElement || filter == secondaryElement);

            cardController.CardView.SetOpacity(IsFilterElement);
        }
    }
}
