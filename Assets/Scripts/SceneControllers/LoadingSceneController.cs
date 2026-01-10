using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UIElements;

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
            string savedCode = PlayerPrefs.GetString("locale");
            Locale locale = LocalizationSettings.AvailableLocales.GetLocale(savedCode);
            LocalizationSettings.SelectedLocale = locale;
        }
    }
};
