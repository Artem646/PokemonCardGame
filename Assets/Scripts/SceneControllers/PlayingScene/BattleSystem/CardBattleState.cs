using System;
using UnityEngine;

public enum CardOwner
{
    Player,
    Enemy
}

public class CardBattleState : MonoBehaviour
{
    public CardOwner Owner { get; private set; }
    public bool IsFresh { get; private set; }
    public bool HasAttacked { get; private set; }

    public int CurrentHP { get; set; }

    public event Action<float> OnXPChanged;

    public void Init(CardOwner owner)
    {
        Owner = owner;
        IsFresh = true;
        HasAttacked = false;
    }

    public void MarkAsReady()
    {
        IsFresh = false;
        HasAttacked = false;
    }

    public void MarkAsAttacked() => HasAttacked = true;

    public bool CanAttack() => !IsFresh && !HasAttacked;

    public void ApplyDamage(int damage)
    {
        CurrentHP -= damage;
        if (CurrentHP < 0) CurrentHP = 0;
        OnXPChanged?.Invoke(CurrentHP);
    }
}
