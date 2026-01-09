using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class CollectionSceneController : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private VisualTreeAsset cardTemplate;
    [SerializeField] private SettingsController settingsController;

    private VisualElement root;
    private VisualElement loadingOverlay;
    private VisualElement cardOverlay;
    private ScrollView cardsContainer;
    private VisualElement filterPanel;
    private VisualElement openFilterPanelButton;
    private VisualElement profileField;

    private CollectionCardListController collectionCardListController;
    private FilterPanelView filterPanelView;

    private bool isOpen = false;
    private float hidden = 250f;
    private float shown = 0f;

    private async void Start()
    {
        InitializeUI();

        LocalizeElements();

        loadingOverlay.style.display = DisplayStyle.Flex;

        CardControllerFactory.Init(template: cardTemplate);

        collectionCardListController = new CollectionCardListController(cardsContainer);

        CardOverlayManager.Instance.RegisterCardOverlay(SceneManager.GetActiveScene().name, cardOverlay);

        UserProfileView.Instance.SetUIDocument(uiDocument, settingsController);
        await UserProfileView.Instance.LoadUserData();

        filterPanelView = new FilterPanelView(root);
        filterPanelView.OnFilterChanged += (activeFilters, pokemonElements) =>
        {
            collectionCardListController.ApplyElementFilter(activeFilters, cardsContainer, pokemonElements);
        };

        await collectionCardListController.AddUserCardsToScrollView();

        await WaitUntilCardsLoaded(cardsContainer, CardRepository.Instance.GetUserCards().cards.Count);

        loadingOverlay.style.display = DisplayStyle.None;

        RegisterCallbacks();
    }

    private void InitializeUI()
    {
        root = uiDocument.rootVisualElement;
        loadingOverlay = root.Q<VisualElement>("loadingOverlay");
        cardsContainer = root.Q<ScrollView>("cardScrollView");
        cardOverlay = root.Q<VisualElement>("overlay");
        filterPanel = root.Q<VisualElement>("elementsFilterPanel");
        openFilterPanelButton = root.Q<VisualElement>("openFiltersButton");
        profileField = root.Q<VisualElement>("profileField");
    }

    private void LocalizeElements()
    {
        Localizer.LocalizeElements(root, new[]
        {
            ("playButton", "PlayButton"),
            ("collectionButton", "CollectionButton"),
            ("bestiaryButton", "BestiaryButton"),
            ("decksButton", "DecksButton"),
            ("filtersLabel", "FiltersLabel")
        }, "ElementsText");
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
            SceneContext.PreviousMenuSceneName = SceneManager.GetActiveScene().name;
            SceneSwitcher.SwitchScene("BestiaryScene", root);
        });

        root.Q<Button>("decksButton").RegisterCallback<ClickEvent>(evt =>
        {
            SceneSwitcher.SwitchScene("DecksScene", root);
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

        profileField.RegisterCallback<ClickEvent>(evt =>
        {
            settingsController.OpenSettings();
        });
    }
}
