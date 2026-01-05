using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class LoadingSceneController : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    private VisualElement root;

    private bool cardsLoaded = false;

    void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 90;
        ApplySavedLocale();
    }

    private async void Start()
    {
        InitializeUI();

        LocalizeElements();

        if (!cardsLoaded)
        {
            await CardRepository.Instance.GetAllGameCards();
            cardsLoaded = true;
        }

        InternetChecker.Instance.VerifyInternetConnection();
    }

    private void InitializeUI()
    {
        root = uiDocument.rootVisualElement;
    }

    private void LocalizeElements()
    {
        Localizer.LocalizeElement(root, "loadingLabel", "LoadingLabel", "ElementsText");
    }

    private void ApplySavedLocale()
    {
        if (PlayerPrefs.HasKey("locale"))
        {
            List<Locale> locales = LocalizationSettings.AvailableLocales.Locales;
            string savedCode = PlayerPrefs.GetString("locale");
            Locale locale = locales.Find(locale => locale.Identifier.Code == savedCode);
            LocalizationSettings.SelectedLocale = locale;
        }
        else
        {
            Locale locale = LocalizationSettings.AvailableLocales.GetLocale("en");
            LocalizationSettings.SelectedLocale = locale;
        }
    }
};
