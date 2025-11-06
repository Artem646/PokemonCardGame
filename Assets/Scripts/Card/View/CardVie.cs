using System;
using System.Xml;
using UnityEngine;
using UnityEngine.UIElements;
using ColorUtility = UnityEngine.ColorUtility;

public class CardView
{
    public CardModel cardModel { get; private set; }

    private VisualElement cardRoot;
    private VisualElement cardElement;
    private Button addToCardDeckButton;

    public event Action<CardView> OnCardClicked;
    public event Action<CardView> OnAddToDeckClicked;

    private readonly VisualTreeAsset cardTemplate;
    private VisualElement overlay;

    public CardView(CardModel model, VisualTreeAsset template, VisualElement parent)
    {
        cardModel = model;
        cardTemplate = template;
        cardRoot = cardTemplate.Instantiate();

        parent.Add(cardRoot);

        InitializeElements();
        BindData();
        RegisterCallbacks();
    }

    private void InitializeElements()
    {
        cardElement = cardRoot.Q<VisualElement>("fullCard");
        addToCardDeckButton = cardRoot.Q<Button>("addToCardDeck");
    }

    public void BindOverlay(VisualElement overlay)
    {
        this.overlay = overlay;
    }

    private void BindData()
    {
        cardElement.style.backgroundColor = new StyleColor(cardModel.colors.cardColor);
        cardRoot.Q<Label>("title").text = cardModel.title;

        SetBodyAndBorders();
        SetImages();

        addToCardDeckButton.style.unityBackgroundImageTintColor = new Color(0.5f, 0.5f, 0.5f);
    }

    private void RegisterCallbacks()
    {
        cardElement?.RegisterCallback<ClickEvent>(evt => { OnCardClicked?.Invoke(this); });
        addToCardDeckButton?.RegisterCallback<ClickEvent>(evt => { OnAddToDeckClicked?.Invoke(this); });
    }

    private void SetBodyAndBorders()
    {
        VisualElement bodyContainer = cardRoot.Q<VisualElement>("body");
        VisualElement elementsArea = cardRoot.Q<VisualElement>("elementsArea");

        string borderFileBody, borderFileElements;

        if (cardModel.secondaryElement == null)
        {
            borderFileBody = "borderWithLinearGradientForTwoElement";
            borderFileElements = "borderForTwoElements";
        }
        else
        {
            borderFileBody = "borderWithLinearGradientForOneElement";
            borderFileElements = "borderForOneElement";

            elementsArea.style.width = new StyleLength(59);
            elementsArea.style.left = new StyleLength(201);
            Root.Q<VisualElement>("mainElement").style.left = new StyleLength(22);
        }

        var bodyXml = XMLDocumentCreater.CreateXmlDocument(borderFileBody);
        var elementsXml = XMLDocumentCreater.CreateXmlDocument(borderFileElements);

        UpdateGradientStops(bodyXml, Model.colors.borderColor1, Model.colors.borderColor2);

        BindVisualElementWithSvg(bodyContainer, bodyXml.OuterXml);
        BindVisualElementWithSvg(elementsArea, elementsXml.OuterXml);
    }

