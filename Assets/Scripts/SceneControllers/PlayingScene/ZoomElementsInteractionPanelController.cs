using UnityEngine;
using UnityEngine.UI;

public class ZoomElementsInteractionPanelController : MonoBehaviour
{
    [SerializeField] private TypeChart typeChart;
    [SerializeField] private GameObject elementIconPrefab;

    [SerializeField] private GameObject elementsInteractionPanel;

    [SerializeField] private GameObject doubleHighDamageSection;
    [SerializeField] private Transform doubleHighDamageIconsContainer;

    [SerializeField] private GameObject highDamageSection;
    [SerializeField] private Transform highDamageIconsContainer;

    [SerializeField] private GameObject defaultDamageSection;
    [SerializeField] private Transform defaultDamageIconsContainer;

    [SerializeField] private GameObject halfDamageSection;
    [SerializeField] private Transform halfDamageIconsContainer;

    [SerializeField] private GameObject quarterDamageSection;
    [SerializeField] private Transform quarterDamageIconsContainer;

    [SerializeField] private GameObject zeroDamageSection;
    [SerializeField] private Transform zeroDamageIconsContainer;

    public void UpdatePanel(CardModel cardModel)
    {
        ClearContainer(doubleHighDamageIconsContainer);
        ClearContainer(highDamageIconsContainer);
        ClearContainer(defaultDamageIconsContainer);
        ClearContainer(halfDamageIconsContainer);
        ClearContainer(quarterDamageIconsContainer);
        ClearContainer(zeroDamageIconsContainer);

        bool hasDoubleHighDamage = false;
        bool hasHighDamage = false;
        bool hasDefaultDamage = false;
        bool hasHalfDamage = false;
        bool hasQuarterDamage = false;
        bool hasZeroDamage = false;

        foreach (PokemonElement element in System.Enum.GetValues(typeof(PokemonElement)))
        {
            float multiplier = typeChart.GetMultiplier(element, cardModel.mainElement);
            if (cardModel.secondaryElement.HasValue)
                multiplier *= typeChart.GetMultiplier(element, cardModel.secondaryElement.Value);

            if (multiplier == 4f)
            {
                CreateIcon(element, doubleHighDamageIconsContainer);
                hasDoubleHighDamage = true;
            }
            else if (multiplier == 2f)
            {
                CreateIcon(element, highDamageIconsContainer);
                hasHighDamage = true;
            }
            else if (multiplier == 1f)
            {
                CreateIcon(element, defaultDamageIconsContainer);
                hasDefaultDamage = true;
            }
            else if (multiplier == 0.5f)
            {
                CreateIcon(element, halfDamageIconsContainer);
                hasHalfDamage = true;
            }
            else if (multiplier == 0.25f)
            {
                CreateIcon(element, quarterDamageIconsContainer);
                hasQuarterDamage = true;
            }
            else if (multiplier == 0f)
            {
                CreateIcon(element, zeroDamageIconsContainer);
                hasZeroDamage = true;
            }
        }

        doubleHighDamageSection.SetActive(hasDoubleHighDamage);
        highDamageSection.SetActive(hasHighDamage);
        defaultDamageSection.SetActive(hasDefaultDamage);
        halfDamageSection.SetActive(hasHalfDamage);
        quarterDamageSection.SetActive(hasQuarterDamage);
        zeroDamageSection.SetActive(hasZeroDamage);
    }
    private void CreateIcon(PokemonElement element, Transform container)
    {
        GameObject iconContainer = Instantiate(elementIconPrefab, container);
        if (iconContainer.TryGetComponent<Image>(out var iconImage))
            iconImage.sprite = Resources.Load<Sprite>($"Sprites/Elements/{element}");
    }

    private void ClearContainer(Transform container)
    {
        foreach (Transform child in container)
            Destroy(child.gameObject);
    }
}