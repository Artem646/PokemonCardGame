using UnityEngine;
using System;
using System.Threading.Tasks;

public class CardRepository : MonoBehaviour
{
    public static CardRepository Instance { get; private set; }

    [SerializeField] private TextAsset cardsJson;

    private CardRepositoryService service;

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
    }

    public async Task<UserCardModelList> GetUserCardsCollection()
    {
        return await service.GetUserCardsCollection();
    }

    public async Task<GameCardModelList> GetAllGameCards()
    {
        return await service.GetGameCards();
    }

    public UserCardModelList GetUserCards() => service.GetUserCardsList();
    public GameCardModelList GetGameCards() => service.GetGameCardsList();
}
