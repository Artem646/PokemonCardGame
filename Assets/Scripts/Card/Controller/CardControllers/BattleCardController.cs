using System.Collections.Generic;
using UnityEngine;

public class BattleCardController : BaseCardController
{
    public IBattleCardView BattleCardView { get; private set; }
    public CardBattleState BattleState { get; private set; }

    public List<AbilityController> AbilityControllers { get; private set; }
    public AbilityController SelectedAbility { get; private set; }

    public bool CanAttack => BattleState.CanAttack();

    public BattleCardController(CardModel model, IBattleCardView view)
        : base(model, view)
    {
        BattleCardView = view;
        BattleState = view.CardRoot.GetComponent<CardBattleState>();

        InitAbilities();
        BindAbilities();
    }

    private void InitAbilities()
    {
        AbilityControllers = new List<AbilityController>
        {
            new(CardModel.abilities.firstAbility, 0),
            new(CardModel.abilities.secondAbility, 1),
            new(CardModel.abilities.thirdAbility, 2),
            new(CardModel.abilities.fourthAbility, 3)
        };
    }

    public void BindAbilities()
    {
        AbilityClickScript[] abilities = BattleCardView.CardRoot.GetComponentsInChildren<AbilityClickScript>();
        foreach (AbilityClickScript ability in abilities)
            ability.Bind(this);
    }

    public void SetSelectedAbility(int index)
    {
        AbilityController ability = AbilityControllers[index];
        SelectedAbility = ability;
    }

    public void SelectAbilityByIndex(int index)
    {
        AbilityController ability = AbilityControllers[index];

        if (ability.AbilityState == AbilityState.AlreadyUsedOnce)
        {
            NotificationManager.ShowNotification("Это одноразовая способность и она уже была использована.", NotificationType.Info);
            return;
        }

        if (ability.AbilityModel.type == AbilityType.Physical || ability.AbilityModel.type == AbilityType.Special)
        {
            if (SelectedAbility == null)
            {
                SetSelectedAbility(index);
                SelectedAbility.Select(AbilityState.FinallySelected);
            }
        }
        else if (ability.AbilityModel.type == AbilityType.Status)
        {
            if (SelectedAbility == null)
            {
                SetSelectedAbility(index);
                SelectedAbility.Select(AbilityState.SelectedForConfirmation);
                return;
            }

            if (SelectedAbility == ability && SelectedAbility.AbilityState == AbilityState.SelectedForConfirmation)
                SelectedAbility.Select(AbilityState.FinallySelected);
        }
    }

    public void DeselectCurrentAbility()
    {
        SelectedAbility?.Deselect();
        SelectedAbility = null;
    }

    public void ResetAbilityWithoutConfirmation()
    {
        if (SelectedAbility != null && SelectedAbility.AbilityState == AbilityState.SelectedForConfirmation)
            DeselectCurrentAbility();
    }

    public void UpdateActivitiesPlatesColor()
    {
        AbilityClickScript[] abilities = BattleCardView.CardRoot.GetComponentsInChildren<AbilityClickScript>();
        foreach (AbilityClickScript ability in abilities)
            ability.UpdatePlateColor();
    }

    public void MarkAsPlayedInThisTurn(CardOwner owner) => BattleState.Init(owner);

    public void ResetTurnFlags()
    {
        BattleState.MarkAsReady();
        DeselectCurrentAbility();
        foreach (AbilityController ability in AbilityControllers)
            ability.ResetForNewTurn();
    }

    public void MarkAsAttacked()
    {
        BattleState.MarkAsAttacked();
        SelectedAbility.MarkAsUsedInThisTurn();
        SelectedAbility = null;
    }

    public override void AddToContainer(object container) { }

    public override void RemoveFromContainer()
    {
        Object.Destroy(BattleCardView.CardRoot);
    }

    public void OnAnimationCardClicked()
    {
        BattleCardController cloneController = CardControllerFactory.Create<BattleCardController>(CardModel);
        IBattleCardView cloneView = cloneController?.BattleCardView;
        if (cloneView != null)
        {
            cloneView.SetHPOnClone(BattleState.CurrentHP);

            cloneView.CardRoot.GetComponent<CardMovemantScript>().enabled = false;
            cloneView.CardRoot.GetComponent<CardAttackExecutor>().enabled = false;
            cloneView.CardRoot.GetComponent<CardClickScript>().enabled = false;
            cloneView.CardRoot.GetComponent<CardBattleState>().enabled = false;

            Transform cloneCardTransform = cloneView.CardRoot.transform;
            Transform cardTransform = BattleCardView.CardRoot.transform;

            cloneCardTransform.SetPositionAndRotation(cardTransform.position, cardTransform.rotation);
            cloneCardTransform.localScale = cardTransform.localScale;

            Overlay3DManager.Instance.ShowCard(cloneView.CardRoot);
        }
    }
}
