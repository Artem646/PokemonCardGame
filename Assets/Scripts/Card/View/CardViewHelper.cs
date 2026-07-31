using UnityEngine;
using UnityEngine.UIElements;
using System.Xml;
using TMPro;

public static class CardViewHelper
{
    public static void UpdateBodyUIToolkit(VisualElement cardRoot, CardModel cardModel)
    {
        VisualElement bodyContainer = cardRoot.Q<VisualElement>("body");
        VisualElement elementsArea = cardRoot.Q<VisualElement>("elementsArea");
        XmlDocument bodyBorderXmlDocument;

        if (cardModel.secondaryElement != null)
        {
            string borderBodyFileName = "borderWithLinearGradientForTwoElement";
            bodyBorderXmlDocument = XMLDocumentCreater.CreateXmlDocument(borderBodyFileName);
            elementsArea.RemoveFromClassList("one-element");
            elementsArea.AddToClassList("two-elements");
        }
        else
        {
            string borderBodyFileName = "borderWithLinearGradientForOneElement";
            bodyBorderXmlDocument = XMLDocumentCreater.CreateXmlDocument(borderBodyFileName);
            elementsArea.RemoveFromClassList("two-elements");
            elementsArea.AddToClassList("one-element");
        }

        UpdateGradientStops(bodyBorderXmlDocument, cardModel.colors.borderColor1, cardModel.colors.borderColor2);
        BindVisualElementWithSvg(bodyContainer, bodyBorderXmlDocument.OuterXml);
    }

    public static void UpdateBody3D(GameObject cardRoot, CardModel cardModel)
    {
        MeshRenderer сardBodyBorderMeshRenderer = cardRoot.transform.Find("FrontUIContainer/Body/CardBodyBorder").GetComponent<MeshRenderer>();
        GameObject elementsArea = cardRoot.transform.Find("FrontUIContainer/Body/DefenseElements/ElementsAreaBorder").gameObject;
        MeshRenderer elementsAreaBorderMeshRenderer = elementsArea.GetComponent<MeshRenderer>();
        GameObject mainElement = cardRoot.transform.Find("FrontUIContainer/Body/DefenseElements/MainElementImage").gameObject;
        GameObject secondaryElement = cardRoot.transform.Find("FrontUIContainer/Body/DefenseElements/SecondaryElementImage").gameObject;

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

            Transform elementAreaTransform = elementsArea.GetComponent<Transform>();
            elementAreaTransform.localPosition = new Vector3(elementAreaTransform.localPosition.x, -0.01572f, elementAreaTransform.localPosition.z);
            elementAreaTransform.localScale = new Vector3(0.01015f, elementAreaTransform.localScale.y, elementAreaTransform.localScale.z);

            Transform mainElementTransform = mainElement.GetComponent<Transform>();
            mainElementTransform.localPosition = new Vector3(mainElementTransform.localPosition.x, -0.01648f, mainElementTransform.localPosition.z);
            secondaryElement.SetActive(false);
        }

