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
    public int CurrentDamage { get; set; }

    // public int AttackModifier { get; private set; } = 0;
    // public int DefenseModifier { get; private set; } = 0;

    public event Action<float> OnHPChanged;

    public void Init(CardOwner owner)
    {
        Owner = owner;
        IsFresh = true;
        HasAttacked = false;
        // AttackModifier = 0;
        // DefenseModifier = 0;
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
        CurrentDamage = damage;
        if (CurrentHP < 0) CurrentHP = 0;
        OnHPChanged?.Invoke(CurrentHP);
    }

    public void SetHP(int hp)
    {
        CurrentHP = hp;
        OnHPChanged?.Invoke(CurrentHP);
    }

    // public void AddAttackModifier(int amount)
    // {
    //     AttackModifier += amount;
    // }

    // public void AddDefenseModifier(int amount)
    // {
    //     DefenseModifier += amount;
    // }

    // public void ClearModifiers()
    // {
    //     AttackModifier = 0;
    //     DefenseModifier = 0;
    // }
}
