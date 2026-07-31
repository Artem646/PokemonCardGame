using System;
using System.Collections.Generic;
using Firebase;
using Firebase.Auth;
using UnityEngine;
using Firebase.Firestore;
using System.Threading.Tasks;
using System.Linq;

public class FirebaseFirestoreService
{
    private static FirebaseFirestoreService _instance;
    public static FirebaseFirestoreService Instance => _instance ??= new FirebaseFirestoreService();

    private FirebaseFirestore firestore;
    public FirebaseFirestore GetFirestore() => firestore;

    private bool isFirestoreInitialized = false;

    private FirebaseFirestoreService() { }

    public void InitializeFirebaseFirestore(FirebaseApp app)
    {
        if (isFirestoreInitialized) return;

        firestore = FirebaseFirestore.GetInstance(app);
        if (firestore == null)
        {
            Debug.LogError("[P][FirebaseService] Ошибка: Firebase Firestore не проинициализирован.");
            return;
        }

        isFirestoreInitialized = true;
    }

    public async Task<bool> CheckIfUserExistsByUid(string uid)
    {
        DocumentReference userDocument = firestore.Collection("users").Document(uid);
        DocumentSnapshot snapshot = await userDocument.GetSnapshotAsync();
        return snapshot.Exists;
    }

    public async Task<bool> IsNicknameAvailable(string nickname)
    {
        Query query = firestore.Collection("users").WhereEqualTo("userData.userName", nickname);
        QuerySnapshot snapshot = await query.GetSnapshotAsync();
        return snapshot.Count == 0;
    }

    // -------------------------------------------------

    public async Task<User> CreateUserDocument(FirebaseUser firebaseUser, string nickname)
    {
        DocumentReference userDocument = firestore.Collection("users").Document(firebaseUser.UserId);

        byte[] defaultAvatarBytes = GetDefaultAvatarBytes();

        UserData newUserData = new()
        {
            userId = firebaseUser.UserId,
            userName = nickname,
            email = firebaseUser.Email ?? "",
            createdAt = DateTime.UtcNow,
            lastLoginAt = DateTime.UtcNow,
            profilePhotoData = defaultAvatarBytes
        };

        Dictionary<string, object> userDataMap = new()
        {
            { "userId", newUserData.userId },
            { "userName", newUserData.userName },
            { "email", newUserData.email},
            { "createdAt", Timestamp.FromDateTime(newUserData.createdAt) },
            { "lastLoginAt", Timestamp.FromDateTime(newUserData.lastLoginAt) },
            { "profilePhotoData", defaultAvatarBytes }
        };

        List<int> startCollection = GenerateStartCollection();

        Dictionary<string, object> newUserDocument = new()
        {
            { "userData", userDataMap },
            { "cardsInCollection", startCollection }
        };

        await userDocument.SetAsync(newUserDocument);
        Debug.Log($@"Документ пользователя ""{firebaseUser.UserId}"" создан.");
        return new User { userData = newUserData, cardsInCollection = startCollection };
    }

    private List<int> GenerateStartCollection()
    {
        GameCardModelList allGameCards = CardRepository.Instance.GetGameCardsList();
        System.Random random = new();

        List<CardModel> baseCards = allGameCards.cards.Where(card => card.evolutions.prev == null).ToList();

        HashSet<int> userStartCards = new();
        while (userStartCards.Count < 6 && userStartCards.Count < baseCards.Count)
        {
            int randomIndex = random.Next(baseCards.Count);
            userStartCards.Add(baseCards[randomIndex].id);
        }

        return userStartCards.ToList();
    }

