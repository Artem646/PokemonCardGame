using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Localization;

public static class SelectedCardModelStorage
{
    public static CardModel SelectedCardModel { get; set; }
}

public class DescriptionSceneController : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    private VisualElement root;
    private VisualElement cardRoot;
    private Label pokemonName;

    private void Start()
    {
        InitializeUI();

        LocalizeElements();

        LocalizedString localizedPokemonName = new("PokemonTitles", SelectedCardModelStorage.SelectedCardModel.titleKey);
        localizedPokemonName.StringChanged += (value) =>
        {
            pokemonName.text = value.ToUpper();
        };

        BindCardData(cardRoot, SelectedCardModelStorage.SelectedCardModel);

        RegisterCallbacks();
    }

    private void InitializeUI()
    {
        root = uiDocument.rootVisualElement;
        cardRoot = root.Q<VisualElement>("fullCard");
        pokemonName = root.Q<Label>("pokemonName");
    }

    private void LocalizeElements()
    {
        Localizer.LocalizeElement(root, "exitButton", "ExitButton", "ElementsText");
    }

    private void BindCardData(VisualElement cardRoot, CardModel cardModel)
    {
        cardRoot.style.backgroundColor = new StyleColor(cardModel.colors.cardColor);
        Localizer.LocalizeElement(cardRoot, "title", cardModel.titleKey, "PokemonTitles");
        CardViewHelper.UpdateBodyUIToolkit(cardRoot, cardModel, CardElementLayoutModeConfig.Description);
        CardViewHelper.SetImagesUIToolkit(cardRoot, cardModel);
    }

    private void RegisterCallbacks()
    {
        root.Q<Button>("exitButton").RegisterCallback<ClickEvent>(evt =>
        {
            SceneSwitcher.ReturnToPreviousDescriptionScene();
        });
    }
}