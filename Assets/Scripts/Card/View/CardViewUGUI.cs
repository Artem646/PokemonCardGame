using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Xml;

public class CardViewUGUI : IBattleCardView
{
    private readonly CardModel cardModel;
    private readonly GameObject cardRoot;
    private readonly GameObject cardPrefab;

    private TextMeshProUGUI titleLabel;

    public CardViewUGUI(CardModel model, GameObject prefab, Transform parent)
    {
        cardModel = model;
        cardPrefab = prefab;
        cardRoot = Object.Instantiate(cardPrefab, parent);
        cardRoot.name = model.title;
        InitializeElements();
        BindViewWithData();
    }

    private void InitializeElements()
    {
        titleLabel = cardRoot.transform.Find("Body/Title").GetComponent<TextMeshProUGUI>();
    }

    private void BindViewWithData()
    {
        cardRoot.GetComponent<Image>().color = cardModel.colors.cardColor;
        titleLabel.text = cardModel.title;
        SetBodyAndBorders();
        SetImages();
    }

    private void SetBodyAndBorders()
    {
        GameObject body = cardRoot.transform.Find("Body").gameObject;
        GameObject elementsArea = cardRoot.transform.Find("Body/ElementsArea").gameObject;

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

            RectTransform recrElementArea = elementsArea.GetComponent<RectTransform>();
            recrElementArea.sizeDelta = new Vector2(37.8f, recrElementArea.sizeDelta.y);
            // recrElementArea.anchoredPosition = new Vector2(201, recrElementArea.anchoredPosition.y);

            RectTransform main = cardRoot.transform.Find("Body/ElementsArea/MainElement").GetComponent<RectTransform>();
            main.anchoredPosition = new Vector2(2.7f, main.anchoredPosition.y);

            cardRoot.transform.Find("Body/ElementsArea/SecondaryElement").GetComponent<Image>().color = new Color(1f, 1f, 1f, 0f);
        }

        elementsAreaBorderXmlDocument = XMLDocumentCreater.CreateXmlDocument(borderElementsFileName);

        UpdateGradientStops(bodyBorderXmlDocument, cardModel.colors.borderColor1, cardModel.colors.borderColor2);

        BindVisualElementWithSvg(body, bodyBorderXmlDocument.OuterXml);
        BindVisualElementWithSvg(elementsArea, elementsAreaBorderXmlDocument.OuterXml);
    }

    private void SetImages()
    {
        Image pokemonImage = cardRoot.transform.Find("Body/PokemonImage").GetComponent<Image>();
        pokemonImage.sprite = Resources.Load<Sprite>($"Sprites/PokemonImages/{cardModel.imageName}");

        Image mainElementImage = cardRoot.transform.Find("Body/ElementsArea/MainElement").GetComponent<Image>();
        mainElementImage.sprite = Resources.Load<Sprite>($"Sprites/Elements/{cardModel.mainElement.ToString().ToLowerInvariant()}");

        Image secondaryElementImage = cardRoot.transform.Find("Body/ElementsArea/SecondaryElement").GetComponent<Image>();
        secondaryElementImage.sprite = Resources.Load<Sprite>($"Sprites/Elements/{cardModel.secondaryElement?.ToString().ToLowerInvariant()}");
    }

    private void BindVisualElementWithSvg(GameObject gameObject, string xmlCode)
    {
        Texture2D texture = SvgRenderer.SvgToTexture(xmlCode);
        gameObject.GetComponent<Image>().sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }

    private void UpdateGradientStops(XmlDocument doc, Color color1, Color color2)
    {
        if (doc == null) return;

        var nsmgr = new XmlNamespaceManager(doc.NameTable);
        nsmgr.AddNamespace("svg", "http://www.w3.org/2000/svg");

        XmlNode stop1 = doc.SelectSingleNode("//svg:linearGradient/svg:stop[1]", nsmgr);
        XmlNode stop2 = doc.SelectSingleNode("//svg:linearGradient/svg:stop[2]", nsmgr);

        if (stop1?.Attributes["stop-color"] != null)
            stop1.Attributes["stop-color"].Value = $"#{ColorUtility.ToHtmlStringRGB(color1)}";

        if (stop2?.Attributes["stop-color"] != null)
            stop2.Attributes["stop-color"].Value = $"#{ColorUtility.ToHtmlStringRGB(color2)}";
    }

    public GameObject CardRootGameObject => cardRoot;
    public GameObject CardPrefab => cardPrefab;
    public UnityEngine.UIElements.VisualElement CardRootUIToolkit => null;
}
