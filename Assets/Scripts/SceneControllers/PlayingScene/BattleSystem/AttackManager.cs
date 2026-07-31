// using System.Linq;
// using UnityEngine;
// using UnityEngine.Localization;

// public class AttackManager3D : MonoBehaviour
// {
//     [SerializeField] private GameManagerScript3D gameManager;
//     [SerializeField] private TypeChart typeChart;

//     // --- ДЛЯ МУЛЬТИПЛЕЕРА (Вызывается сервером) ---
//     public int CalculateDamage(int attackerId, int defenderId)
//     {
//         // Находим модели карт по ID через репозиторий
//         CardModel attackerModel = CardRepository.Instance.GetCardById(attackerId);
//         CardModel defenderModel = CardRepository.Instance.GetCardById(defenderId);

//         if (attackerModel == null || defenderModel == null) return 0;

//         // Считаем множитель
//         float multiplier = typeChart.GetMultiplier(attackerModel.mainElement, defenderModel.mainElement);

//         // Возвращаем чистый урон на основе множителя
//         return GetDamageByMultiplier(multiplier);
//     }

//     // --- ДЛЯ БОТА (Локальная логика) ---
//     public void PerformAttack(BattleCard3DController attacker, BattleCard3DController defender)
//     {
//         float attackerMultiplier = typeChart.GetMultiplier(attacker.CardModel.mainElement, defender.CardModel.mainElement);
//         int attackerDamage = GetDamageByMultiplier(attackerMultiplier);

//         defender.BattleState.ApplyDamage(attackerDamage);

//         // float attackerMultiplier = typeChart.GetMultiplier(attacker.CardModel.mainElement, defender.CardModel.mainElement);
//         // float defenderMultiplier = typeChart.GetMultiplier(defender.CardModel.mainElement, attacker.CardModel.mainElement);

//         // bool attackerIsPlayerCard = gameManager.CurrentGame.PlayerFieldControllers.Contains(attacker);
//         // bool defenderIsPlayerCard = gameManager.CurrentGame.PlayerFieldControllers.Contains(defender);

//         // ResolveAttack(attacker, defender, attackerMultiplier, defenderMultiplier, attackerIsPlayerCard, defenderIsPlayerCard);

//         attacker.MarkAsAttacked();

//         if (attacker.BattleState.Owner == CardOwner.Player)
//             attacker.BattleCardView.ApplyBattleStyle(attacker.BattleState);
//     }

//     // private void ResolveAttack(
//     //     BattleCard3DController attacker, BattleCard3DController defender,
//     //     float attackerMultiplier, float defenderMultiplier,
//     //     bool attackerIsPlayerCard, bool defenderIsPlayerCard)
//     // {
//     //     LocalizedString playerLabel = new("NotificationsText", "Player");
//     //     LocalizedString botLabel = new("NotificationsText", "Bot");

//     //     // string attackerOwner = attackerIsPlayerCard ? playerLabel.GetLocalizedString() : botLabel.GetLocalizedString();
//     //     // string defenderOwner = defenderIsPlayerCard ? playerLabel.GetLocalizedString() : botLabel.GetLocalizedString();

//     //     int attackerDamage = GetDamageByMultiplier(attackerMultiplier);
//     //     // int defenderDamage = GetDamageByMultiplier(defenderMultiplier);

//     //     // attacker.BattleState.ApplyDamage(defenderDamage);
//     //     defender.BattleState.ApplyDamage(attackerDamage);

//     //     // bool attackerDead = attacker.BattleState.CurrentHP <= 0;
//     //     bool defenderDead = defender.BattleState.CurrentHP <= 0;

//     //     // if (attackerDead)
//     //     //     gameManager.MoveCardToReset(attacker, attackerIsPlayerCard);

//     //     // if (defenderDead)
//     //     //     gameManager.MoveCardToReset(defender, defenderIsPlayerCard);

//     //     // NotificationManager.ShowNotification($"{defenderOwner}: {attackerDamage} урона, {attackerOwner}: {defenderDamage} урона", NotificationType.Info);

