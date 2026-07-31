using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

public static class SelectedCardModelStorage
{
    public static CardModel SelectedCardModel { get; set; }
}

public class DescriptionSceneController : MonoBehaviour, IRefreshableScene
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private VisualTreeAsset deckCardTemplate;
    [SerializeField] private GameObject cardPrefab3D;

    private VisualElement root;
    private VisualElement cardPanel;
    private VisualElement evolutionsContainer;
    private VisualElement prevEvolutionCardContainer;
    private VisualElement currentEvolutionCardContainer;
    private VisualElement nextEvolutionCardContainer;
    private Label pokemonName;

    private CardModel selectedCardModel;
    private CollectionCardController cardController;

    private void Start()
    {
        InitializeUI();
        // LocalizeElements();
        RefreshSceneContent();
        RegisterCallbacks();
    }

    private void InitializeUI()
    {
        root = uiDocument.rootVisualElement;
        pokemonName = root.Q<Label>("pokemonName");
        cardPanel = root.Q<VisualElement>("cardPanel");
        evolutionsContainer = root.Q<VisualElement>("evolutionsContainer");
        prevEvolutionCardContainer = evolutionsContainer.Q<VisualElement>("prevEvolutionCardContainer");
        currentEvolutionCardContainer = evolutionsContainer.Q<VisualElement>("currentEvolutionCardContainer");
        nextEvolutionCardContainer = evolutionsContainer.Q<VisualElement>("nextEvolutionCardContainer");
    }

    public void RefreshSceneContent()
    {
        selectedCardModel = SelectedCardModelStorage.SelectedCardModel;

        LocalizedString localizedPokemonName = new("PokemonTitles", selectedCardModel.titleKey);
        localizedPokemonName.StringChanged += (value) => pokemonName.text = value.ToUpper();

        ClearContainers();
        AddCardToContainer();
        ShowEvolutions();
    }

    // private void LocalizeElements()
    // {
    //     // Localizer.LocalizeElement(root, "exitButton", "ExitButton", "ElementsText");
    // }

    public void AddCardToContainer()
    {
        cardController = CardRepository.Instance.GetCollectionCardControllerById(selectedCardModel.id);
        ICollectionCardView cardView = cardController.CollectionCardView;
        cardView.ApplyCardStyleForActiveScene("DescriptionScene");
        cardPanel.Add(cardView.CardRoot);
    }

    public void ShowEvolutions()
    {
        bool prevEvolutionHasValue = selectedCardModel.evolutions.prev.HasValue;
        bool nextEvolutionHasValue = selectedCardModel.evolutions.next.HasValue;

        if (prevEvolutionHasValue)
        {
            int prevEvolutionCardId = selectedCardModel.evolutions.prev.Value;
            CollectionCardController evolutionCardController = CardRepository.Instance.GetCollectionCardControllerById(prevEvolutionCardId);

            ICollectionCardView evolitionCardView = evolutionCardController.CollectionCardView;
            evolitionCardView.CardRoot.AddToClassList("evolution-card");
            prevEvolutionCardContainer.Add(evolitionCardView.CardRoot);
        }
        else
            prevEvolutionCardContainer.AddToClassList("empty-container");

        CreateCurrentEvolutionCard();

        if (nextEvolutionHasValue)
        {
            int nextEvolutionCardId = selectedCardModel.evolutions.next.Value;
            CollectionCardController evolutionCardController = CardRepository.Instance.GetCollectionCardControllerById(nextEvolutionCardId);

            ICollectionCardView evolitionCardView = evolutionCardController.CollectionCardView;
            evolitionCardView.CardRoot.AddToClassList("evolution-card");
            nextEvolutionCardContainer.Add(evolitionCardView.CardRoot);
        }
        else
            nextEvolutionCardContainer.AddToClassList("empty-container");
    }

    private void CreateCurrentEvolutionCard()
    {
        CardModel cardModel = cardController.CardModel;
        VisualElement cardRoot = deckCardTemplate.Instantiate();

        cardRoot.Q<VisualElement>("fullCard").style.backgroundColor = new StyleColor(cardModel.colors.cardColor);
        Label title = cardRoot.Q<Label>("title");

        LocalizedString localizedText = new("PokemonTitles", cardModel.titleKey);
        localizedText.StringChanged += (str) =>
        {
            title.text = @$"<color=white><gradient=""TextGradient"">{str}</gradient></color>";

            string currentLang = LocalizationSettings.SelectedLocale.Identifier.Code;
            if (currentLang.StartsWith("ru") || currentLang.StartsWith("be"))
            {
                title.style.fontSize = 20f;
                title.style.letterSpacing = cardModel.uiToolkitVisualData.letterSpacingRU;
                title.style.unityFontStyleAndWeight = FontStyle.Bold;
            }
            else
            {
                title.style.fontSize = 27f;
                title.style.letterSpacing = cardModel.uiToolkitVisualData.letterSpacingEN;
                title.style.unityFontStyleAndWeight = FontStyle.Normal;
            }
        };

        CardViewHelper.UpdateBodyUIToolkit(cardRoot, cardModel);
        CardViewHelper.SetBodyImagesUIToolkit(cardRoot, cardModel);
        cardRoot.Q<Label>("cardNumberLabel").text = cardModel.id.ToString();

        cardRoot.AddToClassList("small-evolution-card");
        currentEvolutionCardContainer.Add(cardRoot);
    }

    private void ClearContainers()
    {
        cardPanel.Clear();
        prevEvolutionCardContainer.Clear();
        currentEvolutionCardContainer.Clear();
        nextEvolutionCardContainer.Clear();
        prevEvolutionCardContainer.RemoveFromClassList("empty-container");
        nextEvolutionCardContainer.RemoveFromClassList("empty-container");
    }

    private void RegisterCallbacks()
    {
        root.Q<Button>("exitButton").RegisterCallback<ClickEvent>(evt =>
        {
            ClearContainers();
            SceneSwitcher.ReturnToPreviousDescriptionScene(root);
        });

        root.Q<Button>("animationViewingSceneButton").RegisterCallback<ClickEvent>(async evt =>
        {
            List<int> cardsToLoad = new() { selectedCardModel.id, 1, };
            CardRepository.Instance.PreBuildBattleCardControllersInDescriptionScene(cardPrefab3D, cardsToLoad);

            SceneManager.LoadScene("AnimationViewingScene");
            while (SceneManager.GetActiveScene().name != "AnimationViewingScene")
                await Task.Yield();

            ClearContainers();
            CardRepository.Instance.AddCardsToIntermediateCardContainer();
        });
    }
}