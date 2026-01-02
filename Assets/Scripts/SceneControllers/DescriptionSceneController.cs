using UnityEngine;
using UnityEngine.UIElements;

public static class SelectedCardModelStorage
{
    public static CardModel SelectedCardModel { get; set; }
}

public class DescriptionSceneController : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    private VisualElement root;
    private VisualElement cardRoot;
    private Label titleLabel;
    private Label pokemonName;

    private void Start()
    {
        root = uiDocument.rootVisualElement;
        cardRoot = root.Q<VisualElement>("fullCard");
        titleLabel = cardRoot.Q<Label>("title");
        pokemonName = root.Q<Label>("pokemonName");

        pokemonName.text = SelectedCardModelStorage.SelectedCardModel.title.ToUpper();

        BindCardData(cardRoot, SelectedCardModelStorage.SelectedCardModel);

        RegisterCallbacks();
    }

    private void BindCardData(VisualElement cardRoot, CardModel cardModel)
    {
        cardRoot.style.backgroundColor = new StyleColor(cardModel.colors.cardColor);
        titleLabel.text = cardModel.title;
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