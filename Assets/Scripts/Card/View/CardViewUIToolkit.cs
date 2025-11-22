using System.Xml;
using UnityEngine;
using UnityEngine.UIElements;
using ColorUtility = UnityEngine.ColorUtility;

public class CardViewUIToolkit : ICollectionCardView
{
    private readonly CardModel cardModel;
    private readonly VisualElement cardRoot;
    private readonly VisualTreeAsset cardTemplate;

    private VisualElement cardElement;
    private Button addToCardDeckButton;
    private Label titleLabel;

    public CardViewUIToolkit(CardModel model, VisualTreeAsset template)
    {
        cardModel = model;
        cardTemplate = template;
        cardRoot = cardTemplate.Instantiate();

        InitializeElements();
        BindViewWithData();
    }

    private void InitializeElements()
    {
        cardElement = cardRoot.Q<VisualElement>("fullCard");
        addToCardDeckButton = cardRoot.Q<Button>("addToCardDeck");
        titleLabel = cardRoot.Q<Label>("title");
    }

    private void BindViewWithData()
    {
        cardElement.style.backgroundColor = new StyleColor(cardModel.colors.cardColor);
        titleLabel.text = cardModel.title;

        SetBodyAndBorders();
        SetImages();

        addToCardDeckButton.style.unityBackgroundImageTintColor = new Color(0.5f, 0.5f, 0.5f);
    }

    public void RegisterClickHandlers(EventCallback<ClickEvent> onCardElementClick, EventCallback<ClickEvent> onAddToCardDeckButtonClick)
    {
        cardElement?.RegisterCallback(onCardElementClick);
        addToCardDeckButton?.RegisterCallback(onAddToCardDeckButtonClick);
    }

    public void UnregisterClickHandlers(EventCallback<ClickEvent> onCardElementClick, EventCallback<ClickEvent> onAddToCardDeckButtonClick)
    {
        cardElement?.UnregisterCallback(onCardElementClick);
        addToCardDeckButton?.UnregisterCallback(onAddToCardDeckButtonClick);
    }

    private void SetBodyAndBorders()
    {
        VisualElement bodyContainer = cardRoot.Q<VisualElement>("body");
        VisualElement elementsArea = cardRoot.Q<VisualElement>("elementsArea");
        XmlDocument bodyBorderXmlDocument, elementsAreaBorderXmlDocument;

        string borderBodyFileName, borderElementsFileName;

        if (cardModel.secondaryElement != null)
        {
            borderBodyFileName = "borderWithLinearGradientForTwoElement";
            borderElementsFileName = "borderForTwoElements";
            bodyBorderXmlDocument = XMLDocumentCreater.CreateXmlDocument(borderBodyFileName);
        }
        else
        {
            borderBodyFileName = "borderWithLinearGradientForOneElement";
            borderElementsFileName = "borderForOneElement";
            bodyBorderXmlDocument = XMLDocumentCreater.CreateXmlDocument(borderBodyFileName);

            elementsArea.style.width = new StyleLength(59);
            elementsArea.style.left = new StyleLength(201);
            cardRoot.Q<VisualElement>("mainElement").style.left = new StyleLength(22);
        }

        elementsAreaBorderXmlDocument = XMLDocumentCreater.CreateXmlDocument(borderElementsFileName);

        UpdateGradientStops(bodyBorderXmlDocument, cardModel.colors.borderColor1, cardModel.colors.borderColor2);

        BindVisualElementWithSvg(bodyContainer, bodyBorderXmlDocument.OuterXml);
        BindVisualElementWithSvg(elementsArea, elementsAreaBorderXmlDocument.OuterXml);
    }

    private void SetImages()
    {
        cardRoot.Q<VisualElement>("pokemonImage").style.backgroundImage =
            new StyleBackground(Resources.Load<Sprite>($"Sprites/PokemonImages/{cardModel.imageName}"));

        VisualElement mainElement = cardRoot.Q<VisualElement>("mainElement");
        mainElement.style.backgroundImage = new StyleBackground(
            Resources.Load<Sprite>($"Sprites/Elements/{cardModel.mainElement.ToString().ToLowerInvariant()}"));

        VisualElement secondaryElement = cardRoot.Q<VisualElement>("secondaryElement");
        secondaryElement.style.backgroundImage = new StyleBackground(
            Resources.Load<Sprite>($"Sprites/Elements/{cardModel.secondaryElement?.ToString().ToLowerInvariant()}"));
    }

    private void BindVisualElementWithSvg(VisualElement visualElement, string xmlCode)
    {
        Texture2D texture = SvgRenderer.SvgToTexture(xmlCode);
        visualElement.style.backgroundImage = new StyleBackground(texture);
    }

    private void UpdateGradientStops(XmlDocument doc, Color color1, Color color2)
    {
        if (doc == null) return;

        var nsmgr = new XmlNamespaceManager(doc.NameTable);
        nsmgr.AddNamespace("svg", "http://www.w3.org/2000/svg");
        XmlNode stop1, stop2;

        stop1 = doc.SelectSingleNode($"//svg:linearGradient/svg:stop[1]", nsmgr);
        stop2 = doc.SelectSingleNode($"//svg:linearGradient/svg:stop[2]", nsmgr);

        if (stop1?.Attributes["stop-color"] != null)
            stop1.Attributes["stop-color"].Value = $"#{ColorUtility.ToHtmlStringRGB(color1)}";

        if (stop2?.Attributes["stop-color"] != null)
            stop2.Attributes["stop-color"].Value = $"#{ColorUtility.ToHtmlStringRGB(color2)}";
    }

    public void SetAddedToDeck(bool isAdded)
    {
        addToCardDeckButton.style.unityBackgroundImageTintColor = isAdded ? Color.green : Color.gray;
    }

    public void DisableAddToDeckButton()
    {
        addToCardDeckButton.style.display = DisplayStyle.None;
    }

    public void SetOpacity(bool IsFilterElement)
    {
        cardElement.style.opacity = IsFilterElement ? 1f : 0.3f;
    }

    public VisualElement CardRoot => cardRoot;
    public VisualTreeAsset CardTemplate => cardTemplate;
    public GameObject CardRootGameObject => null;
    public VisualElement CardRootUIToolkit => cardRoot;

}