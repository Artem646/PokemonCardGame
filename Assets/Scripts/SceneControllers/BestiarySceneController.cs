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
        CardControllerFactory.Init(template: cardTemplate);

        root = uiDocument.rootVisualElement;
        ScrollView cardsContainer = root.Q<ScrollView>("cardScrollView");

        VisualElement overlay = root.Q<VisualElement>("overlay");
        CardOverlayManager.Instance.RegisterOverlayVisualElement(SceneManager.GetActiveScene().name, overlay);

        bestiaryCardsListController = new CollectionCardListController(cardsContainer);
        filterPanelView = new FilterPanelView(root);

        filterPanelView.OnFilterChanged += (activefilters, pokemonElements) =>
        {
            bestiaryCardsListController.ApplyElementFilter(activefilters, cardsContainer, pokemonElements);
        };

        await bestiaryCardsListController.AddGameCardsToScrollView();

        RegisterCallbacks();
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
