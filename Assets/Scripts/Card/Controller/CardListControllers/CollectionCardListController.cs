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

    public void ApplyElementFilter(List<PokemonElement> activeFilters, VisualElement container, List<PokemonElement> pokemonElements)
    {
        if (activeFilters.Count == 0)
        {
            container.Clear();
            foreach (var controller in CardControllers)
            {
                controller.CollectionCardView.SetActive(true);
                container.Add(controller.CollectionCardView.CardRoot);
            }
            return;
        }

        var filteredCards = CardControllers.Where(controller => activeFilters.Contains(controller.CardModel.mainElement)
        || (controller.CardModel.secondaryElement.HasValue && activeFilters.Contains(controller.CardModel.secondaryElement.Value)))
        .ToList();

        var sortedCards = filteredCards.OrderBy(controller =>
            {
                var element = controller.CardModel.mainElement;
                var secondary = controller.CardModel.secondaryElement;
                int mainIndex = pokemonElements.IndexOf(element);
                int secIndex = secondary.HasValue ? pokemonElements.IndexOf(secondary.Value) : int.MaxValue;
                return Mathf.Min(mainIndex, secIndex);
            })
            .ToList();

        container.Clear();
        foreach (var controller in sortedCards)
        {
            controller.CollectionCardView.SetActive(true);
            container.Add(controller.CollectionCardView.CardRoot);
        }
    }

    protected override CollectionCardController CreateController(CardModel model)
    {
        var controller = CardControllerFactory.Create<CollectionCardController>(model);
        var userCards = CardRepository.Instance.GetUserCards();
        if (userCards != null)
        {
            var ownedIds = userCards.cards.Select(c => c.id).ToHashSet();
            bool isOwned = ownedIds.Contains(model.id);
            controller.CollectionCardView.SetOpacity(isOwned);
        }
        else
        {
            controller.CollectionCardView.SetOpacity(true);
        }
        return controller;
    }

    protected override void OnCardAdded(CollectionCardController controller)
    {
        if (cardContainer is VisualElement scrollView)
        {
            scrollView.Add(controller.CollectionCardView.CardRoot);
        }
    }
}
