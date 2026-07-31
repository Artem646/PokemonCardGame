using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public static class SceneContext
{
    public static string PreviousMenuSceneName { get; set; }
    public static string PreviousDescriptionSceneName { get; set; }
}

public class BestiarySceneController : MonoBehaviour, IRefreshableScene
{
    [SerializeField] private UIDocument uiDocument;

    private VisualElement root;
    private VisualElement cardOverlay;
    private ScrollView album;
    private VisualElement content;
    private Button nextButton;
    private Button prevButton;

    private List<VisualElement> spreads = new();
    private List<CollectionCardController> gameCardControllers;
    private List<int> userCardsIds;

    private FilterPanelView filterPanelView;
    private List<PokemonElement> activeFiltersCache = new();

    private const int CARDS_PER_PAGE = 6;
    private int currentSpreadIndex = 0;

    private bool isAnimating = false;
    public float flipDuration = 0.25f;
    public float maxShadowOpacity = 0.6f;

    private Vector2 swipeStartPosition;
    private bool isSwiping = false;
    private const float SWIPE_THRESHOLD = 100f;

    private void Start()
    {
        InitializeUI();
        CardOverlayManager.Instance.RegisterCardOverlay(SceneManager.GetActiveScene().name, cardOverlay);

        gameCardControllers = CardRepository.Instance.GetGameCollectionCardControllersList();
        UserCardModelList userCards = CardRepository.Instance.GetUserCardsList();
        userCardsIds = userCards.cards.Select(c => c.id).ToList();

        filterPanelView = new FilterPanelView(content);
        filterPanelView.OnFilterChanged += (activeFilters, pokemonElements) =>
        {
            activeFiltersCache = activeFilters;
            currentSpreadIndex = 0;
            ApplyElementFilter(activeFilters);
        };

        ApplyElementFilter(activeFiltersCache);

        InitLogic();
        RegisterCallbacks();
    }

    private void InitializeUI()
    {
        root = uiDocument.rootVisualElement;
        cardOverlay = root.Q<VisualElement>("overlay");
        album = root.Q<ScrollView>("albumScroll");
        content = album.contentContainer;
        nextButton = root.Q<Button>("nextButton");
        prevButton = root.Q<Button>("prevButton");
    }

    private void GenerateAlbum(List<CollectionCardController> cardControllersToShow)
    {
        for (int i = content.childCount - 1; i >= 0; i--)
        {
            if (content[i].name != "coverSpread")
                content[i].RemoveFromHierarchy();
        }

        spreads.Clear();

        VisualElement coverSpread = content.Q<VisualElement>("coverSpread");
        spreads.Add(coverSpread);
        ResetSpreadVisuals(coverSpread);

        VisualElement coverPage = coverSpread.Q<VisualElement>("coverPage");
        AddRingsToPage(coverPage);

        int currentCardIndex = 0;
        int spreadIndex = 1;

        while (currentCardIndex < cardControllersToShow.Count)
        {
            VisualElement spread = new();
            spread.AddToClassList("spread");
            spreads.Add(spread);

            VisualElement leftPage = CreatePage("left-page");
            VisualElement rightPage = CreatePage("right-page");

            int leftNum = (spreadIndex - 1) * 2 + 1;
            int rightNum = leftNum + 1;

            SetPageNumber(leftPage, leftNum.ToString());
            FillPageWithCards(leftPage, ref currentCardIndex, cardControllersToShow);

            SetPageNumber(rightPage, rightNum.ToString());
            FillPageWithCards(rightPage, ref currentCardIndex, cardControllersToShow);

            spread.Add(leftPage);
            spread.Add(rightPage);

            content.Add(spread);

            ResetSpreadVisuals(spread);

            spreadIndex++;
        }

        if (currentSpreadIndex >= spreads.Count)
            currentSpreadIndex = Mathf.Max(0, spreads.Count - 1);

        for (int i = 0; i < spreads.Count; i++)
            spreads[i].style.display = (i == currentSpreadIndex) ? DisplayStyle.Flex : DisplayStyle.None;

        UpdateButtonsVisibility();
    }

