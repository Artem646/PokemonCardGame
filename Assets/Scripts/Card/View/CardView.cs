// using System.Xml;
// using UnityEngine;
// using UnityEngine.UIElements;
// using ColorUtility = UnityEngine.ColorUtility;

// public class CardView
// {
//     public CardModel cardModel { get; private set; }

//     private VisualElement cardRoot;
//     private VisualElement card;
//     private Button addToCardDeckButton;
//     private VisualElement overlay;
//     private readonly VisualTreeAsset cardTemplate;

//     public CardView() { }

//     public CardView(CardModel data, VisualTreeAsset template)
//     {
//         cardModel = data;
//         cardTemplate = template;
//         cardRoot = cardTemplate.Instantiate();
//         FillCard(cardRoot);
//         RegisterCallbacks(cardRoot);
//         Add(cardRoot);
//     }

//     private void RegisterCallbacks(VisualElement cardRoot)
//     {
//         addToCardDeckButton = cardRoot.Q<Button>("addToCardDeck");

//         card?.RegisterCallback<ClickEvent>(evt =>
//         {
//             if (overlay == null) return;

//             var clone = cardTemplate.Instantiate();
//             FillCard(clone);
//             CardScaleAnimator.AnimateCardFromListToOverlay(card, clone, overlay, evt.position);
//         });

//         addToCardDeckButton?.RegisterCallback<ClickEvent>(evt =>
//         {
//             CardDeck.Instance.ToggleAddToCardDeckButton(addToCardDeckButton, this);
//         });
//     }

//     public void BindOverlay(VisualElement overlay)
//     {
//         this.overlay = overlay;

//         overlay?.RegisterCallback<ClickEvent>(evt =>
//         {
//             CardScaleAnimator.AnimateCardBack(overlay);
//         });
//     }

//     private void FillCard(VisualElement cardRoot)
//     {
//         card = cardRoot.Q<VisualElement>("fullCard");
//         card.style.backgroundColor = new StyleColor(cardModel.colors.cardColor);

//         cardRoot.Q<Label>("title").text = cardModel.title;

//         VisualElement bodyContainer = cardRoot.Q<VisualElement>("body");
//         VisualElement elementsArea = cardRoot.Q<VisualElement>("elementsArea");
//         string xmlWithSvgCodeFileName;
//         XmlDocument xmlDocumentBodyBorder, xmlDocumentElementsBorder;

//         if (cardModel.secondaryElement == null)
//         {
//             xmlWithSvgCodeFileName = "borderWithLinearGradientForTwoElement";
//             xmlDocumentBodyBorder = XMLDocumentCreater.CreateXmlDocument(xmlWithSvgCodeFileName);

//             UpdateGradientStops(xmlDocumentBodyBorder, 2, cardModel.colors.borderColor1, cardModel.colors.borderColor2);

//             xmlWithSvgCodeFileName = "borderForTwoElements";
//             xmlDocumentElementsBorder = XMLDocumentCreater.CreateXmlDocument(xmlWithSvgCodeFileName);
//         }
//         else
//         {
//             xmlWithSvgCodeFileName = "borderWithLinearGradientForOneElement";
//             xmlDocumentBodyBorder = XMLDocumentCreater.CreateXmlDocument(xmlWithSvgCodeFileName);
//             UpdateGradientStops(xmlDocumentBodyBorder, 1, cardModel.colors.borderColor1, cardModel.colors.borderColor2);

//             xmlWithSvgCodeFileName = "borderForOneElement";
//             xmlDocumentElementsBorder = XMLDocumentCreater.CreateXmlDocument(xmlWithSvgCodeFileName);
//             elementsArea.style.width = new StyleLength(59);
//             elementsArea.style.left = new StyleLength(201);
//             cardRoot.Q<VisualElement>("mainElement").style.left = new StyleLength(22);
//         }

//         BindVisualElementWithSvg(bodyContainer, xmlDocumentBodyBorder.OuterXml);
//         BindVisualElementWithSvg(elementsArea, xmlDocumentElementsBorder.OuterXml);

//         cardRoot.Q<VisualElement>("pokemonImage").style.backgroundImage = new StyleBackground(Resources.Load<Sprite>($"Sprites/PokemonImages/{cardModel.imageName}"));

//         cardRoot.Q<VisualElement>("mainElement").style.backgroundImage = new StyleBackground(Resources.Load<Sprite>($"Sprites/Elements/{cardModel.mainElement.ToString().ToLowerInvariant()}"));
//         cardRoot.Q<VisualElement>("mainElement").userData = cardModel.mainElement.ToString().ToLowerInvariant();
//         cardRoot.Q<VisualElement>("secondaryElement").style.backgroundImage = new StyleBackground(Resources.Load<Sprite>($"Sprites/Elements/{cardModel.secondaryElement.ToString().ToLowerInvariant()}"));
//         cardRoot.Q<VisualElement>("secondaryElement").userData = cardModel.secondaryElement.ToString().ToLowerInvariant();

//         cardRoot.Q<Button>("addToCardDeck").style.unityBackgroundImageTintColor = new Color(0.5f, 0.5f, 0.5f);
//     }

//     private void BindVisualElementWithSvg(VisualElement visualElement, string xmlCode)
//     {
//         Texture2D texture = SvgRenderer.SvgToTexture(xmlCode);
//         visualElement.style.backgroundImage = new StyleBackground(texture);
//     }

//     private void UpdateGradientStops(XmlDocument doc, int countOfElements, Color color1, Color color2)
//     {
//         if (doc == null) return;

//         var nsmgr = new XmlNamespaceManager(doc.NameTable);
//         nsmgr.AddNamespace("svg", "http://www.w3.org/2000/svg");
//         XmlNode stop1, stop2;

//         if (countOfElements == 1)
//         {
//             stop1 = doc.SelectSingleNode($"//svg:linearGradient/svg:stop[1]", nsmgr);
//             stop2 = doc.SelectSingleNode($"//svg:linearGradient/svg:stop[2]", nsmgr);

//             if (stop1?.Attributes["stop-color"] != null)
//                 stop1.Attributes["stop-color"].Value = ColorUtility.ToHtmlStringRGB(color1);

//             if (stop2?.Attributes["stop-color"] != null)
//                 stop2.Attributes["stop-color"].Value = ColorUtility.ToHtmlStringRGB(color2);
//         }
//         else if (countOfElements == 2)
//         {
//             stop1 = doc.SelectSingleNode($"//svg:linearGradient/svg:stop[1]", nsmgr);
//             stop2 = doc.SelectSingleNode($"//svg:linearGradient/svg:stop[2]", nsmgr);

//             if (stop1?.Attributes["stop-color"] != null)
//                 stop1.Attributes["stop-color"].Value = ColorUtility.ToHtmlStringRGB(color1);

//             if (stop2?.Attributes["stop-color"] != null)
//                 stop2.Attributes["stop-color"].Value = ColorUtility.ToHtmlStringRGB(color2);
//         }
//     }
// }
