using UnityEngine;
using UnityEngine.UIElements;
using System.Xml;

public struct ElementLayout
{
    public string borderBodyFileName;
    public string borderElementsFileName;
    public float Width;
    public float Height;
    public float Left;
    public float Top;
    public float MainLeft;
    public float MainTop;
    public float SecondaryLeft;
    public float SecondaryTop;
}

public static class CardElementLayoutConfigForCollection
{
    public static ElementLayout OneElementLayout = new()
    {
        borderBodyFileName = "borderWithLinearGradientForOneElement",
        borderElementsFileName = "borderForOneElement",
        Width = 50,
        Height = 26,
        Left = 167,
        Top = 195,
        MainLeft = 19,
        MainTop = 4,
        SecondaryLeft = 0,
        SecondaryTop = 0
    };

    public static ElementLayout TwoElementLayout = new()
    {
        borderBodyFileName = "borderWithLinearGradientForTwoElement",
        borderElementsFileName = "borderForTwoElements",
        Width = 61.7f,
        Height = 26,
        Left = 155,
        Top = 195,
        MainLeft = 14,
        MainTop = 4,
        SecondaryLeft = 38,
        SecondaryTop = 4
    };
}

public static class CardElementLayoutConfigForDeck
{
    public static ElementLayout OneElementLayout = new()
    {
        borderBodyFileName = "borderWithLinearGradientForOneElement",
        borderElementsFileName = "borderForOneElement",
        Width = 58,
        Height = 30,
        Left = 207,
        Top = 235,
        MainLeft = 22,
        MainTop = 4,
        SecondaryLeft = 0,
        SecondaryTop = 0
    };

    public static ElementLayout TwoElementLayout = new()
    {
        borderBodyFileName = "borderWithLinearGradientForTwoElement",
        borderElementsFileName = "borderForTwoElements",
        Width = 70,
        Height = 30,
        Left = 195,
        Top = 235,
        MainLeft = 13,
        MainTop = 4,
        SecondaryLeft = 41,
        SecondaryTop = 4
    };
}

public static class CardViewHelper
{
    public static void UpdateBodyUIToolkit(VisualElement cardRoot, CardModel cardModel, bool forDeck)
    {
        VisualElement bodyContainer = cardRoot.Q<VisualElement>("body");
        VisualElement elementsArea = cardRoot.Q<VisualElement>("elementsArea");
        VisualElement mainElement = cardRoot.Q<VisualElement>("mainElement");
        VisualElement secondaryElement = cardRoot.Q<VisualElement>("secondaryElement");

        XmlDocument bodyBorderXmlDocument, elementsAreaBorderXmlDocument;
        ElementLayout layout;

        if (forDeck)
        {
            layout = cardModel.secondaryElement != null
                ? CardElementLayoutConfigForDeck.TwoElementLayout
                : CardElementLayoutConfigForDeck.OneElementLayout;
        }
        else
        {
            layout = cardModel.secondaryElement != null
                ? CardElementLayoutConfigForCollection.TwoElementLayout
                : CardElementLayoutConfigForCollection.OneElementLayout;
        }

        elementsArea.style.width = new StyleLength(layout.Width);
        elementsArea.style.height = new StyleLength(layout.Height);
        elementsArea.style.left = new StyleLength(layout.Left);
        elementsArea.style.top = new StyleLength(layout.Top);

        mainElement.style.left = new StyleLength(layout.MainLeft);
        mainElement.style.top = new StyleLength(layout.MainTop);

        secondaryElement.style.left = new StyleLength(layout.SecondaryLeft);
        secondaryElement.style.top = new StyleLength(layout.SecondaryTop);

        if (cardModel.secondaryElement == null)
            secondaryElement.style.opacity = 0f;

        bodyBorderXmlDocument = XMLDocumentCreater.CreateXmlDocument(layout.borderBodyFileName);
        elementsAreaBorderXmlDocument = XMLDocumentCreater.CreateXmlDocument(layout.borderElementsFileName);

        UpdateGradientStops(bodyBorderXmlDocument, cardModel.colors.borderColor1, cardModel.colors.borderColor2);
        BindVisualElementWithSvg(bodyContainer, bodyBorderXmlDocument.OuterXml);
        BindVisualElementWithSvg(elementsArea, elementsAreaBorderXmlDocument.OuterXml);
    }

