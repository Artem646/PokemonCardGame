using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;

public class ZoomEnemyFieldCardsElementsPanelController : MonoBehaviour
{
    [SerializeField] private GameObject elementIconPrefab;
    [SerializeField] private GameObject elementsInteractionRowPrefab;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private TypeChart typeChart;
    [SerializeField] private List<GameObject> elementsIconsContainers;
    [SerializeField] private Sprite noCardOnSlotSprite;

    [SerializeField] private CanvasGroup panelCanvasGroup;
    [SerializeField] private GameObject LineContainer;

    [SerializeField] private GameObject firstAbilitySection;
    [SerializeField] private GameObject firstAbilityHeaderText;
    [SerializeField] private Transform firstAbilityInteractionRowsContainer;

    [SerializeField] private GameObject secondAbilitySection;
    [SerializeField] private GameObject secondAbilityHeaderText;
    [SerializeField] private Transform secondAbilityInteractionRowsContainer;

    [SerializeField] private GameObject fourthAbilitySection;
    [SerializeField] private GameObject fourthAbilityHeaderText;
    [SerializeField] private Transform fourthAbilityInteractionRowsContainer;

    private Color highEffectiveColor = new(0.2346053f, 0.6075471f, 0.1593378f, 1f);
    private Color neutralEffectiveColor = new(0.7735849f, 0.7496783f, 0.1036312f, 1f);
    private Color notEffectiveColor = new(0.8f, 0.2f, 0.2f, 1f);
    private Color zeroEffectiveColor = new(0.2f, 0.2f, 0.2f, 1f);

    private BattleCardController[] enemyFieldCards;

    public void UpdatePanel(CardSlot currentSlotInZoom, BattleCardController zoomedCard)
    {
        UpdateEnemyFieldIcons();
        if (currentSlotInZoom.type == FieldSlotType.SelfFieldSlot || currentSlotInZoom.type == FieldSlotType.SelfHandSlot)
            UpdateInteractionRows(zoomedCard);
    }

    private void UpdateEnemyFieldIcons()
    {
        for (int i = 0; i < elementsIconsContainers.Count; i++)
            ClearContainer(elementsIconsContainers[i].transform);

        enemyFieldCards = gameManager.CurrentGame.EnemyFieldControllers;

        for (int i = 0; i < elementsIconsContainers.Count; i++)
        {
            if (enemyFieldCards[i] != null)
            {
                CreateElementIcon(enemyFieldCards[i].CardModel.mainElement, elementsIconsContainers[i].transform);
                if (enemyFieldCards[i].CardModel.secondaryElement.HasValue)
                    CreateElementIcon(enemyFieldCards[i].CardModel.secondaryElement.Value, elementsIconsContainers[i].transform);
            }
            else
            {
                GameObject iconContainer = Instantiate(elementIconPrefab, elementsIconsContainers[i].transform);
                if (iconContainer.TryGetComponent<Image>(out var iconImage))
                    iconImage.sprite = noCardOnSlotSprite;
            }
        }
    }

