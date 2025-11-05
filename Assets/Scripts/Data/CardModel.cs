using System;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;

// [Serializable]
// public class CardColors
// {
//     public Color cardColor;
//     public Color borderColor1;
//     public Color borderColor2;
//     public static CardColors FromHex(string cardHex, string border1Hex, string border2Hex)
//     {
//         return new CardColors
//         {
//             cardColor = HexToColor(cardHex),
//             borderColor1 = HexToColor(border1Hex),
//             borderColor2 = HexToColor(border2Hex)
//         };
//     }

//     private static Color HexToColor(string hex)
//     {
//         if (ColorUtility.TryParseHtmlString(hex, out Color color)) return color;
//         return Color.white;
//     }
// }

public class ColorHexConverter : JsonConverter<Color>
{
    public override Color ReadJson(JsonReader reader, Type objectType, Color existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        string hex = reader.Value?.ToString();
        if (string.IsNullOrWhiteSpace(hex) || hex == "#")
            return Color.clear;

        if (ColorUtility.TryParseHtmlString(hex, out Color color))
            return color;

        return Color.white;
    }

    public override void WriteJson(JsonWriter writer, Color value, JsonSerializer serializer)
    {
        // Color32 c = value;
        // string hex = $"#{c.r:X2}{c.g:X2}{c.b:X2}";
        // writer.WriteValue(hex);
    }
}

[JsonObject]
public class CardColors
{
    [JsonConverter(typeof(ColorHexConverter))]
    public Color cardColor;

    [JsonConverter(typeof(ColorHexConverter))]
    public Color borderColor1;

    [JsonConverter(typeof(ColorHexConverter))]
    public Color borderColor2;
}

public enum PokemonElement
{
    Grass,
    Fire,
    Water,
    electric,
    Bug,
    Ground,
    Poison,
    Flying,
    Fighting,
    Normal,
    Psychic
}

[Serializable]
public class CardModel
{
    public int id;
    public string title;
    public string imageName;

    [JsonConverter(typeof(StringEnumConverter), typeof(CamelCaseNamingStrategy))]
    public PokemonElement mainElement;

    [JsonConverter(typeof(StringEnumConverter), typeof(CamelCaseNamingStrategy))]
    public PokemonElement? secondaryElement;
    public CardColors colors;

    // Раскомментируй по необходимости
    // public string ultimateDescription;
    // public int attack;
    // public int defense;
    // public int specialAttack;
    // public int physicalAttack;
    // public int health;
}

[Serializable]
public class CardModelList
{
    public List<CardModel> cards;
}
