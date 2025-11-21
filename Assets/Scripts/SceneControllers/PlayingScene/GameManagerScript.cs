using System.Collections.Generic;
using UnityEngine;

public class Game
{
    public List<CardModel> PlayerDeck { get; set; }
    public List<CardModel> EnemyDeck { get; set; }

    public List<BattleCardController> enemyHand, playerHand, enemyField, playerField;

    public Game()
    {
        // playerHand = new();
        // enemyHand = new();

        // playerField = new();
        // enemyField = new();
    }
}

public class GameManagerScript : MonoBehaviour
{
    [SerializeField] private Transform enemyHand, playerHand;

    private BattleCardListController playerCardListController, enemyCardListController;
    [SerializeField] private GameObject cardPrefab;

    public Game currentGame;

    void Start()
    {
        currentGame = new Game();

        CardControllerFactory.Init(prefab: cardPrefab);

        LoadCardsToPlayerHands();
        LoadCardsToEnemyHands();

        currentGame.PlayerDeck = playerCardListController.CardModels;
        currentGame.EnemyDeck = enemyCardListController.CardModels;
    }

    public async void LoadCardsToPlayerHands()
    {
        playerCardListController = new(playerHand);
        await playerCardListController.LoadPlayerCards();
    }

    public async void LoadCardsToEnemyHands()
    {
        enemyCardListController = new(enemyHand);
        await enemyCardListController.LoadEnemyCards();
    }
}