    private void FillCard(VisualElement cardRoot)
    {





        string xmlWithSvgCodeFileName;
        XmlDocument xmlDocumentBodyBorder, xmlDocumentElementsBorder;

        if (CardModel.secondaryElement == null)
        {
            xmlDocumentBodyBorder = XMLDocumentCreater.CreateXmlDocument(xmlWithSvgCodeFileName);
            UpdateGradientStops(xmlDocumentBodyBorder, 2, CardModel.colors.borderColor1, CardModel.colors.borderColor2);

            xmlDocumentElementsBorder = XMLDocumentCreater.CreateXmlDocument(xmlWithSvgCodeFileName);
        }
        else
        {
            xmlDocumentBodyBorder = XMLDocumentCreater.CreateXmlDocument(xmlWithSvgCodeFileName);
            UpdateGradientStops(xmlDocumentBodyBorder, 1, CardModel.colors.borderColor1, CardModel.colors.borderColor2);

            xmlWithSvgCodeFileName = "borderForOneElement";
            xmlDocumentElementsBorder = XMLDocumentCreater.CreateXmlDocument(xmlWithSvgCodeFileName);
            elementsArea.style.width = new StyleLength(59);
            elementsArea.style.left = new StyleLength(201);
            cardRoot.Q<VisualElement>("mainElement").style.left = new StyleLength(22);
        }

        BindVisualElementWithSvg(bodyContainer, xmlDocumentBodyBorder.OuterXml);
        BindVisualElementWithSvg(elementsArea, xmlDocumentElementsBorder.OuterXml);

        cardRoot.Q<VisualElement>("pokemonImage").style.backgroundImage =
            new StyleBackground(Resources.Load<Sprite>($"Sprites/PokemonImages/{CardModel.imageName}"));

        cardRoot.Q<VisualElement>("mainElement").style.backgroundImage =
            new StyleBackground(Resources.Load<Sprite>($"Sprites/Elements/{CardModel.mainElement.ToString().ToLowerInvariant()}"));

        cardRoot.Q<VisualElement>("secondaryElement").style.backgroundImage =
            new StyleBackground(Resources.Load<Sprite>($"Sprites/Elements/{CardModel.secondaryElement.ToString().ToLowerInvariant()}"));

        cardRoot.Q<Button>("addToCardDeck").style.unityBackgroundImageTintColor = new Color(0.5f, 0.5f, 0.5f);
    }

    private void BindVisualElementWithSvg(VisualElement visualElement, string xmlCode)
    {
        Texture2D texture = SvgRenderer.SvgToTexture(xmlCode);
        visualElement.style.backgroundImage = new StyleBackground(texture);
    }

    private void UpdateGradientStops(XmlDocument doc, int countOfElements, Color color1, Color color2)
    {
        if (doc == null) return;

        var nsmgr = new XmlNamespaceManager(doc.NameTable);
        nsmgr.AddNamespace("svg", "http://www.w3.org/2000/svg");
        XmlNode stop1, stop2;

        if (countOfElements == 1)
        {
            stop1 = doc.SelectSingleNode($"//svg:linearGradient/svg:stop[1]", nsmgr);
            stop2 = doc.SelectSingleNode($"//svg:linearGradient/svg:stop[2]", nsmgr);

            if (stop1?.Attributes["stop-color"] != null)
                stop1.Attributes["stop-color"].Value = ColorUtility.ToHtmlStringRGB(color1);

            if (stop2?.Attributes["stop-color"] != null)
                stop2.Attributes["stop-color"].Value = ColorUtility.ToHtmlStringRGB(color2);
        }
        else if (countOfElements == 2)
        {
            stop1 = doc.SelectSingleNode($"//svg:linearGradient/svg:stop[1]", nsmgr);
            stop2 = doc.SelectSingleNode($"//svg:linearGradient/svg:stop[2]", nsmgr);

            if (stop1?.Attributes["stop-color"] != null)
                stop1.Attributes["stop-color"].Value = ColorUtility.ToHtmlStringRGB(color1);

            if (stop2?.Attributes["stop-color"] != null)
                stop2.Attributes["stop-color"].Value = ColorUtility.ToHtmlStringRGB(color2);
        }
    }

















    private void SetImages()
    {
        Root.Q<VisualElement>("pokemonImage").style.backgroundImage =
            new StyleBackground(Resources.Load<Sprite>($"Sprites/PokemonImages/{Model.imageName}"));

        var mainElement = Root.Q<VisualElement>("mainElement");
        mainElement.style.backgroundImage = new StyleBackground(
            Resources.Load<Sprite>($"Sprites/Elements/{Model.mainElement.ToString().ToLowerInvariant()}"));
        mainElement.userData = Model.mainElement.ToString().ToLowerInvariant();

        var secondaryElement = Root.Q<VisualElement>("secondaryElement");
        secondaryElement.style.backgroundImage = new StyleBackground(
            Resources.Load<Sprite>($"Sprites/Elements/{Model.secondaryElement?.ToString().ToLowerInvariant()}"));
        secondaryElement.userData = Model.secondaryElement?.ToString().ToLowerInvariant();
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

        var stops = doc.SelectNodes("//svg:linearGradient/svg:stop", nsmgr);
        if (stops?.Count >= 2)
        {
            stops[0].Attributes["stop-color"].Value = ColorUtility.ToHtmlStringRGB(color1);
            stops[1].Attributes["stop-color"].Value = ColorUtility.ToHtmlStringRGB(color2);
        }
    }
}



