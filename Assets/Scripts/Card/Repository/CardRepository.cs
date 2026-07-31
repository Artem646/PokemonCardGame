using UnityEngine;
using System;
using System.Threading.Tasks;
using UnityEngine.UIElements;
using System.Collections.Generic;

[RequireComponent(typeof(UIDocument))]
public class CardRepository : MonoBehaviour
{
    public static CardRepository Instance { get; private set; }

    [SerializeField] private TextAsset cardsJson;

    private CardRepositoryService service;

    private VisualElement intermediateCardContainer;
    private GameObject battleCardContainer;

    public event Action OnCardsLoaded;
    public event Action<float> OnProgressChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        service = new CardRepositoryService(cardsJson);
        service.OnCardsLoaded += () => OnCardsLoaded?.Invoke();
        service.OnProgressChanged += (p) => OnProgressChanged?.Invoke(p);

        intermediateCardContainer = new VisualElement { name = "intermediateCardContainer" };
        intermediateCardContainer.style.display = DisplayStyle.None;
        GetComponent<UIDocument>().rootVisualElement.Add(intermediateCardContainer);

        battleCardContainer = new GameObject("BattleCardContainer");
        battleCardContainer.transform.position = new Vector3(0.2f, 2.65436f, 54.3f);
        battleCardContainer.transform.SetParent(transform);
        // battleCardContainer.SetActive(false);
    }

    public void AddCardsToIntermediateCardContainer()
    {
        List<CollectionCardController> gameCollectionCardControllersList = service.GetGameCollectionCardControllersList();
        foreach (CollectionCardController cardController in gameCollectionCardControllersList)
        {
            ICollectionCardView cloneCardView = cardController.CollectionCardView;
            cloneCardView.RemoveAllAddedStyles();
            intermediateCardContainer.Add(cloneCardView.CardRoot);
        }
    }

    public async Task LoadGameCards() => await service.LoadGameCards();

    public void PreBuildGameCollectionCardControllers(VisualTreeAsset cardTemplate)
    {
        service.PreBuildGameCollectionCardControllers(cardTemplate);
        AddCardsToIntermediateCardContainer();
    }

    public async Task PreBuildBattleCardControllers(GameObject cardPrefab3D, List<int> requiredCardIds)
    {
        await service.PreBuildBattleCardControllers(cardPrefab3D, battleCardContainer, requiredCardIds);
    }

    public void BuildNewBattleCardController(int newCardId)
    {
        service.BuildNewBattleCardController(battleCardContainer, newCardId);
    }

    public void PreBuildBattleCardControllersInDescriptionScene(GameObject cardPrefab3D, List<int> requiredCardIds)
    {
        service.PreBuildBattleCardControllersInDescriptionScene(cardPrefab3D, battleCardContainer, requiredCardIds);
    }

    public async Task LoadUserCardsCollection() => await service.LoadUserCardsToCollection();

    public CardModel GetGameCardModelById(int id) => service.GetGameCardModelById(id);
    public CardModel GetUserCardModelById(int id) => service.GetUserCardModelById(id);

    public CollectionCardController GetCollectionCardControllerById(int id) => service.GetCollectionCardControllerById(id);
    public BattleCardController GetBattleCardControllerById(int id) => service.GetBattleCardControllerById(id);

    public GameCardModelList GetGameCardsList() => service.GetGameCardModelList();
    public UserCardModelList GetUserCardsList() => service.GetUserCardModelList();

    public List<CollectionCardController> GetGameCollectionCardControllersList() => service.GetGameCollectionCardControllersList();
    public List<BattleCardController> GetBattleCardControllersList() => service.GetBattleCardControllersList();

    public void ClearUserCards() => service.ClearUserCardsList();
}
