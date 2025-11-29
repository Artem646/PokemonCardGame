using System.Collections.Generic;
using UnityEngine.UIElements;

public interface IFilterableCardListController : ICardListController
{
    void ApplyElementFilter(List<PokemonElement> activeFilters, VisualElement container, List<PokemonElement> pokemonElements);
}