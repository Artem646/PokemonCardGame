using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.Threading.Tasks;
using System.Collections.Generic;
using Firebase.Firestore;
using System;

public static class CardsLoader
{
    private static FirebaseFirestore firebaseFirestore;

    public static GameCardModelList GetCardsListFromJson(string cardsJsonText)
    {
        var settings = new JsonSerializerSettings
        {
            Converters =
            {
                new StringEnumConverter(new CamelCaseNamingStrategy()),
                new ColorHexConverter()
            }
        };
        return JsonConvert.DeserializeObject<GameCardModelList>(cardsJsonText, settings);
    }

    public static async Task<List<int>> GetCardIdsFromFirestore(string userId)
    {
        try
        {
            firebaseFirestore = FirebaseFirestoreService.Instance.GetFirestore();
            DocumentSnapshot snapshot = await firebaseFirestore.Collection("users").Document(userId).GetSnapshotAsync();
            if (!snapshot.Exists)
            {
                return null;
            }
            return snapshot.GetValue<List<int>>("cardsInCollection") ?? null;
        }
        catch (Exception e)
        {
            Debug.LogError($"[P]Ошибка при загрузке ID карт из database: {e}");
            return null;
        }
    }
}