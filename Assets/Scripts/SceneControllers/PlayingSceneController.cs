using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class PlaySceneController : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    private VisualElement deckContainer;

    private void Start()
    {
        deckContainer = uiDocument.rootVisualElement.Q<VisualElement>("deckContainer");
        UpdateDeckPreview();

        uiDocument.rootVisualElement.Q<Button>("back").RegisterCallback<ClickEvent>(evt =>
        {
            SceneManager.LoadScene("CollectionScene");
        });
    }

    private void UpdateDeckPreview()
    {
        deckContainer.Clear();

        List<VisualElement> deckCards = CardDeck.Instance.GetDeckCards();

        foreach (VisualElement card in deckCards)
        {
            deckContainer.Add(card);
        }
    }
}