    private void InitLogic()
    {
        nextButton.clicked += () => FlipPage(1);
        prevButton.clicked += () => FlipPage(-1);

        album.RegisterCallback<PointerMoveEvent>(evt => evt.StopImmediatePropagation(), TrickleDown.TrickleDown);
        album.RegisterCallback<WheelEvent>(evt => evt.StopImmediatePropagation(), TrickleDown.TrickleDown);

        album.RegisterCallback<PointerDownEvent>(OnPointerDown);
        album.RegisterCallback<PointerUpEvent>(OnPointerUp);
        album.RegisterCallback<PointerCancelEvent>(OnPointerCancel);
    }

    private void FlipPage(int direction)
    {
        if (isAnimating) return;

        int nextIdx = currentSpreadIndex + direction;
        if (nextIdx < 0 || nextIdx >= spreads.Count) return;

        isAnimating = true;

        VisualElement currentSpread = spreads[currentSpreadIndex];
        VisualElement nextSpread = spreads[nextIdx];

        currentSpreadIndex = nextIdx;
        UpdateButtonsVisibility();

        nextSpread.style.display = DisplayStyle.Flex;
        nextSpread.SendToBack();

        if (direction > 0)
        {
            VisualElement pageToFlip = currentSpread.Q(className: "right-page");
            VisualElement pageToReveal = nextSpread.Q(className: "left-page");

            pageToReveal.style.scale = new StyleScale(new Vector2(0, 1));

            AnimatePageWithShadow(pageToFlip, true, flipDuration, () =>
            {
                currentSpread.SendToBack();
                AnimatePageWithShadow(pageToReveal, false, flipDuration, () =>
                {
                    currentSpread.style.display = DisplayStyle.None;
                    ResetSpreadVisuals(currentSpread);
                    isAnimating = false;
                });
            });
        }
        else
        {
            VisualElement pageToFlip = currentSpread.Q(className: "left-page");
            VisualElement pageToReveal = nextSpread.Q(className: "right-page");

            pageToReveal.style.scale = new StyleScale(new Vector2(0, 1));

            AnimatePageWithShadow(pageToFlip, true, flipDuration, () =>
            {
                currentSpread.SendToBack();
                AnimatePageWithShadow(pageToReveal, false, flipDuration, () =>
                {
                    currentSpread.style.display = DisplayStyle.None;
                    ResetSpreadVisuals(currentSpread);
                    isAnimating = false;
                });
            });
        }
    }

    private void AnimatePageWithShadow(VisualElement page, bool isClosing, float duration, Action onComplete)
    {
        VisualElement shadow = page.Q("shadow-overlay") ?? new VisualElement { name = "shadow-overlay" };
        if (shadow.parent == null)
        {
            shadow.AddToClassList("shadow-overlay");
            shadow.pickingMode = PickingMode.Ignore;
            page.Add(shadow);
        }

        float startScale = isClosing ? 1 : 0;
        float endScale = isClosing ? 0 : 1;
        float startOp = isClosing ? 0 : maxShadowOpacity;
        float endOp = isClosing ? maxShadowOpacity : 0;

        DOTween.To(() => 0f, t =>
        {
            page.style.scale = new StyleScale(new Vector2(Mathf.Lerp(startScale, endScale, t), 1));
            shadow.style.opacity = Mathf.Lerp(startOp, endOp, t);
        }, 1f, duration)
        .SetEase(isClosing ? Ease.InQuad : Ease.OutQuad)
        .OnComplete(() => onComplete?.Invoke());
    }

    private void UpdateButtonsVisibility()
    {
        prevButton.style.visibility = (currentSpreadIndex == 0) ? Visibility.Hidden : Visibility.Visible;
        nextButton.style.visibility = (currentSpreadIndex >= spreads.Count - 1) ? Visibility.Hidden : Visibility.Visible;
    }

