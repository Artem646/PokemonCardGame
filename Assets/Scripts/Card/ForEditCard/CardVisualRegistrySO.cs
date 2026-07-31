using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "CardVisualRegistry", menuName = "Cards/Registry")]
public class CardVisualRegistrySO : ScriptableObject
{
    private static CardVisualRegistrySO _instance;
    public static CardVisualRegistrySO Instance
    {
        get
        {
            if (_instance == null)
                _instance = Resources.Load<CardVisualRegistrySO>("CardVisualRegistry");
            return _instance;
        }
    }

    [SerializeField] private List<CardVisualDataSO> visualDatas = new();

    private Dictionary<int, CardVisualDataSO> cache;

    public CardVisualDataSO GetVisualData(int cardId)
    {
        if (cache == null || cache.Count == 0)
            InitializeCache();

        if (cache.TryGetValue(cardId, out var data))
            return data;

        return null;
    }

    private void InitializeCache()
    {
        cache = new Dictionary<int, CardVisualDataSO>();

        if (visualDatas == null) return;

        foreach (CardVisualDataSO data in visualDatas)
        {
            if (data != null && !cache.ContainsKey(data.cardId))
                cache.Add(data.cardId, data);
        }

        Debug.Log($"[Registry] Словарь успешно собран. Элементов: {cache.Count}");
    }
}