using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Game
{
    public List<CardModel> PlayerDeck { get; set; }
    public List<CardModel> EnemyDeck { get; set; }

    public List<BattleCardController> enemyHand, playerHand, enemyField, playerField;

    public Game() { }
}

public class GameManagerScript : MonoBehaviour
{
    [SerializeField] private Transform enemyHand, playerHand;
    [SerializeField] private Transform enemyField, playerField;
    [SerializeField] private GameObject cardPrefab;

    private BattleCardListController playerCardListController, enemyCardListController;
    public Game currentGame;

    private NetworkGameState networkGameState;

    public void Init(NetworkGameState state)
    {
        networkGameState = state;
        currentGame = new Game();
        CardControllerFactory.Init(prefab: cardPrefab);

        Debug.Log($"[GameManager] PlayerDeckCsv={networkGameState.FirstPlayerDeckCsv}, EnemyDeckCsv={networkGameState.SecondPlayerDeckCsv}");

        if (networkGameState != null)
        {
            if (networkGameState.Runner.IsServer)
            {
                LoadCardsToPlayerHand(networkGameState.GetPlayerDeckIds());
                LoadCardsToEnemyHand(networkGameState.GetEnemyDeckIds());
            }
            else
            {
                LoadCardsToPlayerHand(networkGameState.GetEnemyDeckIds());
                LoadCardsToEnemyHand(networkGameState.GetPlayerDeckIds());
            }
        }
    }

    public async void LoadCardsToPlayerHand(List<int> playerIds)
    {
        Debug.Log($"[GameManager] LoadCardsToPlayerHands → ids={string.Join(",", playerIds)}");
        await LoadPlayerCardsByIds(playerIds);
        currentGame.PlayerDeck = playerCardListController.CardModels;
        Debug.Log($"[GameManager] PlayerDeck count={currentGame.PlayerDeck.Count}");
    }

    public async void LoadCardsToEnemyHand(List<int> enemyIds)
    {
        Debug.Log($"[GameManager] LoadCardsToEnemyHands → ids={string.Join(",", enemyIds)}");
        await LoadEnemyCardsByIds(enemyIds);
        currentGame.EnemyDeck = enemyCardListController.CardModels;
        Debug.Log($"[GameManager] EnemyDeck count={currentGame.EnemyDeck.Count}");
    }

    public async Task LoadPlayerCardsByIds(List<int> ids)
    {
        playerCardListController ??= new(playerHand);
        await playerCardListController.LoadCardsByIds(ids);
        Debug.Log($"[GameManager] PlayerHand загружен: {string.Join(",", ids)}");
    }

    public async Task LoadEnemyCardsByIds(List<int> ids)
    {
        enemyCardListController ??= new(enemyHand);
        await enemyCardListController.LoadCardsByIds(ids);
        Debug.Log($"[GameManager] EnemyHand загружен: {string.Join(",", ids)}");
    }

    public async void OnCardPlayed(int cardId, int siblingIndex, bool isFirstPlayer, bool toField)
    {
        Debug.Log(cardId + " " + siblingIndex + " " + isFirstPlayer + " " + toField);
        bool firstPlayer = networkGameState.Runner.IsServer;

        if (firstPlayer != isFirstPlayer)
        {
            Transform containerTo, containerFrom;

            if (toField)
            {
                containerTo = enemyField;
                containerFrom = enemyHand;
            }
            else
            {
                containerTo = enemyHand;
                containerFrom = enemyField;
            }

            Debug.Log($"containerTo: {containerTo}, containerFrom: {containerFrom}");
            RemoveCardFromContainer(cardId, containerFrom);

            var controller = new BattleCardListController(containerTo);
            await controller.LoadCardsByIds(new List<int> { cardId });

            var newCard = containerTo.GetChild(containerTo.childCount - 1);
            newCard.SetSiblingIndex(siblingIndex);

            if (toField)
            {
                newCard.GetComponent<CardFlipScript>().FlipToFaceUp();
            }
            else
            {
                newCard.GetComponent<CardFlipScript>().FlipToFaceDown();
            }
        }
    }

    private void RemoveCardFromContainer(int cardId, Transform container)
    {
        Debug.Log("Remove");
        foreach (Transform child in container)
        {
            var cardScript = child.GetComponent<CardMovemantScript>();
            if (cardScript != null && cardScript.CardId == cardId)
            {
                Destroy(child.gameObject);
                Debug.Log($"[GameManager] Карта {cardId} удалена из контейнера");
                break;
            }
        }
    }

    // Вызов из UI при клике/перетаскивании
    public void RequestPlayCard(int cardId, int siblingIndex, bool toField)
    {
        networkGameState.RpcRequestPlayCard(cardId, siblingIndex, toField);
    }
}