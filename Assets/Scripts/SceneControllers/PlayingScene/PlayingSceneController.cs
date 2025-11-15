using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayingSceneController : MonoBehaviour
{
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Transform cardsParent;
    [SerializeField] private Button backButton;

    private BattleCardsListController battleFiledCardsListController;

    private async void Start()
    {
        CardControllerFactory.Init(prefab: cardPrefab, parent: cardsParent);
        battleFiledCardsListController = new BattleCardsListController(cardsParent);
        await battleFiledCardsListController.LoadUserCards();
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
