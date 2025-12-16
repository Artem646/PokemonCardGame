
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
    private CollectionCardListController collectionCardListController;
    private FilterPanelView filterPanelView;

    private async void Start()
    {
        root = uiDocument.rootVisualElement;

        CardControllerFactory.Init(template: cardTemplate);

        ScrollView cardsContainer = root.Q<ScrollView>("cardScrollView");
        collectionCardListController = new CollectionCardListController(cardsContainer);

        VisualElement overlay = root.Q<VisualElement>("overlay");
        CardOverlayManager.Instance.RegisterOverlayVisualElement(SceneManager.GetActiveScene().name, overlay);

        UserProfileView.Instance.SetUIDocument(uiDocument);
        UserProfileView.Instance.UpdateView(UserProfileView.Instance.GetCachedProfile());

        filterPanelView = new FilterPanelView(root);
        filterPanelView.OnFilterChanged += (activeFilters, pokemonElements) =>
        {
            collectionCardListController.ApplyElementFilter(activeFilters, cardsContainer, pokemonElements);
        };

        await collectionCardListController.AddUserCardsToScrollView();

        RegisterCallbacks();
    }

    private void RegisterCallbacks()
    {
        root.Q<Button>("playButton").RegisterCallback<ClickEvent>(evt =>
        {
            SceneSwitcher.SwitchScene("DeckSelectionScene", root);
        });

        root.Q<Button>("bestiaryButton").RegisterCallback<ClickEvent>(evt =>
        {
            SceneContext.PreviousSceneName = SceneManager.GetActiveScene().name;
            SceneSwitcher.SwitchScene("BestiaryScene", root);
        });
    }
}
