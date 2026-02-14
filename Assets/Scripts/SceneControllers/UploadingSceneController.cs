using System.Collections;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class UploadingSceneController : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;

    private VisualElement root;
    private Label uploadingLabel;
    private ProgressBar progressBar;
    private VisualElement fadeOverlay;

    private string baseUploadingText;

    private Tween progressTween;
    private Tween fadeTween;
    private Sequence dotsSequence;

    private async void Start()
    {
        InitializeUI();

        await Task.Delay(100);

        StartDotsAnimation();

        await Task.Delay(2000);

        User user = await FirebaseFirestoreService.Instance.CreateOrUpdateUserDocument(FirebaseAuthService.Instance.GetAuth().CurrentUser);
        UserSession.Instance.ActiveUser = user;

        UserProfileData profile = await UserProfileService.Instance.GetUserProfile();
        UserProfileView.Instance.PreloadData(profile);

        CardRepository.Instance.OnProgressChanged += HandleProgress;
        CardRepository.Instance.OnCardsLoaded += HandleCardsLoaded;

        await CardRepository.Instance.GetUserCardsCollection();
    }

    private void InitializeUI()
    {
        root = uiDocument.rootVisualElement;
        progressBar = root.Q<ProgressBar>("progressBar");
        uploadingLabel = root.Q<Label>("uploadingLabel");
        fadeOverlay = root.Q<VisualElement>("fadeOverlay");
    }

    private void StartDotsAnimation()
    {
        baseUploadingText = uploadingLabel.text;

        dotsSequence = DOTween.Sequence();
        dotsSequence.AppendCallback(() => uploadingLabel.text = baseUploadingText).AppendInterval(0.5f)
        .AppendCallback(() => uploadingLabel.text = baseUploadingText + ".").AppendInterval(0.5f)
        .AppendCallback(() => uploadingLabel.text = baseUploadingText + "..").AppendInterval(0.5f)
        .AppendCallback(() => uploadingLabel.text = baseUploadingText + "...").AppendInterval(0.5f)
        .SetLoops(-1, LoopType.Restart);
    }

    private void HandleProgress(float progress)
    {
        dotsSequence?.Kill();

        float targetValue = progress * 100f;

        progressTween?.Kill();
        progressTween = DOTween.To(
            () => progressBar.value,
            x => progressBar.value = x,
            targetValue, 0.35f
        ).SetEase(Ease.OutQuad);

        Localizer.LocalizeElement(root, "uploadingLabel", "UploadingCardsLabel", "ElementsText", Mathf.RoundToInt(targetValue));
    }

    private void HandleCardsLoaded()
    {
        progressTween?.Kill();
        progressBar.value = 100f;

        Localizer.LocalizeElement(root, "uploadingLabel", "CardsUploadedLabel", "ElementsText", CardRepository.Instance.GetUserCards().cards.Count);

        StartCoroutine(FadeOutAndLoadScene("CollectionScene"));
    }

    private IEnumerator FadeOutAndLoadScene(string sceneName)
    {
        yield return new WaitForSeconds(0.5f);

        fadeTween = DOTween.To(
            () => fadeOverlay.resolvedStyle.opacity,
            x => fadeOverlay.style.opacity = x,
            1f, 0.6f
        ).SetEase(Ease.InQuad);

        yield return fadeTween.WaitForCompletion();

        SceneManager.LoadScene(sceneName);
    }
}
