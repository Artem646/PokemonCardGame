using System.Collections.Generic;
using System.Threading.Tasks;

public interface ICardListController
{
    Task LoadCardsToContainer(List<CardModel> cards);
    void Clear();
}