    public async Task<User> LoadUser(string userId)
    {
        DocumentReference userDocument = firestore.Collection("users").Document(userId);
        DocumentSnapshot documentSnapshot = await userDocument.GetSnapshotAsync();

        Dictionary<string, object> data = documentSnapshot.ToDictionary();
        Dictionary<string, object> userData = data["userData"] as Dictionary<string, object>;

        byte[] photoBytes = null;
        if (userData.ContainsKey("profilePhotoData"))
        {
            object rawData = userData["profilePhotoData"];
            if (rawData is Blob blob) photoBytes = blob.ToBytes();
            else if (rawData is byte[] bytes) photoBytes = bytes;
        }

        UserData existUserData = new()
        {
            userId = userData["userId"].ToString(),
            userName = userData["userName"].ToString(),
            email = userData["email"].ToString(),
            createdAt = ((Timestamp)userData["createdAt"]).ToDateTime(),
            lastLoginAt = ((Timestamp)userData["lastLoginAt"]).ToDateTime(),
            profilePhotoData = photoBytes
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
            decks = new List<Deck>(),
            friends = new List<Friend>()
        };

        QuerySnapshot decksSnapshot = await documentSnapshot.Reference.Collection("decks").GetSnapshotAsync();
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

        QuerySnapshot friendsSnapshot = await documentSnapshot.Reference.Collection("friends").GetSnapshotAsync();
        foreach (DocumentSnapshot friendDocument in friendsSnapshot.Documents)
        {
            Dictionary<string, object> friendData = friendDocument.ToDictionary();

            byte[] friendPhotoBytes = null;
            if (friendData.ContainsKey("photoData"))
            {
                object rawData = friendData["photoData"];
                if (rawData is Blob blob) friendPhotoBytes = blob.ToBytes();
                else if (rawData is byte[] bytes) friendPhotoBytes = bytes;
            }

            Friend friend = new()
            {
                id = friendData["id"].ToString(),
                aliasName = friendData["name"].ToString(),
                photoData = friendPhotoBytes
            };

            user.friends.Add(friend);
        }

        await UpdateLastLoginAt(user);

        Debug.Log($@"Пользователь ""{userId}"" загружен. Колод: {user.decks.Count}");
        return user;
    }

    public async Task UpdateLastLoginAt(User user)
    {
        user.userData.lastLoginAt = DateTime.UtcNow;
        DocumentReference userDocument = firestore.Collection("users").Document(user.userData.userId);

        Dictionary<string, object> updateData = new()
        {
            { "userData.lastLoginAt", Timestamp.FromDateTime(user.userData.lastLoginAt) }
        };
        await userDocument.UpdateAsync(updateData);
    }