//     //     // NotificationManager.ShowNotification($"Нанесено {attackerDamage} урона по карте {defender.CardModel.imageName}. Кем: {attackerOwner}. По кому: {defenderOwner}", NotificationType.Info);

//     //     // Localizer.LocalizeNotification(NotificationKey.ClashWithDamageInBotBattle, NotificationType.Info,
//     //     //     attackerOwner, attacker.CardModel.titleKey, attackerDamage,
//     //     //     defenderOwner, defender.CardModel.titleKey, defenderDamage);
//     // }

//     private int GetDamageByMultiplier(float multiplier)
//     {
//         // return multiplier switch
//         // {
//         //     2f => 20,
//         //     1f => 10,
//         //     0.5f => 5,
//         //     _ => 0
//         // };

//         return multiplier switch
//         {
//             2f => 50,
//             1f => 50,
//             0.5f => 50,
//             _ => 0
//         };
//     }
// }






// using TMPro;
// using UnityEngine;

// public class AttackManager3D : MonoBehaviour
// {
//     [SerializeField] private GameManagerScript3D gameManager;
//     [SerializeField] private TypeChart typeChart;

//     public int CalculateDamage(int attackerId, int defenderId)
//     {
//         CardModel attackerModel = CardRepository.Instance.GetCardModelById(attackerId);
//         CardModel defenderModel = CardRepository.Instance.GetCardModelById(defenderId);
//         float multiplier = typeChart.GetMultiplier(attackerModel.mainElement, defenderModel.mainElement);
//         // return GetDamageByMultiplier(multiplier);
//     }

//     public void PerformAttack(BattleCard3DController attacker, BattleCard3DController defender)
//     {
//         string abilityDamageTextComponentName = "AbilityDamage";
//         switch (attacker.SelectedAbility.AbilityIndex)
//         {
//             case 0: abilityDamageTextComponentName = "FirstAbilityDamage"; break;
//             case 1: abilityDamageTextComponentName = "SecondAbilityDamage"; break;
//             case 3: abilityDamageTextComponentName = "FourthAbilityDamage"; break;
//         }

//         string attackerAbilityDamageText = attacker.BattleCardView.CardRoot.transform.Find($"{abilityDamageTextComponentName}").GetComponent<TextMeshPro>().text;
//         int attackerAbilityDamage = int.Parse(attackerAbilityDamageText);

//         string defenderDefenseValueText = defender.BattleCardView.CardRoot.transform.Find("DefenseValue").GetComponent<TextMeshPro>().text;
//         int defenderDefenseValue = int.Parse(defenderDefenseValueText);

//         float attackerMultiplier = typeChart.GetMultiplier(attacker.SelectedAbility.AbilityModel.element, defender.CardModel.mainElement);
//         if (defender.CardModel.secondaryElement != null)
//             attackerMultiplier *= typeChart.GetMultiplier(attacker.SelectedAbility.AbilityModel.element, defender.CardModel.secondaryElement);

//         int finallyCalculatedDamage = (int)((attackerAbilityDamage - defenderDefenseValue) * attackerMultiplier);

//         // int attackerDamage = GetDamageByMultiplier(attackerMultiplier);
//         // defender.BattleState.ApplyDamage(attackerDamage);

//         attacker.MarkAsAttacked();

//         if (attacker.BattleState.Owner == CardOwner.Player)
//             attacker.BattleCardView.ApplyBattleStyle(attacker.BattleState);
//     }

//     // private int GetDamageByMultiplier(float multiplier)
//     // {
//     //     return multiplier switch
//     //     {
//     //         2f => 50,
//     //         1f => 50,
//     //         0.5f => 50,
//     //         _ => 0
//     //     };
//     // }
// }

using UnityEngine;

public class AttackManager : MonoBehaviour
{
    [SerializeField] private TypeChart typeChart;

