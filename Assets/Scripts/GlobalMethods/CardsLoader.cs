using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

public static class CardsLoader
{
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
}