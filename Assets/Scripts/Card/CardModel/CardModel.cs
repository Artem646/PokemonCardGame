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

[JsonObject]
public class Evolutions
{
    public int? prev;
    public int? next;
}

[JsonObject]
public class Stats
{
    public int attack;
    public int defense;
    public int specialAttack;
    public int specialDefense;
}

[JsonObject]
public class Abilities
{
    public Ability firstAbility;
    public Ability secondAbility;
    public Ability thirdAbility;
    public Ability fourthAbility;
}

[JsonObject]
public class Ability
{
    public string name;
    public string discription;

    [JsonConverter(typeof(StringEnumConverter), typeof(CamelCaseNamingStrategy))]
    public PokemonElement? element;

    public AbilityType? type;
    public int? power;
}

public enum PokemonElement
{
    Grass, Fire, Water,
    Bug, Psychic, Fighting,
    Flying, Electric, Ground,
    Fairy, Normal, Poison,
    Steel
}

public enum AbilityType
{
    Physical, Special, Status
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

    public Evolutions evolutions;
    public CardColors colors;
    public Stats stats;
    public int health;
    public Abilities abilities;

    [NonSerialized] public CardVisualDataSO visualData;
    [NonSerialized] public CardUIToolkitVisualDataSO uiToolkitVisualData;
}
