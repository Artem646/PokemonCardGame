using UnityEngine;
using Firebase.Firestore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.Runtime.CompilerServices;
using System.Globalization;

public static class CardsLoader
{
    public static CardsModelList FromJson(TextAsset cardsJson)
    {
        var settings = new JsonSerializerSettings
        {
            Converters =
            {
                new StringEnumConverter(new CamelCaseNamingStrategy()),
                new ColorHexConverter()
            }
        };
        return JsonConvert.DeserializeObject<CardsModelList>(cardsJson.text, settings);
    }
}