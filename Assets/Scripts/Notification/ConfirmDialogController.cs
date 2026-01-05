using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UIElements;

public class ConfirmDialogController : MonoBehaviour
{
    public static ConfirmDialogController Instance { get; private set; }

    [SerializeField] private UIDocument uiDocument;

    private VisualElement root;
    private VisualElement overlay;
    private VisualElement dialog;
    private Button retryButton;
    private Button exitGameButton;
    private Button openBestiaryButton;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeUI();

        ApplySavedLocale();
        LocalizeElements();

        overlay.style.display = DisplayStyle.None;
    }

    private void InitializeUI()
    {
        root = uiDocument.rootVisualElement;
        overlay = root.Q<VisualElement>("overlay");
        dialog = root.Q<VisualElement>("dialogRoot");
        retryButton = root.Q<Button>("retryButton");
        exitGameButton = root.Q<Button>("exitGameButton");
        openBestiaryButton = root.Q<Button>("openBestiaryButton");
    }

    private void LocalizeElements()
    {
        Localizer.LocalizeElements(root, new[]
        {
            ("dialogMessageLabel", "DialogMessageLabel"),
            ("retryButton", "RetryButton"),
            ("openBestiaryButton", "OpenBestiaryButton"),
            ("exitGameButton", "ExitGameButton")
        }, "ElementsText");
    }

    public static void ShowDialog(Action onRetry, Action onOpenBestiary, Action onExitGame)
    {
        Instance.DisplayDialog(onRetry, onOpenBestiary, onExitGame);
    }

    private void DisplayDialog(Action onRetry, Action onOpenBestiary, Action onExitGame)
    {
        overlay.style.display = DisplayStyle.Flex;
        dialog.style.display = DisplayStyle.Flex;

        retryButton.clicked -= RetryClicked;
        openBestiaryButton.clicked -= OpenBestiaryClicked;
        exitGameButton.clicked -= ExitGameClicked;

        void RetryClicked()
        {
            dialog.style.display = DisplayStyle.None;
            onRetry?.Invoke();
        }
        void OpenBestiaryClicked() => onOpenBestiary?.Invoke();
        void ExitGameClicked() => onExitGame?.Invoke();

        retryButton.clicked += RetryClicked;
        openBestiaryButton.clicked += OpenBestiaryClicked;
        exitGameButton.clicked += ExitGameClicked;
    }

    public static void CloseDialogOverlay()
    {
        Instance.overlay.style.display = DisplayStyle.None;
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
}
