
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

using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
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

    private VisualElement filterPanel;
    private VisualElement handle;
    private bool isOpen = false;
    private float hidden = 250f;
    private float shown = 0f;

    private async void Start()
    {
        root = uiDocument.rootVisualElement;

        root.Q<VisualElement>("loadingOverlay").style.display = DisplayStyle.Flex;

        CardControllerFactory.Init(template: cardTemplate);

        ScrollView cardsContainer = root.Q<ScrollView>("cardScrollView");
        collectionCardListController = new CollectionCardListController(cardsContainer);

        VisualElement cardOverlay = root.Q<VisualElement>("overlay");
        CardOverlayManager.Instance.RegisterOverlayVisualElement(SceneManager.GetActiveScene().name, cardOverlay);

        UserProfileView.Instance.SetUIDocument(uiDocument);
        UserProfileView.Instance.UpdateView(UserProfileView.Instance.GetCachedProfile());

        filterPanelView = new FilterPanelView(root);
        filterPanelView.OnFilterChanged += (activeFilters, pokemonElements) =>
        {
            collectionCardListController.ApplyElementFilter(activeFilters, cardsContainer, pokemonElements);
        };

        await collectionCardListController.AddUserCardsToScrollView();

        await WaitUntilCardsLoaded(cardsContainer, CardRepository.Instance.GetUserCards().cards.Count);

        filterPanel = root.Q<VisualElement>("elementsFilterPanel");
        handle = root.Q<VisualElement>("handle");

        root.Q<VisualElement>("loadingOverlay").style.display = DisplayStyle.None;

        RegisterCallbacks();
    }

    private async Task WaitUntilCardsLoaded(ScrollView cardsContainer, int expectedCount)
    {
        while (cardsContainer.childCount < expectedCount)
        {
            await Task.Yield();
        }
    }

    private void RegisterCallbacks()
    {
        root.Q<Button>("playButton").RegisterCallback<ClickEvent>(evt =>
        {
            SceneSwitcher.SwitchScene("StartPlayScene", root);
        });

        root.Q<Button>("bestiaryButton").RegisterCallback<ClickEvent>(evt =>
        {
            SceneContext.PreviousSceneName = SceneManager.GetActiveScene().name;
            SceneSwitcher.SwitchScene("BestiaryScene", root);
        });

        root.Q<Button>("decksButton").RegisterCallback<ClickEvent>(evt =>
        {
            SceneContext.PreviousSceneName = SceneManager.GetActiveScene().name;
            SceneSwitcher.SwitchScene("DecksScene", root);
        });

        handle.RegisterCallback<ClickEvent>(evt =>
        {
            isOpen = !isOpen;
            float targetWidth = isOpen ? hidden : shown;

            DOTween.To(
                () => filterPanel.resolvedStyle.width,
                w => filterPanel.style.width = w,
                targetWidth,
                0.3f).SetEase(Ease.InOutQuad);
        });
    }
}
