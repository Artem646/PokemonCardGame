using System.Collections.Generic;
using System.Threading.Tasks;

public interface ICardsListContainer
{
    Task LoadCardsToContainer(List<CardModel> cards);
    void AddCardToContainer(CardModel model);
    void Clear();
    // void RefreshAllCards();
}