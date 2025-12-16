using System;
using System.Collections.Generic;
using Firebase.Auth;
using UnityEngine;
using Firebase.Firestore;
using System.Threading.Tasks;

public class FirebaseFirestoreService
{
    private static FirebaseFirestoreService _instance;
    public static FirebaseFirestoreService Instance => _instance ??= new FirebaseFirestoreService();

    private FirebaseFirestore firestore;
    public FirebaseFirestore GetFirestore() => firestore;

    private bool isFirestoreInitialized = false;

    private FirebaseFirestoreService() { }

    public void InitializeFirebaseFirestore()
    {
        if (isFirestoreInitialized) return;

        firestore = FirebaseFirestore.DefaultInstance;
        if (firestore == null)
        {
            Debug.LogError("[P][FirebaseService] Ошибка: Firebase Firestore не проинициализирован.");
            return;
        }

        isFirestoreInitialized = true;

        Debug.Log("[P][FirebaseService] Firebase Firestore успешно инициализирован.");
    }

    public async Task<User> CreateOrUpdateUserDocument(FirebaseUser firebaseUser)
    {
        DocumentReference userDocument = firestore.Collection("users").Document(firebaseUser.UserId);
        DocumentSnapshot snapshot = await userDocument.GetSnapshotAsync();
        if (!snapshot.Exists)
        {
            Debug.Log($"[P][FirebaseService] Документ пользователя {firebaseUser.UserId} создан.");

            UserData newUserData = new()
            {
                userId = firebaseUser.UserId,
                userName = firebaseUser.DisplayName,
                email = firebaseUser.Email,
                createdAt = DateTime.UtcNow,
                lastLoginAt = DateTime.UtcNow
            };

            Dictionary<string, object> userDataMap = new()
            {
                { "userId", newUserData.userId },
                { "userName", newUserData.userName },
                { "email", newUserData.email},
                { "createdAt", Timestamp.FromDateTime(newUserData.createdAt) },
                { "lastLoginAt", Timestamp.FromDateTime(newUserData.lastLoginAt) }
            };

            Dictionary<string, object> newUserDocument = new()
            {
                { "userData", userDataMap },
                { "cardsInCollection", new List<int>() }
            };

            await userDocument.SetAsync(newUserDocument);
            return new User { userData = newUserData };
        }
        else
        {
            Debug.Log($"[P][FirebaseService] Документ пользователя {firebaseUser.UserId} уже существует");
            User loaderUser = await LoadUser(firebaseUser.UserId);
            await UpdateLastLoginAt(loaderUser, userDocument);
            return loaderUser;
        }
    }

    public async Task UpdateLastLoginAt(User user, DocumentReference userDocument)
    {
        user.userData.lastLoginAt = DateTime.UtcNow;
        Dictionary<string, object> updateData = new()
        {
            { "userData.lastLoginAt", Timestamp.FromDateTime(user.userData.lastLoginAt) }
        };
        await userDocument.UpdateAsync(updateData);
        Debug.Log($"[FirestoreService] Данные пользователя {user.userData.userId} обновлены.");
    }

