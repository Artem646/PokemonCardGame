using System.Collections.Generic;

public interface IFilterableCardListController : ICardListController
{
    void ApplyElementFilter(List<PokemonElement> activeFilters);
}