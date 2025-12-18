using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

// public class AttackedCardScript : MonoBehaviour, IDropHandler
// {
//     GameManagerScript gameManager;
//     CardMovemantScript draggingCard;

//     public void OnDrop(PointerEventData eventData)
//     {
//         // draggingCard = eventData.pointerDrag.GetComponent<CardMovemantScript>();
//         // gameManager = FindAnyObjectByType<GameManagerScript>();

//         // BattleCardController attacker = gameManager.CurrentGame.PlayerFieldListController.CardControllers.FirstOrDefault(c => c.CardModel.id == draggingCard.CardId);
//         // BattleCardController defender = gameManager.CurrentGame.EnemyFieldListController.CardControllers.FirstOrDefault(c => c.BattleCardView.CardRoot == gameObject);
//         // FieldType fieldType = transform.parent.GetComponent<DropPlaceScript>().type;

//         // if (attacker != null && attacker.CanAttack && fieldType == FieldType.ENEMY_FIELD)
//         // {
//         //     attacker.ChangeAttackState(false);
//         //     gameManager.CardsFight(attacker, defender);
//         // }
//     }
// }

public class AttackedCardScript : MonoBehaviour, IDropHandler
{
    private GameManagerScript gameManager;

    public void OnDrop(PointerEventData eventData)
    {
        // gameManager = FindAnyObjectByType<GameManagerScript>();

        // var draggingCard = eventData.pointerDrag.GetComponent<CardMovemantScript>();
        // if (draggingCard == null || gameManager == null) return;

        // BattleCardController attacker = gameManager.CurrentGame.PlayerFieldListController.CardControllers
        //     .FirstOrDefault(c => c.CardModel.id == draggingCard.CardId);

        // BattleCardController defender = gameManager.CurrentGame.EnemyFieldListController.CardControllers
        //     .FirstOrDefault(c => c.BattleCardView.CardRoot == gameObject);

        // FieldType fieldType = transform.parent.GetComponent<DropPlaceScript>().type;

        // if (attacker != null && defender != null && attacker.CanAttack && fieldType == FieldType.ENEMY_FIELD)
        // {
        //     attacker.ChangeAttackState(false);
        //     gameManager.RequestCardsFight(attacker.CardModel.id, defender.CardModel.id);
        // }
    }
}
