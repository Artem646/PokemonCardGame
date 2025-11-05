using UnityEngine;
using Firebase.Firestore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;

public class CardRepository : MonoBehaviour
{
    public static CardRepository Instance { get; private set; }
    private FirebaseFirestore firebaseFirestore;
    private CardsModelList cardsModelList = new();
    private UserCardsModelList userCardsModelList = new();
    private List<int> cardIds;

    [SerializeField] private TextAsset cardsJson;

    public event Action OnCardsLoaded;
    public event Action<float> OnProgressChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private async void LoadCardsIdFromDatabase(string userId)
    {
        try
        {
            firebaseFirestore = FirebaseService.Instance.GetFirestore();
            DocumentSnapshot snapshot = await firebaseFirestore.Collection("users").Document(userId).GetSnapshotAsync();
            if (snapshot.Exists)
            {
                cardIds = snapshot.GetValue<List<int>>("cardsInCollection");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[P]Ошибка при загрузке id карт из database: {e}");
        }
    }

    public async Task<UserCardsModelList> GetCardsFromJsonById(string userId)
    {
        try
        {
            if (userCardsModelList.cards.Count > 0)
            {
                userCardsModelList.cards.Clear();
            }

            LoadCardsIdFromDatabase(userId);

            // CardsLoader.FromJson(cardsJson);

            int total = cardIds.Count;
            // int loaded = 0;

            // foreach (int id in cardIds)
            // {
            //     CardModel card = cardsModelList.cards.FirstOrDefault(c => c.id == int.Parse(id));

            //     if (card != null)
            //     {
            //         userCardsModelList.cards.Add(card);
            //     }

            //     loaded++;
            //     float progress = (float)loaded / total;
            //     OnProgressChanged?.Invoke(progress);

            //     await Task.Yield();
            // }
        }
        catch (Exception e)
        {
            Debug.LogError($"[P]Ошибка при загрузке карт: {e}");
        }

        OnCardsLoaded?.Invoke();
        return userCardsModelList;
    }

    // public async Task AddCardToCollection(string userId, string cardId)
    // {
    //     DocumentReference userDocument = firebaseFirestore.Collection("users").Document(userId);
    //     await userDocument.UpdateAsync("cardsInCollection", FieldValue.ArrayUnion(cardId));

    //     CardList cardList = JsonUtility.FromJson<CardList>(cardsJson.text);
    //     CardData card = cardList.cards.FirstOrDefault(c => c.id == cardId);
    //     if (card != null && !cachedCards.Any(c => c.id == cardId))
    //     {
    //         cachedCards.Add(card);
    //     }
    // }

    public List<CardModel> GetUserCards() => userCardsModelList.cards;
}