        elementsAreaBorderXmlDocument = XMLDocumentCreater.CreateXmlDocument(borderElementsFileName);
        UpdateGradientStops(bodyBorderXmlDocument, cardModel.colors.borderColor1, cardModel.colors.borderColor2);
        Texture2D cardBodyBorderTexture = SvgRenderer.SvgToTexture(bodyBorderXmlDocument.OuterXml);
        сardBodyBorderMeshRenderer.materials[2].SetTexture("_BaseMap", cardBodyBorderTexture);
        Texture2D elementsAreaBorderTexture = SvgRenderer.SvgToTexture(elementsAreaBorderXmlDocument.OuterXml);
        elementsAreaBorderMeshRenderer.material.SetTexture("_BaseMap", elementsAreaBorderTexture);
    }

    public static void UpdateStatsUIToolkit(VisualElement cardRoot, CardModel cardModel)
    {
        Label attackValue = cardRoot.Q<Label>("attackValue");
        Label defenseValue = cardRoot.Q<Label>("defenseValue");
        Label specialAttackValue = cardRoot.Q<Label>("specialAttackValue");
        Label physicalAttackValue = cardRoot.Q<Label>("physicalAttackValue");

        attackValue.text = cardModel.stats.attack.ToString();
        defenseValue.text = cardModel.stats.defense.ToString();
        specialAttackValue.text = cardModel.stats.specialAttack.ToString();
        physicalAttackValue.text = cardModel.stats.specialDefense.ToString();
    }

    public static void UpdateStats3D(GameObject cardRoot, CardModel cardModel)
    {
        TextMeshPro attackValue = cardRoot.transform.Find("FrontUIContainer/Footer/Stats/Attack/AttackValue").GetComponent<TextMeshPro>();
        TextMeshPro defenseValue = cardRoot.transform.Find("FrontUIContainer/Footer/Stats/Defense/DefenseValue").GetComponent<TextMeshPro>();
        TextMeshPro specialAttackValue = cardRoot.transform.Find("FrontUIContainer/Footer/Stats/SpecialAttack/SpecialAttackValue").GetComponent<TextMeshPro>();
        TextMeshPro physicalAttackValue = cardRoot.transform.Find("FrontUIContainer/Footer/Stats/PhysicalAttack/PhysicalAttackValue").GetComponent<TextMeshPro>();

        attackValue.text = cardModel.stats.attack.ToString();
        defenseValue.text = cardModel.stats.defense.ToString();
        specialAttackValue.text = cardModel.stats.specialAttack.ToString();
        physicalAttackValue.text = cardModel.stats.specialDefense.ToString();
    }

    public static void UpdateAbilitiesUIToolkit(VisualElement cardRoot, CardModel cardModel)
    {
        Label firstAbilityName = cardRoot.Q<Label>("firstAbilityName");
        Label firstAbilityDiscription = cardRoot.Q<Label>("firstAbilityDiscription");
        Label firstAbilityType = cardRoot.Q<Label>("firstAbilityType");
        firstAbilityName.text = cardModel.abilities.firstAbility.name;
        firstAbilityDiscription.text = cardModel.abilities.firstAbility.discription;
        firstAbilityType.text = cardModel.abilities.firstAbility.type.ToString();

        Label secondAbilityName = cardRoot.Q<Label>("secondAbilityName");
        Label secondAbilityDiscription = cardRoot.Q<Label>("secondAbilityDiscription");
        Label secondAbilityType = cardRoot.Q<Label>("secondAbilityType");
        secondAbilityName.text = cardModel.abilities.secondAbility.name;
        secondAbilityDiscription.text = cardModel.abilities.secondAbility.discription;
        secondAbilityType.text = cardModel.abilities.secondAbility.type.ToString();

        Label thirdAbilityName = cardRoot.Q<Label>("thirdAbilityName");
        Label thirdAbilityDiscription = cardRoot.Q<Label>("thirdAbilityDiscription");
        Label thirdAbilityType = cardRoot.Q<Label>("thirdAbilityType");
        thirdAbilityName.text = cardModel.abilities.thirdAbility.name;
        thirdAbilityDiscription.text = cardModel.abilities.thirdAbility.discription;
        thirdAbilityType.text = cardModel.abilities.thirdAbility.type.ToString();

        Label fourthAbilityName = cardRoot.Q<Label>("fourthAbilityName");
        Label fourthAbilityDiscription = cardRoot.Q<Label>("fourthAbilityDiscription");
        Label fourthAbilityType = cardRoot.Q<Label>("fourthAbilityType");
        fourthAbilityName.text = cardModel.abilities.fourthAbility.name;
        fourthAbilityDiscription.text = cardModel.abilities.fourthAbility.discription;
        fourthAbilityType.text = cardModel.abilities.fourthAbility.type.ToString();
    }

    public static void UpdateAbilities3D(GameObject cardRoot, CardModel cardModel)
    {
        TextMeshPro firstAbilityName = cardRoot.transform.Find("FrontUIContainer/Footer/Abilities/FirstAbility/FirstAbilityName").GetComponent<TextMeshPro>();
        TextMeshPro firstAbilityDiscription = cardRoot.transform.Find("FrontUIContainer/Footer/Abilities/FirstAbility/FirstAbilityDiscription").GetComponent<TextMeshPro>();
        TextMeshPro firstAbilityType = cardRoot.transform.Find("FrontUIContainer/Footer/Abilities/FirstAbility/FirstAbilityType").GetComponent<TextMeshPro>();
        firstAbilityName.text = cardModel.abilities.firstAbility.name;
        firstAbilityDiscription.text = cardModel.abilities.firstAbility.discription;
        firstAbilityType.text = cardModel.abilities.firstAbility.type.ToString();

        TextMeshPro secondAbilityName = cardRoot.transform.Find("FrontUIContainer/Footer/Abilities/SecondAbility/SecondAbilityName").GetComponent<TextMeshPro>();
        TextMeshPro secondAbilityDiscription = cardRoot.transform.Find("FrontUIContainer/Footer/Abilities/SecondAbility/SecondAbilityDiscription").GetComponent<TextMeshPro>();
        TextMeshPro secondAbilityType = cardRoot.transform.Find("FrontUIContainer/Footer/Abilities/SecondAbility/SecondAbilityType").GetComponent<TextMeshPro>();
        secondAbilityName.text = cardModel.abilities.secondAbility.name;
        secondAbilityDiscription.text = cardModel.abilities.secondAbility.discription;
        secondAbilityType.text = cardModel.abilities.secondAbility.type.ToString();

        TextMeshPro thirdAbilityName = cardRoot.transform.Find("FrontUIContainer/Footer/Abilities/ThirdAbility/ThirdAbilityName").GetComponent<TextMeshPro>();
        TextMeshPro thirdAbilityDiscription = cardRoot.transform.Find("FrontUIContainer/Footer/Abilities/ThirdAbility/ThirdAbilityDiscription").GetComponent<TextMeshPro>();
        TextMeshPro thirdAbilityType = cardRoot.transform.Find("FrontUIContainer/Footer/Abilities/ThirdAbility/ThirdAbilityType").GetComponent<TextMeshPro>();
        thirdAbilityName.text = cardModel.abilities.thirdAbility.name;
        thirdAbilityDiscription.text = cardModel.abilities.thirdAbility.discription;
        thirdAbilityType.text = cardModel.abilities.thirdAbility.type.ToString();

        TextMeshPro fourthAbilityName = cardRoot.transform.Find("FrontUIContainer/Footer/Abilities/FourthAbility/FourthAbilityName").GetComponent<TextMeshPro>();
        TextMeshPro fourthAbilityDiscription = cardRoot.transform.Find("FrontUIContainer/Footer/Abilities/FourthAbility/FourthAbilityDiscription").GetComponent<TextMeshPro>();
        TextMeshPro fourthAbilityType = cardRoot.transform.Find("FrontUIContainer/Footer/Abilities/FourthAbility/FourthAbilityType").GetComponent<TextMeshPro>();
        fourthAbilityName.text = cardModel.abilities.fourthAbility.name;
        fourthAbilityDiscription.text = cardModel.abilities.fourthAbility.discription;
        fourthAbilityType.text = cardModel.abilities.fourthAbility.type.ToString();
    }

    public static void UpdateAbilityDamageUIToolkit(VisualElement cardRoot, CardModel cardModel)
    {
        Label firstAbilityDamage = cardRoot.Q<Label>("firstAbilityDamage");
        if (cardModel.abilities.firstAbility.type == AbilityType.Physical)
            firstAbilityDamage.text = (cardModel.stats.attack * cardModel.abilities.firstAbility.power / 50).ToString();
        else if (cardModel.abilities.firstAbility.type == AbilityType.Special)
            firstAbilityDamage.text = (cardModel.stats.specialAttack * cardModel.abilities.firstAbility.power / 50).ToString();

        VisualElement firstAbilityDamageElement = cardRoot.Q<VisualElement>("firstAbilityDamageElement");
        firstAbilityDamageElement.style.backgroundColor = new StyleColor(new Color32(226, 159, 159, 255));

        Label secondAbilityDamage = cardRoot.Q<Label>("secondAbilityDamage");
        if (cardModel.abilities.secondAbility.type == AbilityType.Physical)
            secondAbilityDamage.text = (cardModel.stats.attack * cardModel.abilities.secondAbility.power / 50).ToString();
        else if (cardModel.abilities.secondAbility.type == AbilityType.Special)
            secondAbilityDamage.text = (cardModel.stats.specialAttack * cardModel.abilities.secondAbility.power / 50).ToString();

        VisualElement secondAbilityDamageElement = cardRoot.Q<VisualElement>("secondAbilityDamageElement");
        secondAbilityDamageElement.style.backgroundColor = new StyleColor(new Color32(226, 159, 159, 255));

        Label fourthAbilityDamage = cardRoot.Q<Label>("fourthAbilityDamage");
        if (cardModel.abilities.fourthAbility.type == AbilityType.Physical)
            fourthAbilityDamage.text = (cardModel.stats.attack * cardModel.abilities.fourthAbility.power / 50).ToString();
        else if (cardModel.abilities.fourthAbility.type == AbilityType.Special)
            fourthAbilityDamage.text = (cardModel.stats.specialAttack * cardModel.abilities.fourthAbility.power / 50).ToString();

        VisualElement fourthAbilityDamageElement = cardRoot.Q<VisualElement>("fourthAbilityDamageElement");
        fourthAbilityDamageElement.style.backgroundColor = new StyleColor(new Color32(134, 211, 228, 255));
    }

    public static void UpdateAbilityDamage3D(GameObject cardRoot, CardModel cardModel)
    {
        TextMeshPro firstAbilityDamage = cardRoot.transform.Find("FrontUIContainer/Footer/Abilities/FirstAbility/FirstAbilityDamage").GetComponent<TextMeshPro>();
        if (cardModel.abilities.firstAbility.type == AbilityType.Physical)
            firstAbilityDamage.text = (cardModel.stats.attack * cardModel.abilities.firstAbility.power / 50).ToString();
        else if (cardModel.abilities.firstAbility.type == AbilityType.Special)
            firstAbilityDamage.text = (cardModel.stats.specialAttack * cardModel.abilities.firstAbility.power / 50).ToString();

        MeshRenderer firstAbilityDamagePlateMeshRenderer = cardRoot.transform.Find("FrontUIContainer/Footer/Abilities/FirstAbility/FirstAbilityDamagePlateMesh").GetComponent<MeshRenderer>();
        firstAbilityDamagePlateMeshRenderer.material.color = new Color32(226, 159, 159, 255);

        TextMeshPro secondAbilityDamage = cardRoot.transform.Find("FrontUIContainer/Footer/Abilities/SecondAbility/SecondAbilityDamage").GetComponent<TextMeshPro>();
        if (cardModel.abilities.secondAbility.type == AbilityType.Physical)
            secondAbilityDamage.text = (cardModel.stats.attack * cardModel.abilities.secondAbility.power / 50).ToString();
        else if (cardModel.abilities.secondAbility.type == AbilityType.Special)
            secondAbilityDamage.text = (cardModel.stats.specialAttack * cardModel.abilities.secondAbility.power / 50).ToString();

        MeshRenderer secondAbilityDamagePlateMeshRenderer = cardRoot.transform.Find("FrontUIContainer/Footer/Abilities/SecondAbility/SecondAbilityDamagePlateMesh").GetComponent<MeshRenderer>();
        secondAbilityDamagePlateMeshRenderer.material.color = new Color32(226, 159, 159, 255);

        TextMeshPro fourthAbilityDamage = cardRoot.transform.Find("FrontUIContainer/Footer/Abilities/FourthAbility/FourthAbilityDamage").GetComponent<TextMeshPro>();
        if (cardModel.abilities.fourthAbility.type == AbilityType.Physical)
            fourthAbilityDamage.text = (cardModel.stats.attack * cardModel.abilities.fourthAbility.power / 50).ToString();
        else if (cardModel.abilities.fourthAbility.type == AbilityType.Special)
            fourthAbilityDamage.text = (cardModel.stats.specialAttack * cardModel.abilities.fourthAbility.power / 50).ToString();

        MeshRenderer fourthAbilityDamagePlateMeshRenderer = cardRoot.transform.Find("FrontUIContainer/Footer/Abilities/FourthAbility/FourthAbilityDamagePlateMesh").GetComponent<MeshRenderer>();
        fourthAbilityDamagePlateMeshRenderer.material.color = new Color32(134, 211, 228, 255);
    }

    public static void SetBodyImagesUIToolkit(VisualElement cardRoot, CardModel cardModel)
    {
        VisualElement pokemonImage = cardRoot.Q<VisualElement>("pokemonImage");
        pokemonImage.style.backgroundImage = new StyleBackground(Resources.Load<Sprite>($"Sprites/PokemonImages/{cardModel.imageName}"));

        CardUIToolkitVisualDataSO visualData = cardModel.uiToolkitVisualData;
        pokemonImage.style.top = Length.Percent(visualData.topPercent);
        pokemonImage.style.left = Length.Percent(visualData.leftPercent);
        pokemonImage.style.rotate = new Rotate(Angle.Degrees(visualData.rotationAngle));
        pokemonImage.style.scale = new StyleScale(new Vector2(visualData.scaleX, visualData.scaleY));

        cardRoot.Q<VisualElement>("mainElement").style.backgroundImage =
            new StyleBackground(Resources.Load<Sprite>($"Sprites/Elements/{cardModel.mainElement.ToString().ToLowerInvariant()}"));

        cardRoot.Q<VisualElement>("secondaryElement").style.backgroundImage =
            new StyleBackground(Resources.Load<Sprite>($"Sprites/Elements/{cardModel.secondaryElement?.ToString().ToLowerInvariant()}"));
    }

    public static void SetImages3D(GameObject cardRoot, CardModel cardModel)
    {
        Transform pokemonImageTransform = cardRoot.transform.Find("FrontUIContainer/Body/PokemonImage");

        MeshRenderer pokemonImageMeshRenderer = pokemonImageTransform.GetComponent<MeshRenderer>();
        pokemonImageMeshRenderer.material.SetTexture("_BaseMap", Resources.Load<Texture2D>($"Sprites/PokemonImages/{cardModel.imageName}"));

        pokemonImageTransform.localPosition = cardModel.visualData.position;
        pokemonImageTransform.localScale = cardModel.visualData.scale;
        pokemonImageTransform.localEulerAngles = cardModel.visualData.rotation;

        MeshRenderer mainElementImageMeshRenderer = cardRoot.transform.Find("FrontUIContainer/Body/DefenseElements/MainElementImage").GetComponent<MeshRenderer>();
        mainElementImageMeshRenderer.material.SetTexture("_BaseMap", Resources.Load<Texture2D>($"Sprites/Elements/{cardModel.mainElement.ToString().ToLowerInvariant()}"));

        MeshRenderer secondaryElementImageMeshRenderer = cardRoot.transform.Find("FrontUIContainer/Body/DefenseElements/SecondaryElementImage").GetComponent<MeshRenderer>();
        secondaryElementImageMeshRenderer.material.SetTexture("_BaseMap", Resources.Load<Texture2D>($"Sprites/Elements/{cardModel.secondaryElement?.ToString().ToLowerInvariant()}"));

        MeshRenderer firstAbilityElementMeshRenderer = cardRoot.transform.Find("FrontUIContainer/Footer/Abilities/FirstAbility/FirstAbilityElement").GetComponent<MeshRenderer>();
        firstAbilityElementMeshRenderer.material.SetTexture("_BaseMap", Resources.Load<Texture2D>($"Sprites/Elements/{cardModel.abilities.firstAbility.element}"));

        MeshRenderer secondAbilityElementMeshRenderer = cardRoot.transform.Find("FrontUIContainer/Footer/Abilities/SecondAbility/SecondAbilityElement").GetComponent<MeshRenderer>();
        secondAbilityElementMeshRenderer.material.SetTexture("_BaseMap", Resources.Load<Texture2D>($"Sprites/Elements/{cardModel.abilities.secondAbility.element}"));

        MeshRenderer thirdAbilityElementMeshRenderer = cardRoot.transform.Find("FrontUIContainer/Footer/Abilities/ThirdAbility/ThirdAbilityElement").GetComponent<MeshRenderer>();
        thirdAbilityElementMeshRenderer.material.SetTexture("_BaseMap", Resources.Load<Texture2D>($"Sprites/Elements/{cardModel.abilities.thirdAbility.element}"));

        MeshRenderer fourthAbilityElementMeshRenderer = cardRoot.transform.Find("FrontUIContainer/Footer/Abilities/FourthAbility/FourthAbilityElement").GetComponent<MeshRenderer>();
        fourthAbilityElementMeshRenderer.material.SetTexture("_BaseMap", Resources.Load<Texture2D>($"Sprites/Elements/{cardModel.abilities.fourthAbility.element}"));
    }

    public static void SetAbilityElementsImagesUIToolkit(VisualElement cardRoot, CardModel cardModel)
    {
        cardRoot.Q<VisualElement>("firstAbilityElement").style.backgroundImage =
            new StyleBackground(Resources.Load<Sprite>($"Sprites/Elements/{cardModel.abilities.firstAbility.element.ToString().ToLowerInvariant()}"));

        cardRoot.Q<VisualElement>("secondAbilityElement").style.backgroundImage =
            new StyleBackground(Resources.Load<Sprite>($"Sprites/Elements/{cardModel.abilities.secondAbility.element.ToString().ToLowerInvariant()}"));

        cardRoot.Q<VisualElement>("thirdAbilityElement").style.backgroundImage =
            new StyleBackground(Resources.Load<Sprite>($"Sprites/Elements/{cardModel.abilities.thirdAbility.element.ToString().ToLowerInvariant()}"));

        cardRoot.Q<VisualElement>("fourthAbilityElement").style.backgroundImage =
            new StyleBackground(Resources.Load<Sprite>($"Sprites/Elements/{cardModel.abilities.fourthAbility.element.ToString().ToLowerInvariant()}"));
    }

    public static void BindVisualElementWithSvg(VisualElement visualElement, string xmlCode)
    {
        Texture2D texture = SvgRenderer.SvgToTexture(xmlCode);
        visualElement.style.backgroundImage = new StyleBackground(texture);
    }

    public static void BindHPWithBattleState3D(GameObject cardRoot, CardModel cardModel)
    {
        TextMeshPro healthValue = cardRoot.transform.Find("FrontUIContainer/Body/Health/HealthValue").GetComponent<TextMeshPro>();
        if (cardRoot.TryGetComponent<CardBattleState>(out var battleState))
        {
            battleState.CurrentHP = cardModel.health;
            battleState.OnHPChanged += (newXP) => { healthValue.text = newXP.ToString(); };
            healthValue.text = battleState.CurrentHP.ToString();
        }
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
