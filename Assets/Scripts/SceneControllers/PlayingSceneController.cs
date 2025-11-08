using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class PlaySceneController : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private VisualTreeAsset cardTemplate;
    private BattleFiledCardsListController battleFiledCardsListController;
    private VisualElement deckContainer;

    private async void Start()
    {
        deckContainer = uiDocument.rootVisualElement.Q<VisualElement>("deckContainer");
        battleFiledCardsListController = new BattleFiledCardsListController(deckContainer);
        await battleFiledCardsListController.LoadUserCards();

        uiDocument.rootVisualElement.Q<Button>("back").RegisterCallback<ClickEvent>(evt =>
        {
            SceneManager.LoadScene("CollectionScene");
        });
    }
}