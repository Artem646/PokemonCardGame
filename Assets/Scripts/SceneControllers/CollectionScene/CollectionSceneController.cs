
//         // string photoUrl = FirebaseService.Instance.GetAuth().CurrentUser.PhotoUrl?.ToString();
//         // StartCoroutine(LoadUserImage(photoUrl));

//     // private IEnumerator LoadUserImage(string url)
//     // {
//     //     UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
//     //     yield return request.SendWebRequest();

//     //     if (request.result != UnityWebRequest.Result.Success)
//     //     {
//     //         Debug.LogError("[P][ImageLoader] Ошибка загрузки изображения: " + request.error);
//     //         yield break;
//     //     }

//     //     Texture2D texture = DownloadHandlerTexture.GetContent(request);
//     //     userImage.style.backgroundImage = new StyleBackground(texture);

//     //     Debug.Log("[P][ImageLoader] Изображение пользователя установлено.");
//     // }

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class CollectionSceneController : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private VisualTreeAsset cardTemplate;

    private VisualElement root;
    private CollectionCardsListController сollectionCardsListController;
    private FilterPanelView filterPanelView;

    private async void Start()
    {
        root = uiDocument.rootVisualElement;

        ScrollView cardsContainer = root.Q<ScrollView>("cardScrollView");
        сollectionCardsListController = new CollectionCardsListController(cardsContainer);

        VisualElement overlay = root.Q<VisualElement>("overlay");
        CardOverlayManager.Instance.Init(overlay);

        UserProfileView.Instance.SetUIDocument(uiDocument);
        UserProfileView.Instance.UpdateView(UserProfileView.Instance.GetCachedProfile());

        filterPanelView = new FilterPanelView(root);
        filterPanelView.OnFilterChanged += сollectionCardsListController.ApplyElementFilter;

        CardControllerBuilder.SetDefaultTemplate(cardTemplate);
        await сollectionCardsListController.LoadUserCardsToScrollView();

        // UserCardsModelList userCards = CardRepository.Instance.GetUserCards();

        RegisterCallbacks();
    }

    private void RegisterCallbacks()
    {
        root.Q<Button>("playButton").RegisterCallback<ClickEvent>(evt =>
        {
            SceneManager.LoadScene("PlayScene");
        });

        root.Q<Button>("bestiaryButton").RegisterCallback<ClickEvent>(evt =>
        {
            SceneContext.PreviousSceneName = SceneManager.GetActiveScene().name;
            SceneManager.LoadScene("BestiaryScene");
        });
    }
}
