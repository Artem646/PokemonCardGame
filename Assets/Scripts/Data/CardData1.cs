// using System;
// using System.Collections.Generic;
// using UnityEngine;

// [Serializable]
// public class CardData
// {
//     public string id;
//     public string cardName;
//     public string description;
//     public int manaCost;
//     public int attack;
//     public int health;
//     public Sprite image;
//     public CardType type;

//     public enum CardType
//     {
//         Creature,
//         Spell,
//         Equipment
//     }

//     public CardData(string id, string name, string desc, int mana, int att, int hp, Sprite img, CardType type)
//     {
//         this.id = id;
//         cardName = name;
//         description = desc;
//         manaCost = mana;
//         attack = att;
//         health = hp;
//         image = img;
//         this.type = type;
//     }
// }


// using UnityEngine;
// using UnityEngine.UIElements;
// using System.IO;

// [System.Serializable]
// public class CardData
// {
//     public string title;
//     public string imagePath;
//     public string mainElement;
//     public string secondaryElement;
//     // public string abilityElement;
//     // public string ultimateDescription;
//     // public int attack;
//     // public int defense;
//     // public int specialAttack;
//     // public int physicalAttack;
//     // public int health;
// }

// public class CardLoader : MonoBehaviour
// {
//     [SerializeField] private UIDocument uiDocument;
//     [SerializeField] private string jsonFilePath = "Assets/Scripts/Data/CardData.json";

//     private VisualElement root;

//     void Start()
//     {
//         root = uiDocument.rootVisualElement;

//         string json = File.ReadAllText(jsonFilePath);
//         CardData data = JsonUtility.FromJson<CardData>(json);

//         root.Q<Label>("title").text = data.title;
//         root.Q<VisualElement>("image").style.backgroundImage = new StyleBackground(LoadTexture(data.imagePath));

//         root.Q<Image>("main-element").image = LoadTexture($"Assets/Art/UI/Icons/{data.mainElement}.png");
//         // root.Q<Image>("ability-icon").image = LoadTexture($"Assets/Art/Icons/{data.abilityElement}.png");

//         // root.Q<Label>("attack").text = $"Атака: {data.attack}";
//         // root.Q<Label>("defense").text = $"Защита: {data.defense}";
//         // root.Q<Label>("specialAttack").text = $"Особая атака: {data.specialAttack}";
//         // root.Q<Label>("physicalAttack").text = $"Физическая атака: {data.physicalAttack}";
//         // root.Q<Label>("health").text = $"Здоровье: {data.health}";
//         // root.Q<Label>("ultimate").text = data.ultimateDescription;
//     }

//     private Texture2D LoadTexture(string path)
//     {
//         if (File.Exists(path))
//         {
//             byte[] fileData = File.ReadAllBytes(path);
//             Texture2D tex = new Texture2D(2, 2);
//             tex.LoadImage(fileData);
//             return tex;
//         }
//         return null;
//     }
// }