    private void UpdateInteractionRows(BattleCardController zoomedCard)
    {
        ClearContainer(firstAbilityInteractionRowsContainer);
        ClearContainer(secondAbilityInteractionRowsContainer);
        ClearContainer(fourthAbilityInteractionRowsContainer);

        bool hasFirstAbility = false;
        bool hasSecondAbility = false;
        bool hasFourthAbility = false;

        List<AbilityController> attackingAbilities = zoomedCard.AbilityControllers
            .Where(a => (a.AbilityModel.type == AbilityType.Physical || a.AbilityModel.type == AbilityType.Special)
                && a.AbilityState != AbilityState.AlreadyUsedOnce).ToList();

        foreach (AbilityController ability in attackingAbilities)
        {
            if (ability.AbilityIndex == 0)
            {
                TextMeshProUGUI headerText = firstAbilityHeaderText.GetComponent<TextMeshProUGUI>();
                headerText.text = $"Способность {ability.AbilityIndex + 1}: {ability.AbilityModel.name}";

                for (int i = 0; i < enemyFieldCards.Length; i++)
                {
                    if (enemyFieldCards[i] == null) continue;

                    float multiplier = typeChart.GetMultiplier(ability.AbilityModel.element.Value, enemyFieldCards[i].CardModel.mainElement);
                    if (enemyFieldCards[i].CardModel.secondaryElement.HasValue)
                        multiplier *= typeChart.GetMultiplier(ability.AbilityModel.element.Value, enemyFieldCards[i].CardModel.secondaryElement.Value);

                    CreateInteractionRow(ability, enemyFieldCards[i], multiplier, firstAbilityInteractionRowsContainer);
                }

                hasFirstAbility = true;
            }
            else if (ability.AbilityIndex == 1)
            {
                TextMeshProUGUI headerText = secondAbilityHeaderText.GetComponent<TextMeshProUGUI>();
                headerText.text = $"Способность {ability.AbilityIndex + 1}: {ability.AbilityModel.name}";

                for (int i = 0; i < enemyFieldCards.Length; i++)
                {
                    if (enemyFieldCards[i] == null) continue;

                    float multiplier = typeChart.GetMultiplier(ability.AbilityModel.element.Value, enemyFieldCards[i].CardModel.mainElement);
                    if (enemyFieldCards[i].CardModel.secondaryElement.HasValue)
                        multiplier *= typeChart.GetMultiplier(ability.AbilityModel.element.Value, enemyFieldCards[i].CardModel.secondaryElement.Value);

                    CreateInteractionRow(ability, enemyFieldCards[i], multiplier, secondAbilityInteractionRowsContainer);
                }

                hasSecondAbility = true;
            }
            else if (ability.AbilityIndex == 3)
            {
                TextMeshProUGUI headerText = fourthAbilityHeaderText.GetComponent<TextMeshProUGUI>();
                headerText.text = $"Способность {ability.AbilityIndex + 1}: {ability.AbilityModel.name}";

                for (int i = 0; i < enemyFieldCards.Length; i++)
                {
                    if (enemyFieldCards[i] == null) continue;

                    float multiplier = typeChart.GetMultiplier(ability.AbilityModel.element.Value, enemyFieldCards[i].CardModel.mainElement);
                    if (enemyFieldCards[i].CardModel.secondaryElement.HasValue)
                        multiplier *= typeChart.GetMultiplier(ability.AbilityModel.element.Value, enemyFieldCards[i].CardModel.secondaryElement.Value);

                    CreateInteractionRow(ability, enemyFieldCards[i], multiplier, fourthAbilityInteractionRowsContainer);
                }

                hasFourthAbility = true;
            }
        }

        if (!enemyFieldCards.AnyNotNull())
        {
            panelCanvasGroup.alpha = 0;
            LineContainer.SetActive(false);
        }
        else
        {
            panelCanvasGroup.alpha = 1;
            LineContainer.SetActive(true);

            firstAbilitySection.SetActive(hasFirstAbility);
            secondAbilitySection.SetActive(hasSecondAbility);
            fourthAbilitySection.SetActive(hasFourthAbility);
        }
    }

    private void CreateElementIcon(PokemonElement element, Transform container)
    {
        GameObject elementIcon = Instantiate(elementIconPrefab, container);
        if (elementIcon.TryGetComponent<Image>(out var elementIconImage))
            elementIconImage.sprite = Resources.Load<Sprite>($"Sprites/Elements/{element}");
    }

    private void CreateInteractionRow(AbilityController ability, BattleCardController enemy, float multiplier, Transform interactionRowsContainer)
    {
        GameObject interactionRow = Instantiate(elementsInteractionRowPrefab, interactionRowsContainer);

        Transform abilityElementIconContainer = interactionRow.transform.Find("AbilityElementIconContainer");
        CreateElementIcon(ability.AbilityModel.element.Value, abilityElementIconContainer);

        Transform enemyElementsIconsContainer = interactionRow.transform.Find("EnemyElementsIconsContainer");
        CreateElementIcon(enemy.CardModel.mainElement, enemyElementsIconsContainer);
        if (enemy.CardModel.secondaryElement.HasValue)
            CreateElementIcon(enemy.CardModel.secondaryElement.Value, enemyElementsIconsContainer);

        TextMeshProUGUI multiplierText = interactionRow.transform.Find("MultiplierTextContainer/MultiplierText").GetComponent<TextMeshProUGUI>();
        multiplierText.text = $"{multiplier}x";

        if (multiplier == 4f) { multiplierText.color = highEffectiveColor; }
        else if (multiplier == 2f) { multiplierText.color = highEffectiveColor; }
        else if (multiplier == 1f) { multiplierText.color = neutralEffectiveColor; }
        else if (multiplier == 0.5f) { multiplierText.color = notEffectiveColor; }
        else if (multiplier == 0.25f) { multiplierText.color = notEffectiveColor; }
        else if (multiplier == 0f) { multiplierText.color = zeroEffectiveColor; }
    }

    private void ClearContainer(Transform container)
    {
        foreach (Transform child in container)
            Destroy(child.gameObject);
    }
}