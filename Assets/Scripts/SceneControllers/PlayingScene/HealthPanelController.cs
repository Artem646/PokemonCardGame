using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public enum FieldOwner
{
    Player,
    Enemy
}

public class HealthPanelController : MonoBehaviour
{
    [SerializeField] private List<Image> imageSlots;
    [SerializeField] private List<TextMeshProUGUI> healthSlots;
    [SerializeField] private Sprite noCardOnSlotSprite;
    [SerializeField] private FieldOwner fieldOwner;
    [SerializeField] private GameManagerScript gameManager;

    private List<BattleCardController> fieldCards = new();

    public void UpdateSlots()
    {
        if (gameManager.CurrentGame.PlayerFieldListController.CardControllers == null ||
            gameManager.CurrentGame.EnemyFieldListController.CardControllers == null)
            return;

        if (fieldOwner == FieldOwner.Player)
            fieldCards = gameManager.CurrentGame.PlayerFieldListController.CardControllers;
        else if (fieldOwner == FieldOwner.Enemy)
            fieldCards = gameManager.CurrentGame.EnemyFieldListController.CardControllers;

        for (int i = 0; i < imageSlots.Count; i++)
        {
            if (i < fieldCards.Count)
            {
                imageSlots[i].sprite = Resources.Load<Sprite>($"Sprites/PokemonImages/{fieldCards[i].CardModel.imageName}");
                healthSlots[i].text = fieldCards[i].BattleState.CurrentHP.ToString();
            }
            else
            {
                imageSlots[i].sprite = noCardOnSlotSprite;
                healthSlots[i].text = "0";
            }
        }
    }
}
