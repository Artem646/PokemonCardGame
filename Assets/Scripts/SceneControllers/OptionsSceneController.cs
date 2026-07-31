using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization;

public class OptionsSceneController : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;

    private VisualElement root;
    private RadioButtonGroup languageGroup;
    private RadioButtonGroup themeGroup;

    private ThemeStyleSheet darkTheme;
    private ThemeStyleSheet lightTheme;

    private void Start()
    {
        InitializeUI();
        languageGroup.RegisterValueChangedCallback(OnLanguageChanged);
        themeGroup.RegisterValueChangedCallback(OnThemeChanged);
        SyncLanguageGroupWithCurrentLocale();
        SyncThemeGroupWithCurrentTheme();
        RegisterCallbacks();
    }

    private void InitializeUI()
    {
        root = uiDocument.rootVisualElement;
        languageGroup = root.Q<RadioButtonGroup>("languageGroup");
        themeGroup = root.Q<RadioButtonGroup>("themeGroup");
    }

    private void OnLanguageChanged(ChangeEvent<int> evt)
    {
        int selectedIndex = evt.newValue;
        string selectedCode = selectedIndex switch
        {
            0 => "ru",
            1 => "en",
            2 => "be",
            _ => "en"
        };

        SetLocale(selectedCode);
    }

    private void SetLocale(string code)
    {
        Locale locale = LocalizationSettings.AvailableLocales.GetLocale(code);
        LocalizationSettings.SelectedLocale = locale;
    }

    private void SyncLanguageGroupWithCurrentLocale()
    {
        Locale currentLocale = LocalizationSettings.SelectedLocale;
        switch (currentLocale.Identifier.Code)
        {
            case "ru":
                languageGroup.value = 0;
                break;
            case "en":
                languageGroup.value = 1;
                break;
            case "be":
                languageGroup.value = 2;
                break;
        }
    }

    private void LoadThemesIfNeeded()
    {
        if (darkTheme == null) darkTheme = Resources.Load<ThemeStyleSheet>("Themes/DarkTheme");
        if (lightTheme == null) lightTheme = Resources.Load<ThemeStyleSheet>("Themes/LightTheme");
    }

    private void OnThemeChanged(ChangeEvent<int> evt)
    {
        LoadThemesIfNeeded();

        int selectedIndex = evt.newValue;
        string selectedThemeString = selectedIndex switch
        {
            0 => "Dark",
            1 => "Light",
            _ => "Dark"
        };

        switch (selectedThemeString)
        {
            case "Dark":
                uiDocument.panelSettings.themeStyleSheet = darkTheme;
                break;
            case "Light":
                uiDocument.panelSettings.themeStyleSheet = lightTheme;
                break;
        }

        PlayerPrefs.SetString("SelectedTheme", selectedThemeString);
        PlayerPrefs.Save();
    }

    public void SyncThemeGroupWithCurrentTheme()
    {
        string currentThemeString = PlayerPrefs.GetString("SelectedTheme", "Dark");
        switch (currentThemeString)
        {
            case "Dark":
                themeGroup.value = 0;
                break;
            case "Light":
                themeGroup.value = 1;
                break;
        }
    }

    private void RegisterCallbacks() { }
}
