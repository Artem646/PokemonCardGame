using System;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

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
    public override void WriteJson(JsonWriter writer, Color value, JsonSerializer serializer) { }
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
    Psychic,
    Fairy
}

[Serializable]
public class CardModel
{
    public int id;
    public string titleKey;
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
