using UnityEngine;
using UnityEngine.UI;

public class CardGameView : MonoBehaviour
{
    public CardModel cardModel;
    public GameObject cardPrefab;

    private GameObject cardInstance;
    private Image backgroundImage;
    private Text titleText;
    private Button addToDeckButton;
    private Image pokemonImage;
    private Image mainElementImage;
    private Image secondaryElementImage;

    public void Initialize()
    {
        cardInstance = Instantiate(cardPrefab, transform);
        backgroundImage = cardInstance.transform.Find("Background").GetComponent<Image>();
        titleText = cardInstance.transform.Find("Title").GetComponent<Text>();
        addToDeckButton = cardInstance.transform.Find("AddToDeckButton").GetComponent<Button>();
        pokemonImage = cardInstance.transform.Find("PokemonImage").GetComponent<Image>();
        mainElementImage = cardInstance.transform.Find("MainElement").GetComponent<Image>();
        secondaryElementImage = cardInstance.transform.Find("SecondaryElement").GetComponent<Image>();

        BindData();
    }

    private void BindData()
    {
        backgroundImage.color = cardModel.colors.cardColor;
        titleText.text = cardModel.title;

        SetImages();

        Color tint = new Color(0.5f, 0.5f, 0.5f);
        addToDeckButton.image.color = tint;
    }

    private void SetImages()
    {
        pokemonImage.sprite = Resources.Load<Sprite>($"Sprites/PokemonImages/{cardModel.imageName}");
        mainElementImage.sprite = Resources.Load<Sprite>($"Sprites/Elements/{cardModel.mainElement.ToString().ToLowerInvariant()}");

        if (cardModel.secondaryElement != null)
        {
            secondaryElementImage.sprite = Resources.Load<Sprite>($"Sprites/Elements/{cardModel.secondaryElement.ToString().ToLowerInvariant()}");
        }
        else
        {
            secondaryElementImage.gameObject.SetActive(false);
        }
    }

    public void SetAddedToDeck(bool isAdded)
    {
        addToDeckButton.image.color = isAdded ? Color.green : Color.gray;
    }

    public void SetOpacity(bool isVisible)
    {
        CanvasGroup group = cardInstance.GetComponent<CanvasGroup>();
        if (group == null)
        {
            group = cardInstance.AddComponent<CanvasGroup>();
        }
        group.alpha = isVisible ? 1f : 0.3f;
    }

    public void SetStyleForBattle()
    {
        cardInstance.transform.localScale = new Vector3(0.8f, 0.8f, 1f);
        addToDeckButton.gameObject.SetActive(false);
    }

    public GameObject CardInstance => cardInstance;
}