    public async Task DeleteAnonymousUserDocument(User user)
    {
        try
        {
            DocumentReference userDocument = firestore.Collection("users").Document(user.userData.userId);
            DocumentSnapshot snapshot = await userDocument.GetSnapshotAsync();
            if (snapshot.Exists)
            {
                await userDocument.DeleteAsync();
                Debug.Log($@"[FirestoreService] Документ анонимного пользователя ""{user.userData.userId}"" удалён.");
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

    private byte[] GetDefaultAvatarBytes()
    {
        Texture2D defaultTexture = Resources.Load<Texture2D>("Sprites/defaultAvatar");
        int maxImageSize = 256;
        Texture2D resizedTexture = ResizeTexture(defaultTexture, maxImageSize, maxImageSize);
        return resizedTexture.EncodeToJPG(75);
    }

    private Texture2D ResizeTexture(Texture2D source, int targetWidth, int targetHeight)
    {
        RenderTexture renderTexture = RenderTexture.GetTemporary(targetWidth, targetHeight);
        Graphics.Blit(source, renderTexture);

        Texture2D result = new(targetWidth, targetHeight);
        RenderTexture.active = renderTexture;
        result.ReadPixels(new Rect(0, 0, targetWidth, targetHeight), 0, 0);
        result.Apply();

        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(renderTexture);
        return result;
    }

    public async Task UpdateUserPhoto(string userId, byte[] photoData)
    {
        DocumentReference userDocument = firestore.Collection("users").Document(userId);
        Dictionary<string, object> updateData = new()
        {
            { "userData.profilePhotoData", photoData }
        };
        await userDocument.UpdateAsync(updateData);
    }

    // -------------------------------------------------

    public async Task AddCardToUserCollection(User user, int cardId)
    {
        if (!user.cardsInCollection.Contains(cardId))
            user.cardsInCollection.Add(cardId);

        DocumentReference userDocument = firestore.Collection("users").Document(user.userData.userId);
        var updateData = new Dictionary<string, object>
        {
            { "cardsInCollection", FieldValue.ArrayUnion(cardId) }
        };
        await userDocument.UpdateAsync(updateData);
        Debug.Log($@"[FirestoreService] Карта ""{cardId}"" добавлена пользователю ""{user.userData.userId}""");
    }

    public async Task RemoveCardFromUserCollection(User user, int cardId)
    {
        if (user.cardsInCollection.Contains(cardId))
            user.cardsInCollection.Remove(cardId);

        DocumentReference userDocument = firestore.Collection("users").Document(user.userData.userId);
        var updateData = new Dictionary<string, object>
        {
            { "cardsInCollection", FieldValue.ArrayRemove(cardId) }
        };
        await userDocument.UpdateAsync(updateData);
        Debug.Log($@"[FirestoreService] Карта ""{cardId}"" удалена у пользователя ""{user.userData.userId}""");
    }

    // -------------------------------------------------

    public async Task AddDeck(User user, Deck deck)
    {
        deck.deckId = GenerateDeckId();
        user.decks.Add(deck);

        DocumentReference deckDocument = firestore.Collection("users").Document(user.userData.userId).Collection("decks").Document(deck.deckId);

        Dictionary<string, object> deckData = new()
        {
            { "name", deck.name },
            { "cards", deck.cards }
        };

        await deckDocument.SetAsync(deckData);
        Localizer.LocalizeNotification(NotificationKey.DeckAdded, NotificationType.Success, deck.name);
    }

    public async Task UpdateDeck(User user, Deck deck)
    {
        DocumentReference deckDocument = firestore.Collection("users").Document(user.userData.userId).Collection("decks").Document(deck.deckId);

        Dictionary<string, object> deckData = new()
        {
            { "name", deck.name },
            { "cards", deck.cards }
        };

        await deckDocument.SetAsync(deckData, SetOptions.Overwrite);
        Localizer.LocalizeNotification(NotificationKey.DeckUpdated, NotificationType.Success, deck.name);
    }

    public async Task DeleteDeck(User user, Deck deck)
    {
        Deck existingDeck = user.decks.Find(d => d.deckId == deck.deckId);
        if (existingDeck != null)
        {
            user.decks.Remove(existingDeck);

            DocumentReference deckDocument = firestore.Collection("users").Document(user.userData.userId).Collection("decks").Document(deck.deckId);
            await deckDocument.DeleteAsync();

            Localizer.LocalizeNotification(NotificationKey.DeckDeleted, NotificationType.Success, deck.name);
        }
    }

    private string GenerateDeckId() => Guid.NewGuid().ToString("N");

    // -------------------------------------------------

    public async Task<string> FindUserIdByName(string userName)
    {
        Query query = firestore.Collection("users").WhereEqualTo("userData.userName", userName);
        QuerySnapshot snapshot = await query.GetSnapshotAsync();
        DocumentSnapshot documentSnapshot = snapshot.Documents.First();

        Dictionary<string, object> data = documentSnapshot.ToDictionary();
        Dictionary<string, object> userDataMap = data["userData"] as Dictionary<string, object>;

        return userDataMap["userId"].ToString();
    }

    public async Task AddFriend(User user, Friend friend)
    {
        user.friends.Add(friend);

        DocumentReference friendDocument = firestore.Collection("users").Document(user.userData.userId).Collection("friends").Document(friend.id);

        byte[] defaultAvatarBytes = GetDefaultAvatarBytes();

        Dictionary<string, object> friendData = new()
        {
            { "id", friend.id },
            { "name", friend.aliasName },
            { "photoData", defaultAvatarBytes }
        };

        await friendDocument.SetAsync(friendData);

        NotificationManager.ShowNotification("Друг добавлен", NotificationType.Success);
        // Localizer.LocalizeNotification(NotificationKey.DeckAdded, NotificationType.Success, deck.name);
    }

    public async Task UpdateFriend(User user, Friend friend)
    {
        DocumentReference friendDocument = firestore.Collection("users").Document(user.userData.userId).Collection("friends").Document(friend.id);
        Dictionary<string, object> updateData = new()
        {
            { "name", friend.aliasName }
        };
        await friendDocument.UpdateAsync(updateData);

        NotificationManager.ShowNotification("Друг изменён", NotificationType.Success);
        // Localizer.LocalizeNotification(NotificationKey.DeckUpdated, NotificationType.Success, deck.name);
    }

    public async Task UpdateFriendPhoto(string userId, Friend friend, byte[] photoData)
    {
        DocumentReference friendDocument = firestore.Collection("users").Document(userId).Collection("friends").Document(friend.id);
        Dictionary<string, object> updateData = new()
        {
            { "photoData", photoData }
        };
        await friendDocument.UpdateAsync(updateData);
    }

    public async Task DeleteFriend(User user, Friend friend)
    {
        Friend existingFriend = user.friends.Find(f => f.id == friend.id);
        if (existingFriend != null)
        {
            user.friends.Remove(existingFriend);

            DocumentReference friendDocument = firestore.Collection("users").Document(user.userData.userId).Collection("friends").Document(friend.id);
            await friendDocument.DeleteAsync();

            NotificationManager.ShowNotification("Друг удалён", NotificationType.Success);
            // Localizer.LocalizeNotification(NotificationKey.DeckDeleted, NotificationType.Success, deck.name);
        }
    }

    // -------------------------------------------------

    public void Dispose()
    {
        firestore = null;
        _instance = null;
    }
}