using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System.Threading.Tasks;

public class SwitchAbilityPanelController : MonoBehaviour
{
    [SerializeField] private CanvasGroup switchAbilityPanelCanvasGroup;
    [SerializeField] private Button attackButton;
    [SerializeField] private AnimationViewingSceneController animationSceneController;
    [SerializeField] private TextMeshProUGUI selectedAbilityName;
    [SerializeField] private List<Button> abilityButtons;

    private List<int> availableAbilityIndices = new();
    private BattleCardController currentCardController;
    private BattleCardController enemyCardController;

    private async void Start()
    {
        CardSlot currentFieldSlot = animationSceneController.PlayerFieldManager.FieldSlots[0];
        CardSlot enemyFieldSlot = animationSceneController.EnemyFieldManager.FieldSlots[0];

        while (currentFieldSlot.CurrentCard == null || enemyFieldSlot.CurrentCard == null)
            await Task.Yield();

        currentCardController = currentFieldSlot.CurrentCard.GetComponent<CardControllerLink>().Controller;
        enemyCardController = enemyFieldSlot.CurrentCard.GetComponent<CardControllerLink>().Controller;

        InitAbilitiesPanel(currentCardController);

        attackButton.onClick.AddListener(OnAttackButtonClicked);

        switchAbilityPanelCanvasGroup.alpha = 1;
        switchAbilityPanelCanvasGroup.interactable = true;
    }

    private void InitAbilitiesPanel(BattleCardController cardController)
    {
        int abitilyButtonIndex = 0;

        for (int i = 0; i < cardController.AbilityControllers.Count; i++)
        {
            AbilityController abilityController = cardController.AbilityControllers[i];
            if (!string.IsNullOrEmpty(abilityController.AbilityModel.name))
            {
                availableAbilityIndices.Add(i);

                Button currentAbilityButton = abilityButtons[abitilyButtonIndex];
                currentAbilityButton.gameObject.SetActive(true);

                TextMeshProUGUI abilityButtonText = currentAbilityButton.GetComponentInChildren<TextMeshProUGUI>();
                abilityButtonText.text = abilityController.AbilityModel.name;

                int capturedIndex = abitilyButtonIndex;

                currentAbilityButton.onClick.AddListener(() => OnAbilityButtonClicked(capturedIndex));

                abitilyButtonIndex++;
            }
        }

        if (availableAbilityIndices.Count > 0)
            OnAbilityButtonClicked(0);
    }

    private void OnAbilityButtonClicked(int abilityButtonIndex)
    {
        int abilityIndex = availableAbilityIndices[abilityButtonIndex];

        currentCardController.SetSelectedAbility(abilityIndex);

        string abilityName = currentCardController.AbilityControllers[abilityIndex].AbilityModel.name;
        selectedAbilityName.text = abilityName;

        for (int i = 0; i < abilityButtons.Count; i++)
        {
            ColorBlock colorBlock = abilityButtons[i].colors;
            colorBlock.normalColor = (i == abilityButtonIndex) ? new Color32(52, 159, 61, 255) : Color.white;
            abilityButtons[i].colors = colorBlock;
        }
    }

    private void OnAttackButtonClicked()
    {
        switchAbilityPanelCanvasGroup.interactable = false;
        CardAttackExecutor attackExecutor = currentCardController.BattleCardView.CardRoot.GetComponent<CardAttackExecutor>();
        StartCoroutine(attackExecutor.ExecuteOnlyAnimation(currentCardController, enemyCardController, () => switchAbilityPanelCanvasGroup.interactable = true));
    }
}