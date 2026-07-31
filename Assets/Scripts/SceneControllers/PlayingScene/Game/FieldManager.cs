using System.Collections.Generic;
using UnityEngine;

public class FieldManager : MonoBehaviour
{
    [SerializeField] private CardSlot[] slots;

    private CardSlot GetRandomSlot(bool isFreeSlots)
    {
        List<CardSlot> foundSlots = new();

        foreach (CardSlot slot in slots)
        {
            bool expr = isFreeSlots ? slot.IsEmpty : !slot.IsEmpty;
            if (expr) foundSlots.Add(slot);
        }

        if (foundSlots.Count == 0)
            return null;

        return foundSlots[Random.Range(0, foundSlots.Count)];
    }

    public CardSlot GetRandomEmptySlot() => GetRandomSlot(true);
    public CardSlot GetRandomFilledSlot() => GetRandomSlot(false);
    public CardSlot[] FieldSlots => slots;
}
