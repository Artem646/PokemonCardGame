using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CardRepositoryService
{
    private GameCardModelList gameCardsList = new();
    private UserCardModelList userCardsList = new();

    private Dictionary<int, CardModel> gameCardModelCache = new();
    private Dictionary<int, CardModel> userCardModelCache = new();

    private Dictionary<int, CollectionCardController> gameCollectionCardControllerCache = new();
    private Dictionary<int, Queue<BattleCardController>> battleCardControllerCache = new();

    private readonly TextAsset cardsJson;

    public event Action OnCardsLoaded;
    public event Action<float> OnProgressChanged;

    public CardRepositoryService(TextAsset cardsJson) { this.cardsJson = cardsJson; }

    public async Task LoadGameCards()
    {
        try
        {
            string cardsJsonText = cardsJson.text;
            gameCardsList = await Task.Run(() => CardsLoader.GetCardsListFromJson(cardsJsonText));
            gameCardModelCache = gameCardsList.cards.ToDictionary(card => card.id, card => card);

            Debug.Log($"[P] Загружено {gameCardsList.cards.Count} карт из JSON.");
        }
        catch (Exception e)
        {
            Debug.LogError($"[P] Ошибка загрузки JSON: {e}");
        }
    }

    public async Task LoadUserCardsToCollection()
    {
        List<int> cardIds = UserSession.Instance.ActiveUser.cardsInCollection;
        userCardsList.cards.Clear();

        if (cardIds == null || cardIds.Count == 0)
        {
            OnCardsLoaded?.Invoke();
            return;
        }

        await AddCardsToCollectionByIds(cardIds);
        Debug.Log($"[P] Успешно загружено {userCardsList.cards.Count} карт.");
        OnCardsLoaded?.Invoke();
    }

    private async Task AddCardsToCollectionByIds(List<int> cardIds)
    {
        int total = cardIds.Count;
        int loaded = 0;

        foreach (int id in cardIds)
        {
            CardModel model = GetGameCardModelById(id);
            if (model != null) userCardsList.cards.Add(model);

            loaded++;
            float progress = (float)loaded / total;
            OnProgressChanged?.Invoke(progress);

            await Task.Delay(10);
        }

        userCardModelCache = userCardsList.cards.ToDictionary(card => card.id, card => card);
        OnProgressChanged?.Invoke(1f);
    }

    public void PreBuildGameCollectionCardControllers(VisualTreeAsset cardTemplate)
    {
        if (gameCollectionCardControllerCache.Count > 0) return;
        CardControllerFactory.Init(template: cardTemplate);

        foreach (CardModel model in gameCardsList.cards)
        {
            CollectionCardController controller = CardControllerFactory.Create<CollectionCardController>(model);
            controller.CollectionCardView.CardRoot.userData = model;
            gameCollectionCardControllerCache.Add(model.id, controller);
        }
    }

    public async Task PreBuildBattleCardControllers(GameObject cardPrefab3D, GameObject battleCardContainer, List<int> requiredCardIds)
    {
        ClearBattleCardCache();

        CardControllerFactory.Init(prefab3D: cardPrefab3D);

        int total = requiredCardIds.Count;
        int loaded = 0;

        int cardsPerRow = 10;
        float spacingX = 7f;
        float spacingZ = 8.5f;

        int currentColumn = 0;
        int currentRow = 0;

        foreach (int id in requiredCardIds)
        {
            CardModel model = GetGameCardModelById(id);
            BattleCardController battleCardController = CardControllerFactory.Create<BattleCardController>(model);

            if (!battleCardControllerCache.ContainsKey(model.id))
                battleCardControllerCache[model.id] = new Queue<BattleCardController>();
            battleCardControllerCache[model.id].Enqueue(battleCardController);

            float posX = currentColumn * spacingX;
            float posZ = currentRow * spacingZ;

            GameObject cardRoot = battleCardController.BattleCardView.CardRoot;
            Transform cardTransform = cardRoot.transform;
            cardTransform.localScale = new Vector3(50, 125, 115);
            cardTransform.SetParent(battleCardContainer.transform);
            cardTransform.SetLocalPositionAndRotation(new Vector3(posX, 0, posZ), Quaternion.Euler(0, 0, 90));

            currentColumn++;
            if (currentColumn >= cardsPerRow)
            {
                currentColumn = 0;
                currentRow++;
            }

            // cardRoot.SetActive(false);

            loaded++;
            float progress = (float)loaded / total;
            OnProgressChanged?.Invoke(progress);

            await Task.Yield();
        }

        OnProgressChanged?.Invoke(1f);
        OnCardsLoaded?.Invoke();
    }

    public void BuildNewBattleCardController(GameObject battleCardContainer, int newCardId)
    {
        CardModel model = GetGameCardModelById(newCardId);
        BattleCardController battleCardController = CardControllerFactory.Create<BattleCardController>(model);

        if (!battleCardControllerCache.ContainsKey(model.id))
            battleCardControllerCache[model.id] = new Queue<BattleCardController>();
        battleCardControllerCache[model.id].Enqueue(battleCardController);

        GameObject cardRoot = battleCardController.BattleCardView.CardRoot;
        Transform cardTransform = cardRoot.transform;
        cardTransform.localScale = new Vector3(50, 125, 115);
        cardTransform.SetParent(battleCardContainer.transform);
        cardTransform.SetLocalPositionAndRotation(new Vector3(0, 0, 0), Quaternion.Euler(0, 0, 90));
    }

    public void PreBuildBattleCardControllersInDescriptionScene(GameObject cardPrefab3D, GameObject battleCardContainer, List<int> requiredCardIds)
    {
        ClearBattleCardCache();

        CardControllerFactory.Init(prefab3D: cardPrefab3D);

        int cardsPerRow = 10;
        float spacingX = 6f;
        float spacingZ = 7.5f;

        int currentColumn = 0;
        int currentRow = 0;

        foreach (int id in requiredCardIds)
        {
            CardModel model = GetGameCardModelById(id);
            BattleCardController battleCardController = CardControllerFactory.Create<BattleCardController>(model);

            if (!battleCardControllerCache.ContainsKey(model.id))
                battleCardControllerCache[model.id] = new Queue<BattleCardController>();
            battleCardControllerCache[model.id].Enqueue(battleCardController);

            float posX = currentColumn * spacingX;
            float posZ = currentRow * spacingZ;

            GameObject cardRoot = battleCardController.BattleCardView.CardRoot;
            Transform cardTransform = cardRoot.transform;
            cardTransform.SetParent(battleCardContainer.transform);
            cardTransform.SetLocalPositionAndRotation(new Vector3(posX, 0, posZ), Quaternion.Euler(0, 0, 90));

            currentColumn++;
            if (currentColumn >= cardsPerRow)
            {
                currentColumn = 0;
                currentRow++;
            }
        }
    }

    public CardModel GetGameCardModelById(int id) => gameCardModelCache.TryGetValue(id, out var card) ? card : null;
    public CardModel GetUserCardModelById(int id) => userCardModelCache.TryGetValue(id, out var card) ? card : null;

    public CollectionCardController GetCollectionCardControllerById(int id) => gameCollectionCardControllerCache.TryGetValue(id, out var c) ? c : null;
    public BattleCardController GetBattleCardControllerById(int id)
    {
        if (battleCardControllerCache.TryGetValue(id, out var queue) && queue.Count > 0)
            return queue.Dequeue();
        return null;
    }

    public GameCardModelList GetGameCardModelList() => gameCardsList;
    public UserCardModelList GetUserCardModelList() => userCardsList;

    public List<CollectionCardController> GetGameCollectionCardControllersList() => gameCollectionCardControllerCache.Values.ToList();
    public List<BattleCardController> GetBattleCardControllersList()
    {
        List<BattleCardController> allCards = new();
        foreach (Queue<BattleCardController> queue in battleCardControllerCache.Values)
            allCards.AddRange(queue);
        return allCards;
    }

    public void ClearUserCardsList() => userCardsList.cards.Clear();
    public void ClearBattleCardCache()
    {
        foreach (Queue<BattleCardController> queue in battleCardControllerCache.Values)
        {
            while (queue.Count > 0)
            {
                BattleCardController card = queue.Dequeue();
                if (card != null && card.BattleCardView != null && card.BattleCardView.CardRoot != null)
                    UnityEngine.Object.Destroy(card.BattleCardView.CardRoot);
            }
        }

        battleCardControllerCache.Clear();
    }
}
