using UnityEngine;
using Firebase.Firestore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

public class CardRepository : MonoBehaviour
{
    public static CardRepository Instance { get; private set; }
    private FirebaseFirestore firebaseFirestore;
    private List<CardData> cachedCards = new();
    // private List<CardModel> cardModelList = new();

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

    public async Task<List<CardData>> LoadUserCardsFromDatabase(string userId)
    {
        if (cachedCards.Count > 0)
        {
            cachedCards.Clear();
        }

        try
        {
            firebaseFirestore = FirebaseService.Instance.GetFirestore();

            DocumentSnapshot snapshot = await firebaseFirestore.Collection("users").Document(userId).GetSnapshotAsync();
            if (snapshot.Exists)
            {
                List<string> cardIDs = snapshot.GetValue<List<string>>("cardsInCollection");
                var settings = new JsonSerializerSettings
                {
                    Converters = {
                        new StringEnumConverter(new CamelCaseNamingStrategy()),
                        new ColorHexConverter()
                    }
                };

                CardModelList cardModelList = JsonConvert.DeserializeObject<CardModelList>(cardsJson.text, settings);

                int total = cardIDs.Count;
                int loaded = 0;

                foreach (string id in cardIDs)
                {
                    int intId = Int32.Parse(id);
                    CardModel card = cardModelList.cards.FirstOrDefault(c => c.id == intId);
                    Debug.Log(card.title);
                    Debug.Log(card.colors.cardColor);
                    Debug.Log(card.imageName);
                    Debug.Log(card.secondaryElement);
                    Debug.Log(card.mainElement);
                    Debug.Log(card.colors.borderColor1);

                    // if (card != null)
                    // {
                    //     cachedCards.Add(card);
                    // }

                    loaded++;
                    float progress = (float)loaded / total;
                    OnProgressChanged?.Invoke(progress);

                    await Task.Yield();
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[P]Ошибка при загрузке карт: {e}");
        }

        OnCardsLoaded?.Invoke();
        return cachedCards;
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

    public List<CardData> GetCachedCards() => cachedCards;
}
