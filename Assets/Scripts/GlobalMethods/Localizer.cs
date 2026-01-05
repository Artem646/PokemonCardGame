using UnityEngine.UIElements;
using UnityEngine.Localization;
using UnityEngine;
using TMPro;

public static class Localizer
{
    public static void LocalizeElement(VisualElement root, string elementName, string key, string tableName, params object[] arguments)
    {
        TextElement element = root.Q<TextElement>(elementName);
        if (element != null)
        {
            LocalizedString localizedText = new(tableName, key)
            {
                Arguments = arguments
            };

            localizedText.StringChanged += (str) =>
            {
                element.text = str;
            };
        }
    }

    public static void LocalizeGameObjectElement(GameObject root, string elementName, string key, string tableName)
    {
        if (root.transform.Find(elementName).TryGetComponent<TextMeshProUGUI>(out var element))
        {
            LocalizedString localizedText = new(tableName, key);
            localizedText.StringChanged += (str) =>
            {
                element.text = str;
            };
        }
    }

    public static void LocalizeElements(VisualElement root, (string elementName, string key)[] mappings, string tableName)
    {
        foreach (var (elementName, key) in mappings)
        {
            LocalizeElement(root, elementName, key, tableName);
        }
    }

    public static void LocalizeGameObjectElements(GameObject root, (string elemetnName, string key)[] mappings, string tableName)
    {
        foreach (var (elementName, key) in mappings)
        {
            LocalizeGameObjectElement(root, elementName, key, tableName);
        }
    }

    public static void LocalizeNotification(NotificationKey key, NotificationType type, params object[] arguments)
    {
        string tableName = "NotificationsText";
        string keyName = key.ToString();

        LocalizedString localizedText = new(tableName, keyName)
        {
            Arguments = arguments
        };

        string str = localizedText.GetLocalizedString();
        NotificationManager.ShowNotification(str, type);
    }
}