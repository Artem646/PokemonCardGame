using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

public class CardRepositoryService
{
    private GameCardModelList gameCardsList = new();
    private UserCardModelList userCardsList = new();
    private readonly TextAsset cardsJson;

    public event Action OnCardsLoaded;
    public event Action<float> OnProgressChanged;

    public CardRepositoryService(TextAsset cardsJson)
    {
        this.cardsJson = cardsJson;
    }

    public async Task<UserCardModelList> GetUserCardsCollection()
    {
        try
        {
            List<int> cardIds = UserSession.Instance.ActiveUser.cardsInCollection;
            if (cardIds.Count == 0)
            {
                Debug.LogWarning("[P] У пользователя нет карт в коллекции.");
                OnCardsLoaded?.Invoke();
                return userCardsList;
            }

            await AddCardsToCollectionByIds(cardIds);

            Debug.Log($"[P] Успешно загружено {userCardsList.cards.Count} карт.");
        }
        catch (Exception e)
        {
            Debug.LogError($"[P] Ошибка при загрузке карт: {e}");
            return new UserCardModelList { cards = new List<CardModel>() };
        }

        OnCardsLoaded?.Invoke();
        return userCardsList;
    }

    public async Task<GameCardModelList> GetGameCards()
    {
        try
        {
            string cardsJsonText = cardsJson.text;
            var loaded = await Task.Run(() => CardsLoader.GetCardsListFromJson(cardsJsonText));
            if (loaded?.cards == null || loaded.cards.Count == 0)
            {
                Debug.LogWarning("[P] JSON загружен, но список карт пуст.");
                gameCardsList = new GameCardModelList { cards = new List<CardModel>() };
            }
            gameCardsList = loaded;
            Debug.Log($"[P] Загружено {gameCardsList.cards.Count} карт из JSON.");
        }
        catch (Exception e)
        {
            Debug.LogError($"[P] Ошибка при загрузке JSON карт: {e}");
            gameCardsList = new GameCardModelList { cards = new List<CardModel>() };
        }

        return gameCardsList;
    }

    private async Task AddCardsToCollectionByIds(List<int> cardIds)
    {
        int total = cardIds.Count;
        int loaded = 0;

        foreach (int cardId in cardIds)
        {
            CardModel card = GetCardById(cardId);
            if (card != null)
            {
                AddCardToCollection(card);
            }
            loaded++;
            float progress = (float)loaded / total;
            OnProgressChanged?.Invoke(progress);

            await Task.Yield();
        }
    }

    private CardModel GetCardById(int cardId)
    {
        CardModel card = gameCardsList.cards.FirstOrDefault(c => c.id == cardId);
        if (card == null)
        {
            Debug.LogWarning($"[P] Карта с id = {cardId} не найдена.");
        }
        return card;
    }

    public void AddCardToCollection(CardModel card)
    {
        if (card == null)
            return;

        bool alreadyExists = userCardsList.cards.Any(c => c.id == card.id);
        if (!alreadyExists)
        {
            userCardsList.cards.Add(card);
        }
    }

    public UserCardModelList GetUserCardsList() => userCardsList;
    public GameCardModelList GetGameCardsList() => gameCardsList;
    public void ClearUserCardsList() => userCardsList.cards.Clear();
}
