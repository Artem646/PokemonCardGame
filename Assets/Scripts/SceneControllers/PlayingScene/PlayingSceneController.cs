using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayingSceneController : MonoBehaviour
{
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Transform handContainer;
    [SerializeField] private Button backButton;
    [SerializeField] private GameObject cardOverlay;


    private BattleCardsListController battleFiledCardsListController;

    private async void Start()
    {
        CardControllerFactory.Init(prefab: cardPrefab, parent: handContainer);
        battleFiledCardsListController = new BattleCardsListController(handContainer);
        await battleFiledCardsListController.LoadUserCards();
        CardOverlayManager.Instance.Init(cardOverlay);
        RegisterCallbacks();
    }

    private void RegisterCallbacks()
    {
        backButton.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("CollectionScene");
        });
    }
}
