using UnityEngine;
using UnityEngine.SceneManagement;
using Firebase.Auth;
using UnityEngine.UIElements;

public class UploadingSceneController : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    private ProgressBar progressBar;
    private Label statusLabel;

    private async void Start()
    {
        FirebaseAuth auth = FirebaseService.Instance.GetAuth();
        string userId = auth.CurrentUser.UserId;
        await CardRepository.Instance.GetUserCardsCollection(userId);
    }
    private void OnEnable()
    {
        var root = uiDocument.rootVisualElement;
        progressBar = root.Q<ProgressBar>("progressBar");
        statusLabel = root.Q<Label>("statusLabel");

        CardRepository.Instance.OnProgressChanged += HandleProgress;
        CardRepository.Instance.OnCardsLoaded += HandleCardsLoaded;
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
        statusLabel.text = $"Загрузка карт... {Mathf.RoundToInt(progress * 100)}%";
    }

    private void HandleCardsLoaded()
    {
        statusLabel.text = $"Загружено {CardRepository.Instance.GetUserCards().cards.Count} карт";
        SceneManager.LoadScene("CollectionScene");
    }
}
