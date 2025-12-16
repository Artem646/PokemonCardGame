using System.Collections.Generic;
using System.Threading.Tasks;

public interface ICardListController
{
    Task AddCardsToContainer(List<CardModel> cards);
    void Clear();
}