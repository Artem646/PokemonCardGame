using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class CollectionSceneController : MonoBehaviour, IRefreshableScene
{
    [SerializeField] private UIDocument uiDocument;

    private VisualElement root;
    private VisualElement cardOverlay;
    private VisualElement cardsContainer;
    private VisualElement filterPanel;
    private VisualElement openFilterPanelButton;
    private VisualElement fadeOverlay;

    private Tween fadeTween;

    private List<CollectionCardController> userCardControllers = new();

    private FilterPanelView filterPanelView;
    private List<PokemonElement> activeFiltersCache = new();

    private bool isOpen = false;
    private const float HIDDEN = -250f;
    private const float SHOWN = 0f;

    private void Start()
    {
        InitializeUI();
        FadeIn();
        CardOverlayManager.Instance.RegisterCardOverlay(SceneManager.GetActiveScene().name, cardOverlay);

        UserCardModelList userCards = CardRepository.Instance.GetUserCardsList();
        List<int> userCardsIds = userCards.cards.Select(c => c.id).ToList();
        userCardControllers = CardRepository.Instance.GetGameCollectionCardControllersList()
            .Where(cardController => userCardsIds.Contains(cardController.CardModel.id)).ToList();

        filterPanelView = new FilterPanelView(root);
        filterPanelView.OnFilterChanged += (activeFilters, pokemonElements) =>
        {
            activeFiltersCache = activeFilters;
            ApplyElementFilter(activeFilters);
        };

        ApplyElementFilter(activeFiltersCache);
        RegisterCallbacks();
    }

    private void InitializeUI()
    {
        root = uiDocument.rootVisualElement;
        cardOverlay = root.Q<VisualElement>("overlay");
        cardsContainer = root.Q<VisualElement>("cardsContainer");
        filterPanel = root.Q<VisualElement>("filterPanel");
        openFilterPanelButton = root.Q<VisualElement>("openFiltersButton");
        fadeOverlay = root.Q<VisualElement>("fadeOverlay");
    }

    private void FadeIn()
    {
        fadeOverlay.style.opacity = 1f;

        fadeTween?.Kill();
        fadeTween = DOTween.To(
            () => fadeOverlay.style.opacity.value,
            x => fadeOverlay.style.opacity = x,
            0f, 0.6f
        ).SetEase(Ease.OutQuad);

        fadeOverlay.style.display = DisplayStyle.None;
    }

    private void FillContainerWithCards(List<CollectionCardController> cardControllersList)
    {
        cardsContainer.Clear();
        foreach (CollectionCardController cardController in cardControllersList)
        {
            ICollectionCardView cardView = cardController.CollectionCardView;
            cardView.ApplyCardStyleForActiveScene("CollectionScene");
            cardsContainer.Add(cardView.CardRoot);
        }
    }

    public void RefreshSceneContent() => ApplyElementFilter(activeFiltersCache);

    public void ApplyElementFilter(List<PokemonElement> activeFilters)
    {
        List<CollectionCardController> filteredCardControllers = new();

        if (activeFilters == null || activeFilters.Count == 0)
            filteredCardControllers = userCardControllers;
        else
        {
            filteredCardControllers = userCardControllers.Where(cardController =>
                activeFilters.Contains(cardController.CardModel.mainElement) ||
                (cardController.CardModel.secondaryElement.HasValue &&
                activeFilters.Contains(cardController.CardModel.secondaryElement.Value))
            ).ToList();
        }

        FillContainerWithCards(filteredCardControllers);
    }

    private void RegisterCallbacks()
    {
        openFilterPanelButton.RegisterCallback<ClickEvent>(evt =>
        {
            isOpen = !isOpen;
            float targetMargin = isOpen ? SHOWN : HIDDEN;

            DOTween.To(
                () => filterPanel.resolvedStyle.marginLeft,
                x => filterPanel.style.marginLeft = x,
                targetMargin, 0.5f)
                .SetEase(Ease.OutQuad);
        });
    }
}
