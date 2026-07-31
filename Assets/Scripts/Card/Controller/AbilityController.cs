using System;

public enum AbilityState
{
    NotSelected,
    SelectedForConfirmation,
    FinallySelected,
    UsedInThisTurn,
    AlreadyUsedOnce
}

public class AbilityController
{
    public Ability AbilityModel { get; private set; }
    public int AbilityIndex { get; private set; }

    public AbilityState AbilityState { get; private set; }
    public bool IsOneTimeUse { get; private set; }

    public event Action<AbilityState> OnAbilityStateChanged;

    public AbilityController(Ability model, int index)
    {
        AbilityModel = model;
        AbilityIndex = index;
        AbilityState = AbilityState.NotSelected;
        IsOneTimeUse = model.type == AbilityType.Status;
    }

    public void Select(AbilityState abilityState)
    {
        AbilityState = abilityState;
        OnAbilityStateChanged?.Invoke(AbilityState);
    }

    public void Deselect()
    {
        if (AbilityState == AbilityState.AlreadyUsedOnce) return;
        AbilityState = AbilityState.NotSelected;
        OnAbilityStateChanged?.Invoke(AbilityState);
    }

    public void MarkAsUsedInThisTurn()
    {
        AbilityState = AbilityState.UsedInThisTurn;
        OnAbilityStateChanged?.Invoke(AbilityState);
    }

    public void ResetForNewTurn()
    {
        if (IsOneTimeUse && AbilityState == AbilityState.UsedInThisTurn)
            AbilityState = AbilityState.AlreadyUsedOnce;
        else if (AbilityState == AbilityState.UsedInThisTurn)
            AbilityState = AbilityState.NotSelected;

        OnAbilityStateChanged?.Invoke(AbilityState);
    }

    public bool IsAttackType() =>
        (AbilityModel.type == AbilityType.Physical || AbilityModel.type == AbilityType.Special)
            && AbilityState == AbilityState.FinallySelected;
}