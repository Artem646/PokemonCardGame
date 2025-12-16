using Newtonsoft.Json.Linq;
using UnityEngine;

public static class ConfigLoader
{
    public static string GetWebClientId()
    {
        TextAsset jsonString = Resources.Load<TextAsset>("config");
        JObject jsonObject = JObject.Parse(jsonString.text);
        string webClientId = jsonObject["Google"]["WebClientId"].ToString();
        return webClientId;
    }
}