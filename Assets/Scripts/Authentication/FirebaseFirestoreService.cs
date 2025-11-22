using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Firestore;
using Firebase.Auth;
using UnityEngine;

public class FirebaseFirestoreService
{
    private static FirebaseFirestoreService _instance;
    public static FirebaseFirestoreService Instance
    {
        get
        {
            _instance ??= new FirebaseFirestoreService();
            return _instance;
        }
    }

    private readonly FirebaseAuth auth;
    private FirebaseFirestore firestore;

    private bool isFirestoreInitialized = false;

    public FirebaseFirestore GetFirestore() => firestore;

    public FirebaseFirestoreService()
    { }

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

    public async Task CreateUserDocument(string userId)
    {
        DocumentReference userDocument = firestore.Collection("users").Document(userId);
        DocumentSnapshot snapshot = await userDocument.GetSnapshotAsync();

        if (!snapshot.Exists)
        {
            Debug.Log($"[P][FirestoreRepository] Документ пользователя {userId} создан.");

            Dictionary<string, object> profileData = new()
            {
                { "userId", auth.CurrentUser.UserId },
                { "username", auth.CurrentUser.DisplayName },
                { "email", auth.CurrentUser.Email ?? "" },
                { "createdAt", Timestamp.GetCurrentTimestamp() },
                { "lastLoginAt", Timestamp.GetCurrentTimestamp() }
            };

            Dictionary<string, object> userData = new()
            {
                { "profile", profileData },
                { "cards", new List<int>() }
            };

            await userDocument.SetAsync(userData);
        }
        else
        {
            Debug.Log($"[P][FirestoreRepository] Документ пользователя {userId} уже существует");
        }
    }

    public async Task DeleteAnonymousUserDocument(string userId)
    {
        try
        {
            DocumentReference userDoc = firestore.Collection("users").Document(userId);
            DocumentSnapshot snapshot = await userDoc.GetSnapshotAsync();

            if (snapshot.Exists)
            {
                await userDoc.DeleteAsync();
                Debug.Log($"[P][FirestoreRepository] Документ анонимного пользователя {userId} удалён.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[P][FirestoreRepository] Ошибка при удалении документа анонимного пользователя: {e.Message}");
        }
    }

    public async Task AddCardToUserCollection(string userId, int cardId)
    {
        DocumentReference userDoc = firestore.Collection("users").Document(userId);
        Dictionary<string, object> updateData = new()
        {
            { "cards", FieldValue.ArrayUnion(cardId) }
        };
        await userDoc.UpdateAsync(updateData);
        Debug.Log($"[P][FirestoreRepository] Карта {cardId} добавлена пользователю {userId}");
    }

    public async Task RemoveCardFromUserCollection(string userId, int cardId)
    {
        DocumentReference userDoc = firestore.Collection("users").Document(userId);
        Dictionary<string, object> updateData = new()
        {
            { "cards", FieldValue.ArrayRemove(cardId) }
        };
        await userDoc.UpdateAsync(updateData);
        Debug.Log($"[P][FirestoreRepository] Карта {cardId} удалена у пользователя {userId}");
    }

    public void Dispose()
    {
        firestore = null;
        _instance = null;
    }
}
