using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

public class CardRepositoryService
{
    private GameCardsModelList gameCardsList = new();
    private UserCardsModelList userCardsList = new();
    private readonly TextAsset cardsJson;

    public event Action OnCardsLoaded;
    public event Action<float> OnProgressChanged;

    public CardRepositoryService(TextAsset cardsJson)
    {
        this.cardsJson = cardsJson;
    }

    public async Task<UserCardsModelList> GetUserCardsCollection(string userId)
    {
        try
        {
            userCardsList?.cards?.Clear();

            List<int> cardIds = await GetUserCardIds(userId);
            if (cardIds.Count == 0)
            {
                Debug.LogWarning("[P] У пользователя нет карт в коллекции.");
                OnCardsLoaded?.Invoke();
                return userCardsList;
            }

            LoadAllCardsIfNeeded();
            await AddCardsToCollectionByIds(cardIds);

            Debug.Log($"[P] Успешно загружено {userCardsList.cards.Count} карт.");
        }
        catch (Exception e)
        {
            Debug.LogError($"[P] Ошибка при загрузке карт: {e}");
            return new UserCardsModelList { cards = new List<CardModel>() };
        }

        OnCardsLoaded?.Invoke();
        return userCardsList;
    }

    private async Task<List<int>> GetUserCardIds(string userId)
    {
        return await CardsLoader.GetCardIdsFromFirestore(userId) ?? new List<int>();
    }

    private void LoadAllCardsIfNeeded()
    {
        if (gameCardsList?.cards?.Count > 0)
            return;

        try
        {
            string cardsJsonText = cardsJson.text;
            var loaded = CardsLoader.GetCardsListFromJson(cardsJsonText);
            if (loaded?.cards == null || loaded.cards.Count == 0)
            {
                Debug.LogWarning("[P] JSON загружен, но список карт пуст.");
                gameCardsList = new GameCardsModelList { cards = new List<CardModel>() };
                return;
            }
            gameCardsList = loaded;
            Debug.Log($"[P] Загружено {gameCardsList.cards.Count} карт из JSON.");
        }
        catch (Exception e)
        {
            Debug.LogError($"[P] Ошибка при загрузке JSON карт: {e}");
            gameCardsList = new GameCardsModelList { cards = new List<CardModel>() };
        }
    }

    public async Task<GameCardsModelList> GetGameCards()
    {
        gameCardsList?.cards?.Clear();

        try
        {
            string cardsJsonText = cardsJson.text;
            var loaded = await Task.Run(() => CardsLoader.GetCardsListFromJson(cardsJsonText));
            if (loaded?.cards == null || loaded.cards.Count == 0)
            {
                Debug.LogWarning("[P] JSON загружен, но список карт пуст.");
                gameCardsList = new GameCardsModelList { cards = new List<CardModel>() };
            }
            gameCardsList = loaded;
            Debug.Log($"[P] Загружено {gameCardsList.cards.Count} карт из JSON.");
        }
        catch (Exception e)
        {
            Debug.LogError($"[P] Ошибка при загрузке JSON карт: {e}");
            gameCardsList = new GameCardsModelList { cards = new List<CardModel>() };
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

    public UserCardsModelList GetUserCardsList() => userCardsList;
    public GameCardsModelList GetGameCardsList() => gameCardsList;
}
