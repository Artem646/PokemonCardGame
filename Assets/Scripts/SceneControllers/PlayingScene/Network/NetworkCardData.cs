using Fusion;
using System;

[Serializable]
public struct NetworkCardData : INetworkStruct
{
    public bool isActive;
    public int cardId;

    public int health;
    // public int AttackModifier;          
    // public int DefenseModifier;         

    public int attackCounter;
    public int lastUsedAbilityIndex;
    public int lastTargetSlotIndex;
    public int originalHandSlot;
    public bool isDead;
    public bool readyForEvolution;
}