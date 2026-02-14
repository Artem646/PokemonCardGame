//     private void ResolveAttack(
//         BattleCardController attacker, BattleCardController defender,
//         float attackerMultiplier, float defenderMultiplier,
//         bool attackerIsPlayerCard, bool defenderIsPlayerCard)
//     {
//         LocalizedString playerLabel = new("NotificationsText", "Player");
//         LocalizedString botLabel = new("NotificationsText", "Bot");

//         string attackerOwner = attackerIsPlayerCard ? playerLabel.GetLocalizedString() : botLabel.GetLocalizedString();
//         string defenderOwner = defenderIsPlayerCard ? playerLabel.GetLocalizedString() : botLabel.GetLocalizedString();

//         if (Mathf.Approximately(attackerMultiplier, defenderMultiplier))
//         {
//             // 0:0 — ничья, обе карты сбрасываются
//             gameManager.MoveCardToReset(attacker, attackerIsPlayerCard);
//             gameManager.MoveCardToReset(defender, defenderIsPlayerCard);

//             Localizer.LocalizeNotification(NotificationKey.ClashWithTieInBotBattle, NotificationType.Info,
//                 attackerOwner, attacker.CardModel.titleKey,
//                 defenderOwner, defender.CardModel.titleKey);
//         }
//         else if (attackerMultiplier > defenderMultiplier)
//         {
//             // 1 — атакующий побеждает, защитник сбрасывается
//             gameManager.MoveCardToReset(defender, defenderIsPlayerCard);

//             Localizer.LocalizeNotification(NotificationKey.СlashWithWinnerInBotBattle, NotificationType.Info,
//                 attackerOwner, attacker.CardModel.titleKey,
//                 defenderOwner, defender.CardModel.titleKey,
//                 attacker.CardModel.mainElement,
//                 defender.CardModel.mainElement);
//         }
//         else
//         {
//             // 0:1 — атакующий проигрывает, сбрасывается он
//             gameManager.MoveCardToReset(attacker, attackerIsPlayerCard);

//             Localizer.LocalizeNotification(NotificationKey.СlashWithWinnerInBotBattle, NotificationType.Info,
//                 defenderOwner, defender.CardModel.titleKey,
//                 attackerOwner, attacker.CardModel.titleKey,
//                 defender.CardModel.mainElement,
//                 attacker.CardModel.mainElement);
//         }
//     }
// }

using System.Linq;
using UnityEngine;
using UnityEngine.Localization;

public class AttackManager : MonoBehaviour
{
    [SerializeField] private GameManagerScript gameManager;
    [SerializeField] private TypeChart typeChart;

    public void PerformNetworkAttack(int attackerId, int defenderId)
    {
        BattleCardController attacker = gameManager.CurrentGame.EnemyFieldListController.CardControllers
            .First(c => c.CardModel.id == attackerId);
        BattleCardController defender = gameManager.CurrentGame.PlayerFieldListController.CardControllers
            .First(c => c.CardModel.id == defenderId);

        PerformAttack(attacker, defender);
    }

    public void PerformAttack(BattleCardController attacker, BattleCardController defender)
    {
        float attackerMultiplier = typeChart.GetMultiplier(attacker.CardModel.mainElement, defender.CardModel.mainElement);
        float defenderMultiplier = typeChart.GetMultiplier(defender.CardModel.mainElement, attacker.CardModel.mainElement);

        bool attackerIsPlayerCard = gameManager.CurrentGame.PlayerFieldListController.CardControllers.Contains(attacker);
        bool defenderIsPlayerCard = gameManager.CurrentGame.PlayerFieldListController.CardControllers.Contains(defender);

        ResolveAttack(attacker, defender, attackerMultiplier, defenderMultiplier, attackerIsPlayerCard, defenderIsPlayerCard);

        attacker.MarkAsAttacked();

        if (attacker.BattleState.Owner == CardOwner.Player)
            attacker.BattleCardView.ApplyBattleStyle(attacker.BattleState);
    }

    private void ResolveAttack(
        BattleCardController attacker, BattleCardController defender,
        float attackerMultiplier, float defenderMultiplier,
        bool attackerIsPlayerCard, bool defenderIsPlayerCard)
    {
        LocalizedString playerLabel = new("NotificationsText", "Player");
        LocalizedString botLabel = new("NotificationsText", "Bot");

        // string attackerOwner = attackerIsPlayerCard ? playerLabel.GetLocalizedString() : botLabel.GetLocalizedString();
        // string defenderOwner = defenderIsPlayerCard ? playerLabel.GetLocalizedString() : botLabel.GetLocalizedString();

        int attackerDamage = GetDamageByMultiplier(attackerMultiplier);
        // int defenderDamage = GetDamageByMultiplier(defenderMultiplier);

        // attacker.BattleState.ApplyDamage(defenderDamage);
        defender.BattleState.ApplyDamage(attackerDamage);

        // bool attackerDead = attacker.BattleState.CurrentHP <= 0;
        bool defenderDead = defender.BattleState.CurrentHP <= 0;

        // if (attackerDead)
        //     gameManager.MoveCardToReset(attacker, attackerIsPlayerCard);

        // if (defenderDead)
        //     gameManager.MoveCardToReset(defender, defenderIsPlayerCard);

        // NotificationManager.ShowNotification($"{defenderOwner}: {attackerDamage} урона, {attackerOwner}: {defenderDamage} урона", NotificationType.Info);

        // NotificationManager.ShowNotification($"Нанесено {attackerDamage} урона по карте {defender.CardModel.imageName}. Кем: {attackerOwner}. По кому: {defenderOwner}", NotificationType.Info);

        // Localizer.LocalizeNotification(NotificationKey.ClashWithDamageInBotBattle, NotificationType.Info,
        //     attackerOwner, attacker.CardModel.titleKey, attackerDamage,
        //     defenderOwner, defender.CardModel.titleKey, defenderDamage);
    }

    private int GetDamageByMultiplier(float multiplier)
    {
        // return multiplier switch
        // {
        //     2f => 20,
        //     1f => 10,
        //     0.5f => 5,
        //     _ => 0
        // };

        return multiplier switch
        {
            2f => 50,
            1f => 50,
            0.5f => 50,
            _ => 0
        };
    }
}
