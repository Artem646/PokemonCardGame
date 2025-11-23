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
                { "username", newUserData.userName },
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
            await UpdateUserData(loaderUser, firebaseUser, userDocument);
            return loaderUser;
        }
    }

    public async Task UpdateUserData(User user, FirebaseUser firebaseUser, DocumentReference userDocument)
    {
        user.userData.userId = firebaseUser.UserId;
        user.userData.userName = firebaseUser.DisplayName;
        user.userData.email = firebaseUser.Email ?? "";
        user.userData.lastLoginAt = DateTime.UtcNow;

        Dictionary<string, object> updateData = new()
        {
            { "userData.userId", user.userData.userId },
            { "userData.username", user.userData.userName },
            { "userData.email", user.userData.email },
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

    public async Task SaveDeck(User user, Deck deck)
    {
        Deck existingDeck = user.decks.Find(d => d.deckId == deck.deckId);
        if (existingDeck != null)
        {
            existingDeck.name = deck.name;
            existingDeck.cards = new List<int>(deck.cards);
        }
        else
        {
            user.decks.Add(deck);
        }

        DocumentReference deckDoc = firestore.Collection("users").Document(user.userData.userId).Collection("decks").Document(deck.deckId);

        Dictionary<string, object> deckData = new()
        {
            { "name", deck.name },
            { "cards", deck.cards }
        };

        await deckDoc.SetAsync(deckData, SetOptions.Overwrite);
        Debug.Log($"[FirestoreService] Колода '{deck.name}' сохранена для пользователя {user.userData.userId} (id: {deck.deckId}), карт: {deck.cards.Count}");
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