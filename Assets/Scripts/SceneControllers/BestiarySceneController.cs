using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public static class SceneContext
{
    public static string PreviousSceneName;
}

public class BestiarySceneController : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private VisualTreeAsset cardTemplate;

    private VisualElement root;
    private CollectionCardListController bestiaryCardsListController;
    private FilterPanelView filterPanelView;

    private async void Start()
    {
        root = uiDocument.rootVisualElement;

        root.Q<VisualElement>("loadingOverlay").style.display = DisplayStyle.Flex;

        CardControllerFactory.Init(template: cardTemplate);

        ScrollView cardsContainer = root.Q<ScrollView>("cardScrollView");
        bestiaryCardsListController = new CollectionCardListController(cardsContainer);

        VisualElement overlay = root.Q<VisualElement>("overlay");
        CardOverlayManager.Instance.RegisterOverlayVisualElement(SceneManager.GetActiveScene().name, overlay);

        filterPanelView = new FilterPanelView(root);

        filterPanelView.OnFilterChanged += (activefilters, pokemonElements) =>
        {
            bestiaryCardsListController.ApplyElementFilter(activefilters, cardsContainer, pokemonElements);
        };

        await bestiaryCardsListController.AddGameCardsToScrollView();

        await WaitUntilCardsLoaded(cardsContainer, CardRepository.Instance.GetGameCards().cards.Count);

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
        root.Q<Button>("exitButton").RegisterCallback<ClickEvent>(evt =>
        {
            if (SceneContext.PreviousSceneName == "LoadingScene")
                SceneManager.LoadScene(SceneContext.PreviousSceneName);
            else
                SceneSwitcher.SwitchScene(SceneContext.PreviousSceneName, root);
        });
    }
}
