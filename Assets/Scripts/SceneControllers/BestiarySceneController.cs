using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public static class SceneContext
{
    public static string PreviousMenuSceneName { get; set; }
    public static string PreviousDescriptionSceneName { get; set; }
}

public class BestiarySceneController : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private VisualTreeAsset cardTemplate;

    private VisualElement root;
    private VisualElement loadingOverlay;
    private VisualElement cardOverlay;
    private ScrollView cardsContainer;
    private VisualElement filterPanel;
    private VisualElement openFilterPanelButton;

    private CollectionCardListController bestiaryCardsListController;
    private FilterPanelView filterPanelView;

    private bool isOpen = false;
    private float hidden = 250f;
    private float shown = 0f;

    private async void Start()
    {
        InitializeUI();

        loadingOverlay.style.display = DisplayStyle.Flex;

        CardControllerFactory.Init(template: cardTemplate);

        bestiaryCardsListController = new CollectionCardListController(cardsContainer);

        CardOverlayManager.Instance.RegisterCardOverlay(SceneManager.GetActiveScene().name, cardOverlay);

        filterPanelView = new FilterPanelView(root);
        filterPanelView.OnFilterChanged += (activefilters, pokemonElements) =>
        {
            bestiaryCardsListController.ApplyElementFilter(activefilters, cardsContainer, pokemonElements);
        };

        await bestiaryCardsListController.AddGameCardsToScrollView();

        await WaitUntilCardsLoaded(cardsContainer, CardRepository.Instance.GetGameCards().cards.Count);

        loadingOverlay.style.display = DisplayStyle.None;

        RegisterCallbacks();
    }

    private void InitializeUI()
    {
        root = uiDocument.rootVisualElement;
        loadingOverlay = root.Q<VisualElement>("loadingOverlay");
        cardOverlay = root.Q<VisualElement>("overlay");
        cardsContainer = root.Q<ScrollView>("cardScrollView");
        filterPanel = root.Q<VisualElement>("elementsFilterPanel");
        openFilterPanelButton = root.Q<VisualElement>("openFiltersButton");
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
        root.Q<Button>("exitButton").RegisterCallback<ClickEvent>(evt =>
        {
            if (SceneContext.PreviousMenuSceneName == "LoadingScene")
                SceneManager.LoadScene(SceneContext.PreviousMenuSceneName);
            else
                SceneSwitcher.SwitchScene(SceneContext.PreviousMenuSceneName, root);
        });

        openFilterPanelButton.RegisterCallback<ClickEvent>(evt =>
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
