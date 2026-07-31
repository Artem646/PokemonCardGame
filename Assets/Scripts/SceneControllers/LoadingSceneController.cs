using DG.Tweening;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UIElements;
using System.Collections;
using System.Threading.Tasks;

public class LoadingSceneController : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private VisualTreeAsset cardTemplate;

    private VisualElement root;
    private Label tapOnScreenLabel;
    private VisualElement spinner;
    private Tween blinkTextTween;
    private Tween spinnerRotateTween;
    private Tween spinnerScaleTween;

    private bool isReadyToTap = false;

    private void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
        ApplySavedLocale();
        ApplySavedTheme();
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

    private void ApplySavedTheme()
    {
        ThemeStyleSheet darkTheme = Resources.Load<ThemeStyleSheet>("Themes/DarkTheme");
        ThemeStyleSheet lightTheme = Resources.Load<ThemeStyleSheet>("Themes/LightTheme");

        string currentThemeString = PlayerPrefs.GetString("SelectedTheme", "Dark");
        switch (currentThemeString)
        {
            case "Dark":
                uiDocument.panelSettings.themeStyleSheet = darkTheme;
                break;
            case "Light":
                uiDocument.panelSettings.themeStyleSheet = lightTheme;
                break;
        }
    }

    private void Start()
    {
        InitializeUI();
        LocalizeElements();
        StartCoroutine(LoadingRoutine());
    }

    private void InitializeUI()
    {
        root = uiDocument.rootVisualElement;
        tapOnScreenLabel = root.Q<Label>("tapOnScreenLabel");
        spinner = root.Q<VisualElement>("loadingSpinner");
    }

    private void LocalizeElements()
    {
        Localizer.LocalizeElement(root, "loadingLabel", "LoadingLabel", "ElementsText");
        Localizer.LocalizeElement(root, "tapOnScreenLabel", "TapOnScreenLabel", "ElementsText");
    }

    private IEnumerator LoadingRoutine()
    {
        StartSpinnerAnimation();

        Task loadCardsTask = CardRepository.Instance.LoadGameCards();
        yield return new WaitForSeconds(2);
        yield return new WaitUntil(() => loadCardsTask.IsCompleted);

        CardRepository.Instance.PreBuildGameCollectionCardControllers(cardTemplate);

        StopSpinnerAnimation();

        spinner.style.display = DisplayStyle.None;
        tapOnScreenLabel.style.display = DisplayStyle.Flex;

        StartBlinkTextAnimation();

        isReadyToTap = true;

        root.RegisterCallback<PointerDownEvent>(OnScreenTap);
    }

    private void StartSpinnerAnimation()
    {
        spinner.style.rotate = new Rotate(0);

        spinnerRotateTween = DOTween.To(
            () => spinner.resolvedStyle.rotate.angle.value,
            x => spinner.style.rotate = new Rotate(x),
            360f, 1.5f
        )
        .SetEase(Ease.Linear)
        .SetLoops(-1, LoopType.Restart);

        spinnerScaleTween = DOTween.To(
            () => spinner.resolvedStyle.scale.value.x,
            x => spinner.style.scale = new Scale(new Vector3(x, x, 1f)),
            1.15f, 1.2f
        )
        .SetEase(Ease.Linear)
        .SetLoops(-1, LoopType.Yoyo);
    }

    private void StopSpinnerAnimation()
    {
        spinnerRotateTween?.Kill();
        spinnerScaleTween?.Kill();
    }

    private void StartBlinkTextAnimation()
    {
        tapOnScreenLabel.style.opacity = 1f;

        blinkTextTween = DOTween.To(
            () => tapOnScreenLabel.resolvedStyle.opacity,
            x => tapOnScreenLabel.style.opacity = x,
            0.2f, 1f
        )
        .SetLoops(-1, LoopType.Yoyo)
        .SetEase(Ease.InOutQuad);
    }

    private void OnScreenTap(PointerDownEvent evt)
    {
        if (!isReadyToTap) return;

        blinkTextTween?.Kill();
        tapOnScreenLabel.style.opacity = 1f;

        InternetChecker.Instance.VerifyInternetConnection();
    }
};
