using UnityEngine;
using UnityEngine.EventSystems;

public class AbilityClickScript : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private int abilityIndex;
    [SerializeField] private MeshRenderer abilityPlateRenderer;

    private AbilityController abilityController;
    private BattleCardController battleCardController;

    bool isAbilityClickable;

    private Color32 defaultColor = new(255, 243, 183, 255);
    private Color32 selectedForConfirmationColor = new(221, 229, 72, 255);
    private Color32 finallySelectedColor = new(77, 217, 87, 255);
    private Color32 usedInThisTurnColor = new(165, 165, 165, 255);
    private Color32 alreadyUsedColor = new(232, 84, 68, 255);

    public void Bind(BattleCardController cardController)
    {
        Unbind();
        battleCardController = cardController;
        abilityController = battleCardController.AbilityControllers[abilityIndex];
        abilityController.OnAbilityStateChanged += HandleSelectionPhaseChanged;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (transform.parent.parent.parent.parent.parent.parent.TryGetComponent<CardSlot>(out var slot))
        {
            isAbilityClickable = FindAnyObjectByType<GameManager>().IsMyTurn
                && slot.type == FieldSlotType.SelfFieldSlot
                && FindAnyObjectByType<CameraViewManager>().CurrentZoomedSlot == slot
                && battleCardController.CanAttack && (GameTypeConfig.CurrentType == GameType.Multiplayer ?
                    FindAnyObjectByType<NetworkGameTurnManager>().CurrentPhase == TurnPhase.Attack :
                    FindAnyObjectByType<BotTurnManager>().CurrentPhase == TurnPhase.Attack);

            if (isAbilityClickable)
                battleCardController.SelectAbilityByIndex(abilityIndex);
        }
    }

    private void HandleSelectionPhaseChanged(AbilityState abilityState) => UpdatePlateColor();

    public void UpdatePlateColor()
    {
        bool isCardInZoom = false;

        if (transform.parent.parent.TryGetComponent<CardSlot>(out var slot))
            isCardInZoom = FindAnyObjectByType<CameraViewManager>().CurrentZoomedSlot == slot;

        if (!isCardInZoom)
        {
            if (abilityController.AbilityState == AbilityState.FinallySelected)
                abilityPlateRenderer.material.color = finallySelectedColor;
            else if (abilityController.AbilityState == AbilityState.UsedInThisTurn)
                abilityPlateRenderer.material.color = usedInThisTurnColor;
            else
                abilityPlateRenderer.material.color = defaultColor;

            return;
        }

        switch (abilityController.AbilityState)
        {
            case AbilityState.NotSelected:
                abilityPlateRenderer.material.color = defaultColor;
                break;
            case AbilityState.SelectedForConfirmation:
                abilityPlateRenderer.material.color = selectedForConfirmationColor;
                break;
            case AbilityState.FinallySelected:
                abilityPlateRenderer.material.color = finallySelectedColor;
                break;
            case AbilityState.UsedInThisTurn:
                abilityPlateRenderer.material.color = usedInThisTurnColor;
                break;
            case AbilityState.AlreadyUsedOnce:
                abilityPlateRenderer.material.color = alreadyUsedColor;
                break;
        }
    }

    public void Unbind()
    {
        if (abilityController != null)
            abilityController.OnAbilityStateChanged -= HandleSelectionPhaseChanged;
    }

    private void OnDestroy() => Unbind();
}