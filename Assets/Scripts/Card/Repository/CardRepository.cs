// using UnityEngine;
// using Firebase.Firestore;
// using System.Collections.Generic;
// using System.Threading.Tasks;
// using System.Linq;
// using System;

// public class CardRepository : MonoBehaviour
// {
//     public static CardRepository Instance { get; private set; }
//     private CardsModelList allCardsList = new();
//     private UserCardsModelList userCardsList = new();

//     [SerializeField] private TextAsset cardsJson;

//     public event Action OnCardsLoaded;
//     public event Action<float> OnProgressChanged;

//     private void Awake()
//     {
//         if (Instance != null && Instance != this)
//         {
//             Destroy(gameObject);
//             return;
//         }
//         else
//         {
//             Instance = this;
//             DontDestroyOnLoad(gameObject);
//         }
//     }

//     private async Task<List<int>> GetUserCardIds(string userId)
//     {
//         return await CardsLoader.GetCardIdsFromFirestore(userId) ?? new List<int>();
//     }

//     private void LoadAllCardsIfNeeded()
//     {
//         if (allCardsList != null && allCardsList.cards.Count > 0)
//             return;

//         try
//         {
//             allCardsList = CardsLoader.GetCardsListFromJson(cardsJson);
//             Debug.Log($"[P] Загружено {allCardsList.cards.Count} карт из JSON.");
//         }
//         catch (Exception e)
//         {
//             Debug.LogError($"[P] Ошибка при загрузке JSON карт: {e}");
//         }
//     }

//     private async Task AddCardsToCollectionByIds(List<int> cardIds)
//     {
//         int total = cardIds.Count;
//         int loaded = 0;

//         foreach (int cardId in cardIds)
//         {
//             CardModel card = CreateCardById(cardId);
//             if (card != null)
//             {
//                 AddCardToCollection(card);
//             }

//             loaded++;
//             float progress = (float)loaded / total;
//             OnProgressChanged?.Invoke(progress);

//             await Task.Yield();
//         }
//     }

//     public void AddCardToCollection(CardModel card)
//     {
//         if (card == null)
//             return;

//         bool alreadyExists = userCardsList.cards.Any(c => c.id == card.id);
//         if (!alreadyExists)
//         {
//             userCardsList.cards.Add(card);
//         }
//     }

//     private CardModel CreateCardById(int cardId)
//     {
//         CardModel card = allCardsList.cards.FirstOrDefault(c => c.id == cardId);
//         if (card == null)
//         {
//             Debug.LogWarning($"[P] Карта с id = {cardId} не найдена.");
//         }
//         return card;
//     }

//     public async Task<UserCardsModelList> GetUserCardsCollection(string userId)
//     {
//         try
//         {
//             userCardsList.cards.Clear();

//             List<int> cardIds = await GetUserCardIds(userId);
//             if (cardIds.Count == 0)
//             {
//                 Debug.LogWarning("[P] У пользователя нет карт в коллекции.");
//                 OnCardsLoaded?.Invoke();
//                 return userCardsList;
//             }

//             LoadAllCardsIfNeeded();

//             await AddCardsToCollectionByIds(cardIds);

//             Debug.Log($"[P] Успешно загружено {userCardsList.cards.Count} карт.");
//         }
//         catch (Exception e)
//         {
//             Debug.LogError($"[P] Ошибка при загрузке карт: {e}");
//         }

//         OnCardsLoaded?.Invoke();
//         return userCardsList;
//     }

//     public UserCardsModelList GetUserCardsList() => userCardsList;
//     public CardsModelList GetAllCardsList() => allCardsList;
// }

using UnityEngine;
using System;
using System.Threading.Tasks;

public class CardRepository : MonoBehaviour
{
    public static CardRepository Instance { get; private set; }

    [SerializeField] private TextAsset cardsJson;

    private CardRepositoryService service;

    public event Action OnCardsLoaded;
    public event Action<float> OnProgressChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        service = new CardRepositoryService(cardsJson);
        service.OnCardsLoaded += () => OnCardsLoaded?.Invoke();
        service.OnProgressChanged += (p) => OnProgressChanged?.Invoke(p);
    }

    public async Task<UserCardsModelList> GetUserCardsCollection(string userId)
    {
        return await service.GetUserCardsCollection(userId);
    }

    public UserCardsModelList GetUserCards() => service.GetUserCardsList();
    public CardsModelList GetAllCards() => service.GetAllCardsList();
}
