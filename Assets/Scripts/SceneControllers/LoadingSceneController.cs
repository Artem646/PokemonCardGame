using DG.Tweening;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UIElements;

public class LoadingSceneController : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    private VisualElement root;
    private Label tapOnScreenLabel;
    private VisualElement spinner;
    private Tween blinkTextTween;
    private Tween spinnerRotateTween;
    private Tween spinnerScaleTween;

    private bool cardsLoaded = false;

    private void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 90;
        ApplySavedLocale();
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

    private async void Start()
    {
        InitializeUI();

        LocalizeElements();

        StartBlinkTextAnimation();
        StartSpinnerAnimation();

        root.RegisterCallback<PointerDownEvent>(OnScreenTap);

        if (!cardsLoaded)
        {
            await CardRepository.Instance.GetAllGameCards();
            cardsLoaded = true;
        }
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

    private void StartBlinkTextAnimation()
    {
        tapOnScreenLabel.style.opacity = 1f;

        blinkTextTween = DOTween.To(
            () => tapOnScreenLabel.resolvedStyle.opacity,
            x => tapOnScreenLabel.style.opacity = x,
            0f, 1f
        )
        .SetLoops(-1, LoopType.Yoyo)
        .SetEase(Ease.InOutQuad);
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
            x => spinner.style.scale = new Scale(new Vector3(x, x, 10f)),
            1.15f, 1.2f
        )
        .SetEase(Ease.Linear)
        .SetLoops(-1, LoopType.Yoyo);
    }

    private void OnScreenTap(PointerDownEvent evt)
    {
        blinkTextTween?.Kill();
        spinnerRotateTween?.Kill();
        spinnerScaleTween?.Kill();

        tapOnScreenLabel.style.opacity = 1f;
        spinner.style.scale = new Scale(new Vector3(1f, 1f, 1f));

        InternetChecker.Instance.VerifyInternetConnection();
    }
};