    public static void UpdateBodyUGUI(GameObject cardRoot, CardModel cardModel)
    {
        GameObject bodyContainer = cardRoot.transform.Find("Body").gameObject;
        GameObject elementsArea = cardRoot.transform.Find("Body/ElementsArea").gameObject;
        GameObject mainElement = cardRoot.transform.Find("Body/ElementsArea/MainElement").gameObject;
        GameObject secondaryElement = cardRoot.transform.Find("Body/ElementsArea/SecondaryElement").gameObject;

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

            RectTransform main = mainElement.GetComponent<RectTransform>();
            main.anchoredPosition = new Vector2(2.7f, main.anchoredPosition.y);

            secondaryElement.GetComponent<UnityEngine.UI.Image>().color = new Color(1f, 1f, 1f, 0f);
        }

        elementsAreaBorderXmlDocument = XMLDocumentCreater.CreateXmlDocument(borderElementsFileName);
        UpdateGradientStops(bodyBorderXmlDocument, cardModel.colors.borderColor1, cardModel.colors.borderColor2);
        BindGameObjectWithSvg(bodyContainer, bodyBorderXmlDocument.OuterXml);
        BindGameObjectWithSvg(elementsArea, elementsAreaBorderXmlDocument.OuterXml);
    }

    public static void SetImagesUGUI(GameObject root, CardModel model)
    {
        UnityEngine.UI.Image pokemonImage = root.transform.Find("Body/PokemonImage").GetComponent<UnityEngine.UI.Image>();
        pokemonImage.sprite = Resources.Load<Sprite>($"Sprites/PokemonImages/{model.imageName}");

        UnityEngine.UI.Image mainElementImage = root.transform.Find("Body/ElementsArea/MainElement").GetComponent<UnityEngine.UI.Image>();
        mainElementImage.sprite = Resources.Load<Sprite>($"Sprites/Elements/{model.mainElement.ToString().ToLowerInvariant()}");

        UnityEngine.UI.Image secondaryElementImage = root.transform.Find("Body/ElementsArea/SecondaryElement").GetComponent<UnityEngine.UI.Image>();
        secondaryElementImage.sprite = Resources.Load<Sprite>($"Sprites/Elements/{model.secondaryElement?.ToString().ToLowerInvariant()}");
    }

    public static void SetImagesUIToolkit(VisualElement root, CardModel model)
    {
        root.Q<VisualElement>("pokemonImage").style.backgroundImage =
            new StyleBackground(Resources.Load<Sprite>($"Sprites/PokemonImages/{model.imageName}"));

        root.Q<VisualElement>("mainElement").style.backgroundImage =
            new StyleBackground(Resources.Load<Sprite>($"Sprites/Elements/{model.mainElement.ToString().ToLowerInvariant()}"));

        root.Q<VisualElement>("secondaryElement").style.backgroundImage =
            new StyleBackground(Resources.Load<Sprite>($"Sprites/Elements/{model.secondaryElement?.ToString().ToLowerInvariant()}"));
    }

    public static void BindGameObjectWithSvg(GameObject gameObject, string xmlCode)
    {
        Texture2D texture = SvgRenderer.SvgToTexture(xmlCode);
        gameObject.GetComponent<UnityEngine.UI.Image>().sprite =
            Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }

    public static void BindVisualElementWithSvg(VisualElement visualElement, string xmlCode)
    {
        Texture2D texture = SvgRenderer.SvgToTexture(xmlCode);
        visualElement.style.backgroundImage = new StyleBackground(texture);
    }

    public static void UpdateGradientStops(XmlDocument document, Color color1, Color color2)
    {
        if (document != null)
        {
            var nsmgr = new XmlNamespaceManager(document.NameTable);
            nsmgr.AddNamespace("svg", "http://www.w3.org/2000/svg");

            XmlNode stop1 = document.SelectSingleNode("//svg:linearGradient/svg:stop[1]", nsmgr);
            XmlNode stop2 = document.SelectSingleNode("//svg:linearGradient/svg:stop[2]", nsmgr);

            if (stop1?.Attributes["stop-color"] != null)
                stop1.Attributes["stop-color"].Value = $"#{ColorUtility.ToHtmlStringRGB(color1)}";

            if (stop2?.Attributes["stop-color"] != null)
                stop2.Attributes["stop-color"].Value = $"#{ColorUtility.ToHtmlStringRGB(color2)}";
        }
    }
}
