// using System.Collections.Generic;
// using UnityEngine;

// public class CollectionManager : MonoBehaviour
// {
//     public static CollectionManager Instance;
//     public List<CardData> allCards = new();
//     public List<CardData> playerCollection = new();

//     [Header("Card Images")]
//     public Sprite warriorImage;

//     void Awake()
//     {
//         if (Instance == null)
//         {
//             Instance = this;
//             DontDestroyOnLoad(gameObject);
//         }
//         else
//         {
//             Destroy(gameObject);
//         }

//         LoadDefaultCards();
//     }

//     void LoadDefaultCards()
//     {
//         allCards.Add(new CardData("card_01", "Воин", "Сильное существо", 3, 4, 5, warriorImage, CardData.CardType.Creature));
//         allCards.Add(new CardData("card_02", "Огненный шар", "Наносит 3 урона", 2, 3, 0, warriorImage, CardData.CardType.Spell));

//         playerCollection.Add(allCards[0]);
//         playerCollection.Add(allCards[1]);
//     }

//     public List<CardData> GetPlayerCollection()
//     {
//         return playerCollection;
//     }

//     public void AddCardToCollection(CardData card)
//     {
//         if (!playerCollection.Contains(card))
//         {
//             playerCollection.Add(card);
//         }
//     }
// }