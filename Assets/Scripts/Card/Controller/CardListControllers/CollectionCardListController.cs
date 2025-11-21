using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

public class CollectionCardListController : CardListController<CollectionCardController>, IFilterableCardListController
{
    public CollectionCardListController(VisualElement container)
        : base(container) { }

    public async Task LoadUserCardsToScrollView()
    {
        Clear();
        UserCardModelList userCards = CardRepository.Instance.GetUserCards();
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
        GameCardModelList gameCards = CardRepository.Instance.GetGameCards();
        await LoadCardsToContainer(gameCards.cards);
    }

    public void ApplyElementFilter(List<PokemonElement> activeFilters)
    {
        foreach (CollectionCardController controller in CardControllers)
        {
            PokemonElement mainElement = controller.CardModel.mainElement;
            PokemonElement? secondaryElement = controller.CardModel.secondaryElement;

            bool IsFilterElement = activeFilters.Count == 0 || activeFilters.Any(filter => filter == mainElement || filter == secondaryElement);

            controller.CollectionCardView.SetOpacity(IsFilterElement);
        }
    }
}
