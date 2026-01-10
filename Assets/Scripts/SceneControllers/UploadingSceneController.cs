using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class UploadingSceneController : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;

    private VisualElement root;
    private ProgressBar progressBar;

    private async void Start()
    {
        User user = await FirebaseFirestoreService.Instance.CreateOrUpdateUserDocument(FirebaseAuthService.Instance.GetAuth().CurrentUser);
        UserSession.Instance.ActiveUser = user;

        UserProfileData profile = await UserProfileService.Instance.GetUserProfile();
        UserProfileView.Instance.PreloadData(profile);

        await CardRepository.Instance.GetUserCardsCollection();
    }

    private void OnEnable()
    {
        InitializeUI();

        CardRepository.Instance.OnProgressChanged += HandleProgress;
        CardRepository.Instance.OnCardsLoaded += HandleCardsLoaded;
    }

    private void InitializeUI()
    {
        root = uiDocument.rootVisualElement;
        progressBar = root.Q<ProgressBar>("progressBar");
    }

    private void OnDisable()
    {
        if (CardRepository.Instance != null)
        {
            CardRepository.Instance.OnProgressChanged -= HandleProgress;
            CardRepository.Instance.OnCardsLoaded -= HandleCardsLoaded;
        }
    }

    private void HandleProgress(float progress)
    {
        progressBar.value = progress * 100f;
        Localizer.LocalizeElement(root, "uploadingLabel", "UploadingCardsLabel", "ElementsText", Mathf.RoundToInt(progress * 100));
    }

    private void HandleCardsLoaded()
    {
        Localizer.LocalizeElement(root, "uploadingLabel", "CardsUploadedLabel", "ElementsText", CardRepository.Instance.GetUserCards().cards.Count);
        SceneManager.LoadScene("CollectionScene");
    }
}
