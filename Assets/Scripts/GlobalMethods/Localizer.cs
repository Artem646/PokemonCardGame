using UnityEngine.UIElements;
using UnityEngine.Localization;
using UnityEngine;
using TMPro;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Localization.Settings;

public static class Localizer
{
    public static void LocalizeElement(VisualElement root, string elementName, string key, string tableName, params object[] arguments)
    {
        TextElement textElement = root.Q<TextElement>(elementName);
        if (textElement != null)
        {
            LocalizedString localizedText = new(tableName, key) { Arguments = arguments };
            localizedText.StringChanged += (str) => { textElement.text = str; };
        }
    }

    public static void LocalizeGameObjectElement(TextMeshProUGUI element, string key, string tableName)
    {
        LocalizedString localizedText = new(tableName, key);
        localizedText.StringChanged += (str) => { element.text = str; };
    }

    public static void LocalizeCardTitleElement(VisualElement cardRoot, string elementName, CardModel cardModel, string tableName)
    {
        TextElement textElement = cardRoot.Q<TextElement>(elementName);
        if (textElement != null)
        {
            LocalizedString localizedText = new(tableName, cardModel.titleKey);
            localizedText.StringChanged += (str) =>
            {
                textElement.text = @$"<color=white><gradient=""TextGradient"">{str}</gradient></color>";

                string currentLang = LocalizationSettings.SelectedLocale.Identifier.Code;
                if (currentLang.StartsWith("ru") || currentLang.StartsWith("be"))
                {
                    textElement.style.fontSize = cardModel.uiToolkitVisualData.fontSizeRU;
                    textElement.style.letterSpacing = cardModel.uiToolkitVisualData.letterSpacingRU;
                    textElement.style.unityFontStyleAndWeight = FontStyle.Bold;
                }
                else
                {
                    textElement.style.fontSize = cardModel.uiToolkitVisualData.fontSizeEN;
                    textElement.style.letterSpacing = cardModel.uiToolkitVisualData.letterSpacingEN;
                    textElement.style.unityFontStyleAndWeight = FontStyle.Normal;
                }
            };
        }
    }

    public static void LocalizeCard3DTitleGameObject(GameObject cardRoot, string elementName, CardModel cardModel, string tableName)
    {
        if (cardRoot.transform.Find(elementName).TryGetComponent<TextMeshPro>(out var element))
        {
            LocalizedString localizedText = new(tableName, cardModel.titleKey);
            localizedText.StringChanged += (str) =>
            {
                element.text = str;

                string currentLang = LocalizationSettings.SelectedLocale.Identifier.Code;
                if (currentLang.StartsWith("ru") || currentLang.StartsWith("be"))
                {
                    element.fontSize = cardModel.visualData.fontSizeRU;
                    element.characterSpacing = cardModel.visualData.characterSpacingRU;
                    element.fontStyle = FontStyles.Bold;
                }
                else
                {
                    element.fontSize = cardModel.visualData.fontSizeEN;
                    element.characterSpacing = cardModel.visualData.characterSpacingEN;
                    element.fontStyle = FontStyles.Normal;
                }
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

    // public static void LocalizeGameObjectElements(GameObject root, (string elementName, string key)[] mappings, string tableName)
    // {
    //     foreach (var (elementName, key) in mappings)
    //     {
    //         LocalizeGameObjectElement(root, elementName, key, tableName);
    //     }
    // }

    public static void LocalizeNotification(NotificationKey key, NotificationType type, params object[] arguments)
    {
        string text = GetNotificationText(key, arguments);
        NotificationManager.ShowNotification(text, type);
    }

    public static string GetNotificationText(NotificationKey key, params object[] arguments)
    {
        string tableName = "NotificationsText";
        string keyName = key.ToString();
        LocalizedString localizedText = new(tableName, keyName) { Arguments = arguments };
        return localizedText.GetLocalizedString();
    }

    public static string GetLocalizedText(string keyName, params object[] arguments)
    {
        string tableName = "ElementsText";
        LocalizedString localizedText = new(tableName, keyName) { Arguments = arguments };
        return localizedText.GetLocalizedString();
    }

    public static void LocalizeChoices(RadioButtonGroup group, string[] keys)
    {
        string tableName = "ElementsText";

        void updateAction(Locale locale)
        {
            List<string> choices = keys.Select(key => new LocalizedString(tableName, key).GetLocalizedString()).ToList();
            group.choices = choices;
        }

        LocalizationSettings.SelectedLocaleChanged += updateAction;

        updateAction(LocalizationSettings.SelectedLocale);

        group.RegisterCallback<DetachFromPanelEvent>(evt =>
        {
            LocalizationSettings.SelectedLocaleChanged -= updateAction;
        });
    }
}