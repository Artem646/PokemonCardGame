using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CardUIToolkitVisualRegistry", menuName = "Cards/UIToolkit Registry")]
public class CardUIToolkitVisualRegistrySO : ScriptableObject
{
    private static CardUIToolkitVisualRegistrySO _instance;
    public static CardUIToolkitVisualRegistrySO Instance
    {
        get
        {
            if (_instance == null)
                _instance = Resources.Load<CardUIToolkitVisualRegistrySO>("CardUIToolkitVisualRegistry");
            return _instance;
        }
    }

    [SerializeField] private List<CardUIToolkitVisualDataSO> visualDatas = new();

    private Dictionary<int, CardUIToolkitVisualDataSO> cache;

    public CardUIToolkitVisualDataSO GetVisualData(int cardId)
    {
        if (cache == null || cache.Count == 0)
            InitializeCache();

        if (cache.TryGetValue(cardId, out var data))
            return data;

        return null;
    }

    private void InitializeCache()
    {
        cache = new Dictionary<int, CardUIToolkitVisualDataSO>();

        if (visualDatas == null) return;

        foreach (CardUIToolkitVisualDataSO data in visualDatas)
        {
            if (data != null && !cache.ContainsKey(data.cardId))
                cache.Add(data.cardId, data);
        }

        Debug.Log($"[Registry] Словарь успешно собран. Элементов: {cache.Count}");
    }
}