    public async Task DeleteAnonymousUserDocument(User user)
    {
        try
        {
            DocumentReference userDoc = firestore.Collection("users").Document(user.userData.userId);
            DocumentSnapshot snapshot = await userDoc.GetSnapshotAsync();
            if (snapshot.Exists)
            {
                await userDoc.DeleteAsync();
                Debug.Log($"[FirestoreService] Документ анонимного пользователя {user.userData.userId} удалён.");
            }

            if (user != null)
            {
                user.userData = null;
                user?.cardsInCollection.Clear();
                user?.decks.Clear();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[FirestoreService] Ошибка при удалении анонимного пользователя: {e.Message}");
        }
    }

    public async Task AddCardToUserCollection(User user, int cardId)
    {
        if (!user.cardsInCollection.Contains(cardId))
        {
            user.cardsInCollection.Add(cardId);
        }

        DocumentReference userDoc = firestore.Collection("users").Document(user.userData.userId);
        var updateData = new Dictionary<string, object>
        {
            { "cardsInCollection", FieldValue.ArrayUnion(cardId) }
        };
        await userDoc.UpdateAsync(updateData);
        Debug.Log($"[FirestoreService] Карта {cardId} добавлена пользователю {user.userData.userId}");
    }

    public async Task RemoveCardFromUserCollection(User user, int cardId)
    {
        if (user.cardsInCollection.Contains(cardId))
        {
            user.cardsInCollection.Remove(cardId);
        }

        DocumentReference userDoc = firestore.Collection("users").Document(user.userData.userId);
        var updateData = new Dictionary<string, object>
        {
            { "cardsInCollection", FieldValue.ArrayRemove(cardId) }
        };
        await userDoc.UpdateAsync(updateData);
        Debug.Log($"[FirestoreService] Карта {cardId} удалена у пользователя {user.userData.userId}");
    }

    public async Task AddDeck(User user, Deck deck)
    {
        string deckId = GenerateDeckId();
        deck.deckId = deckId;
        user.decks.Add(deck);

        Debug.Log($"[DeckEditor] Новая колода '{deck.name}' добавлена локально. Карт: {deck.cards.Count}");

        DocumentReference deckDocument = firestore.Collection("users").Document(user.userData.userId).Collection("decks").Document(deck.deckId);

        Dictionary<string, object> deckData = new()
        {
            { "name", deck.name },
            { "cards", deck.cards }
        };

        await deckDocument.SetAsync(deckData);
        Debug.Log($"[FirestoreService] Новая колода '{deck.name}' сохранена для пользователя {user.userData.userId} (id: {deck.deckId})");
        NotificationManager.ShowNotification($"Колода '{deck.name}' добавлена для пользователя {user.userData.userName}");
    }

    public async Task UpdateDeck(User user, Deck deck)
    {
        Deck existingDeck = user.decks.Find(d => d.deckId == deck.deckId);
        if (existingDeck != null)
        {
            existingDeck.name = deck.name;
            existingDeck.cards = new List<int>(deck.cards);

            Debug.Log($"[DeckEditor] Колода '{deck.name}' обновлена локально. Карт: {deck.cards.Count}");

            DocumentReference deckDocument = firestore.Collection("users").Document(user.userData.userId).Collection("decks").Document(deck.deckId);

            Dictionary<string, object> deckData = new()
            {
                { "name", deck.name },
                { "cards", deck.cards }
            };

            await deckDocument.SetAsync(deckData, SetOptions.Overwrite);
            Debug.Log($"[FirestoreService] Колода '{deck.name}' обновлена для пользователя {user.userData.userId} (id: {deck.deckId})");
            NotificationManager.ShowNotification($"Колода '{deck.name}' обновлена для пользователя {user.userData.userName}");
        }
    }

    public async Task DeleteDeck(User user, Deck deck)
    {
        Deck existingDeck = user.decks.Find(d => d.deckId == deck.deckId);
        if (existingDeck != null)
        {
            user.decks.Remove(existingDeck);
            Debug.Log($"[DeckEditor] Колода '{existingDeck.name}' удалена локально.");

            DocumentReference deckDocument = firestore.Collection("users").Document(user.userData.userId).Collection("decks").Document(deck.deckId);
            await deckDocument.DeleteAsync();

            Debug.Log($"[FirestoreService] Колода '{deck.name}' удалена для пользователя {user.userData.userId} (id: {deck.deckId})");
            NotificationManager.ShowNotification($"Колода '{deck.name}' удалена для пользователя {user.userData.userName} (id: {deck.deckId})");
        }
    }

    private string GenerateDeckId()
    {
        string guid = Guid.NewGuid().ToString("N");
        return new string(guid);
    }
    public async Task<User> LoadUser(string userId)
    {
        DocumentReference userDocument = firestore.Collection("users").Document(userId);
        DocumentSnapshot documentSnapshot = await userDocument.GetSnapshotAsync();

        if (!documentSnapshot.Exists)
        {
            Debug.LogWarning($"[FirestoreService] Пользователь {userId} не найден.");
            return null;
        }

        Dictionary<string, object> data = documentSnapshot.ToDictionary();
        Dictionary<string, object> userDataMap = data["userData"] as Dictionary<string, object>;

        UserData existUserData = new()
        {
            userId = userDataMap["userId"].ToString(),
            userName = userDataMap["userName"].ToString(),
            email = userDataMap["email"].ToString(),
            createdAt = ((Timestamp)userDataMap["createdAt"]).ToDateTime(),
            lastLoginAt = ((Timestamp)userDataMap["lastLoginAt"]).ToDateTime()
        };

        List<int> cards = new();
        if (data.ContainsKey("cardsInCollection"))
        {
            foreach (var card in (List<object>)data["cardsInCollection"])
                cards.Add(Convert.ToInt32(card));
        }

        User user = new()
        {
            userData = existUserData,
            cardsInCollection = cards,
            decks = new List<Deck>()
        };

        QuerySnapshot decksSnapshot = await userDocument.Collection("decks").GetSnapshotAsync();
        foreach (DocumentSnapshot deckDocument in decksSnapshot.Documents)
        {
            Dictionary<string, object> deckData = deckDocument.ToDictionary();
            Deck deck = new()
            {
                deckId = deckDocument.Id,
                name = deckData["name"].ToString(),
                cards = new List<int>()
            };

            if (deckData.ContainsKey("cards"))
            {
                foreach (var card in (List<object>)deckData["cards"])
                    deck.cards.Add(Convert.ToInt32(card));
            }

            user.decks.Add(deck);
        }

        Debug.Log($"[FirestoreService] Пользователь {userId} загружен. Колод: {user.decks.Count}");
        return user;
    }

    public void Dispose()
    {
        firestore = null;
        _instance = null;
    }
}