    private void ResetSpreadVisuals(VisualElement spread)
    {
        VisualElement left = spread.Q(className: "left-page");
        VisualElement right = spread.Q(className: "right-page");

        if (left != null)
        {
            left.style.scale = new StyleScale(Vector2.one);
            VisualElement shadow = left.Q("shadow-overlay");
            if (shadow != null) shadow.style.opacity = 0;
        }
        if (right != null)
        {
            right.style.scale = new StyleScale(Vector2.one);
            VisualElement shadow = right.Q("shadow-overlay");
            if (shadow != null) shadow.style.opacity = 0;
        }
    }

    private VisualElement CreatePage(string sideClass)
    {
        VisualElement page = new();
        page.AddToClassList("album-page");
        page.AddToClassList(sideClass);
        AddRingsToPage(page);
        return page;
    }

    private void AddRingsToPage(VisualElement page)
    {
        VisualElement ringsContainer = new();
        ringsContainer.AddToClassList("rings-container");

        VisualElement middleElement = new();
        middleElement.AddToClassList("middle-element");
        ringsContainer.Add(middleElement);

        for (int i = 0; i < 5; i++)
        {
            VisualElement ring = new();
            ring.AddToClassList("ring-image");
            ringsContainer.Add(ring);
        }

        page.Add(ringsContainer);
    }

    private void SetPageNumber(VisualElement page, string number)
    {
        Label label = new(number);
        label.AddToClassList("page-number-label");
        page.Add(label);
    }

    private void FillPageWithCards(VisualElement page, ref int currentIdx, List<CollectionCardController> cardControllersList)
    {
        int added = 0;
        while (added < CARDS_PER_PAGE && currentIdx < cardControllersList.Count)
        {
            CollectionCardController cardController = cardControllersList[currentIdx];
            ICollectionCardView cardView = cardController.CollectionCardView;

            bool isOwned = userCardsIds.Contains(cardController.CardModel.id);
            cardController.CollectionCardView.ApplyOwnedCardStyle(isOwned);

            cardView.ApplyCardStyleForActiveScene("BestiaryScene");
            cardView.CardRoot.RemoveFromClassList("middle-card");
            if (added == 1 || added == 4) cardView.CardRoot.AddToClassList("middle-card");

            page.Add(cardView.CardRoot);

            currentIdx++;
            added++;
        }
    }

    public void RefreshSceneContent() => ApplyElementFilter(activeFiltersCache);

    public void ApplyElementFilter(List<PokemonElement> activeFilters)
    {
        if (isAnimating) return;

        List<CollectionCardController> filteredCardControllers = new();

        if (activeFilters == null || activeFilters.Count == 0)
            filteredCardControllers = gameCardControllers;
        else
        {
            filteredCardControllers = gameCardControllers.Where(cardController =>
                activeFilters.Contains(cardController.CardModel.mainElement) ||
                (cardController.CardModel.secondaryElement.HasValue &&
                activeFilters.Contains(cardController.CardModel.secondaryElement.Value))
            ).ToList();
        }

        GenerateAlbum(filteredCardControllers);
    }

    private void OnPointerDown(PointerDownEvent evt)
    {
        if (isAnimating) return;

        swipeStartPosition = evt.position;
        isSwiping = true;
    }

    private void OnPointerUp(PointerUpEvent evt)
    {
        if (!isSwiping || isAnimating) return;

        Vector2 swipeEndPosition = evt.position;
        ProcessSwipe(swipeEndPosition);
        isSwiping = false;
    }

    private void ProcessSwipe(Vector2 endPosition)
    {
        float deltaX = endPosition.x - swipeStartPosition.x;
        if (Mathf.Abs(deltaX) > SWIPE_THRESHOLD)
        {
            if (deltaX < 0) FlipPage(1);
            else FlipPage(-1);
        }
    }

    private void OnPointerCancel(PointerCancelEvent evt) => isSwiping = false;

    private void RegisterCallbacks()
    {
        root.Q<Button>("exitButton").RegisterCallback<ClickEvent>(evt =>
        {
            if (SceneContext.PreviousMenuSceneName == "LoadingScene")
                SceneManager.LoadScene(SceneContext.PreviousMenuSceneName);
            else
                SceneSwitcher.SwitchScene(SceneContext.PreviousMenuSceneName, root);
        });
    }
}