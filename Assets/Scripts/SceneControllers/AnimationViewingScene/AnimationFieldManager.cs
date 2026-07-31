using UnityEngine;

public class AnimationFieldManager : MonoBehaviour
{
    [SerializeField] private CardSlot[] slots;

    public bool TryAddCardInStart(BattleCardController cardController)
    {
        foreach (CardSlot slot in slots)
        {
            if (slot.IsEmpty)
            {
                slot.AddCardInStartGame(cardController);
                return true;
            }
        }

        return false;
    }

    public CardSlot[] FieldSlots => slots;
}