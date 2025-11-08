using System.Collections.Generic;

public interface IFilterableCardsListContainer : ICardsListContainer
{
    void ApplyElementFilter(List<PokemonElement> activeFilters);
}