    public int CalculateDamageInNetworkGame(NetworkCardData attackerData, int abilityIndex, NetworkCardData defenderData)
    {
        CardModel attackerModel = CardRepository.Instance.GetGameCardModelById(attackerData.cardId);
        CardModel defenderModel = CardRepository.Instance.GetGameCardModelById(defenderData.cardId);

        Ability attakerAbility = GetAbilityByIndex(attackerModel, abilityIndex);

        // int currentAttackValue = attackerModel.stats.attack + attackerData.AttackModifier;

        int currentAttackValue = attackerModel.stats.attack;
        currentAttackValue = Mathf.Max(0, currentAttackValue);

        int abilityDamage = 0;
        if (attakerAbility.type == AbilityType.Physical)
            abilityDamage = Mathf.FloorToInt(currentAttackValue * attakerAbility.power.Value / 50f);
        else if (attakerAbility.type == AbilityType.Special)
            abilityDamage = Mathf.FloorToInt(attackerModel.stats.specialAttack * attakerAbility.power.Value / 50f);

        // int currentDefenseValue = defenderModel.stats.defense + defenderData.DefenseModifier;

        int currentDefenseValue = defenderModel.stats.defense;
        currentDefenseValue = Mathf.Max(0, currentDefenseValue);

        float attackerMultiplier = typeChart.GetMultiplier(attakerAbility.element.Value, defenderModel.mainElement);
        if (defenderModel.secondaryElement.HasValue)
            attackerMultiplier *= typeChart.GetMultiplier(attakerAbility.element.Value, defenderModel.secondaryElement.Value);

        int finallyCalculatedDamage = Mathf.FloorToInt((abilityDamage - currentDefenseValue) * attackerMultiplier);

        return Mathf.Max(0, finallyCalculatedDamage);
    }

    public int CalculateDamage(BattleCardController attacker, int abilityIndex, BattleCardController defender)
    {
        Ability attakerAbility = GetAbilityByIndex(attacker.CardModel, abilityIndex);

        // int currentAttackValue = attacker.CardModel.stats.attack + attacker.BattleState.AttackModifier;

        int currentAttackValue = attacker.CardModel.stats.attack;
        currentAttackValue = Mathf.Max(0, currentAttackValue);

        int abilityDamage = 0;
        if (attakerAbility.type == AbilityType.Physical)
            abilityDamage = Mathf.FloorToInt(currentAttackValue * attakerAbility.power.Value / 50f);
        else if (attakerAbility.type == AbilityType.Special)
            abilityDamage = Mathf.FloorToInt(attacker.CardModel.stats.specialAttack * attakerAbility.power.Value / 50f);

        // int currentDefenseValue = defender.CardModel.stats.defense + defender.BattleState.DefenseModifier;

        int currentDefenseValue = defender.CardModel.stats.defense;
        currentDefenseValue = Mathf.Max(0, currentDefenseValue);

        float attackerMultiplier = typeChart.GetMultiplier(attakerAbility.element.Value, defender.CardModel.mainElement);
        if (defender.CardModel.secondaryElement.HasValue)
            attackerMultiplier *= typeChart.GetMultiplier(attakerAbility.element.Value, defender.CardModel.secondaryElement.Value);

        int finallyCalculatedDamage = Mathf.FloorToInt((abilityDamage - currentDefenseValue) * attackerMultiplier);

        return Mathf.Max(0, finallyCalculatedDamage);
    }

    public void PerformAttack(BattleCardController attacker, BattleCardController defender, int damage)
    {
        defender.BattleState.ApplyDamage(damage);
        attacker.MarkAsAttacked();
        if (attacker.BattleState.Owner == CardOwner.Player)
            attacker.BattleCardView.ApplyBattleStyle(attacker.BattleState);
    }

    private Ability GetAbilityByIndex(CardModel cardModel, int index)
    {
        return index switch
        {
            0 => cardModel.abilities.firstAbility,
            1 => cardModel.abilities.secondAbility,
            2 => cardModel.abilities.thirdAbility,
            3 => cardModel.abilities.fourthAbility,
            _ => new Ability()
        };
    }
}
