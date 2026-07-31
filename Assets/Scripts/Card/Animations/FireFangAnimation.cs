using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;

public class FireFangAnimation : BaseAttackAnimation
{
    public override IEnumerator PlayAnimation(BattleCardController attacker, BattleCardController defender, AttackManager attackManager, Action<Vector3, int> showDamagePopupCallback)
    {
        Transform attackerCardTransform = attacker.BattleCardView.CardRoot.transform;
        Transform defenderCardTransform = defender.BattleCardView.CardRoot.transform;

        attackerCardTransform.GetPositionAndRotation(out Vector3 cardStartPos, out Quaternion cardStartRot);
        Vector3 cardStartScale = attackerCardTransform.localScale;

        float liftHeight = 0.5f;
        int totalDamage = attackManager.CalculateDamage(attacker, attacker.SelectedAbility.AbilityIndex, defender);

        GameObject effectAbilityFilter = defender.BattleCardView.CardRoot.transform.Find("AnimationObjects/EffectAbilityFilter").gameObject;
        MeshRenderer effectAbilityFilterMeshRenderer = effectAbilityFilter.GetComponent<MeshRenderer>();
        Material effectAbilityMaterial = effectAbilityFilterMeshRenderer.materials[2];
        Color32 hotColor = new(179, 73, 73, 141);

        Vector3 combatStancePos = defenderCardTransform.position + (attackerCardTransform.position - defenderCardTransform.position).normalized * 1.0f;
        combatStancePos.y += liftHeight;

        yield return attackerCardTransform.DOMove(combatStancePos, 0.4f).SetEase(Ease.InBack).WaitForCompletion();

        int biteCount = 2;
        for (int i = 0; i < biteCount; i++)
        {
            Vector3 hitPoint = defenderCardTransform.position + Vector3.up * 0.2f;

            Sequence biteSequence = DOTween.Sequence();

            biteSequence.Append(attackerCardTransform.DOLocalRotateQuaternion(Quaternion.Euler(-60f, -90f, 180f), 0.2f).SetEase(Ease.OutCubic));
            biteSequence.Join(attackerCardTransform.DOMove(combatStancePos + Vector3.up * 0.5f - attackerCardTransform.forward * 0.3f, 0.2f));

            if (i > 0)
                biteSequence.Join(effectAbilityMaterial.DOColor(hotColor, 0.2f).SetEase(Ease.OutCubic));

            biteSequence.Append(attackerCardTransform.DOMove(hitPoint, 0.1f).SetEase(Ease.InExpo));
            biteSequence.Join(attackerCardTransform.DOLocalRotateQuaternion(Quaternion.Euler(-120f, -90f, 180f), 0.1f).SetEase(Ease.InExpo));

            yield return biteSequence.WaitForCompletion();

            int partialDamage = (i == biteCount - 1) ? (totalDamage - (totalDamage / 2)) : totalDamage / 2;
            showDamagePopupCallback?.Invoke(hitPoint + Vector3.up * 1f, partialDamage);

            defenderCardTransform.DOShakePosition(0.3f, new Vector3(0.5f, 0.2f, 0), 25);
            defenderCardTransform.DOPunchScale(new Vector3(-0.2f, -0.2f, 0), 0.2f);

            yield return new WaitForSeconds(0.05f);
        }

        if (!AnimationWithOutDamageMode.Mode) attackManager.PerformAttack(attacker, defender, totalDamage);
        else if (GameTypeConfig.CurrentType == GameType.Multiplayer)
        {
            GameManager gameManager = UnityEngine.Object.FindAnyObjectByType<GameManager>();
            if (gameManager != null && gameManager.IsMyTurn)
            {
                NetworkGameController networkGameController = UnityEngine.Object.FindAnyObjectByType<NetworkGameController>();
                if (networkGameController != null)
                {
                    int attackerSlotIndex = (int)attacker.BattleCardView.CardRoot.transform.parent.GetComponent<CardSlot>().indexSlotInField;
                    int defenderSlotIndex = (int)defender.BattleCardView.CardRoot.transform.parent.GetComponent<CardSlot>().indexSlotInField;
                    networkGameController.RequestApplyDamage(attackerSlotIndex, attacker.SelectedAbility.AbilityIndex, defenderSlotIndex);
                }
            }
        }

        Sequence returnSequence = DOTween.Sequence();
        returnSequence.Append(attackerCardTransform.DOMove(cardStartPos, 0.4f).SetEase(Ease.OutCubic));
        returnSequence.Join(attackerCardTransform.DORotateQuaternion(cardStartRot, 0.4f).SetEase(Ease.OutQuad));
        yield return returnSequence.WaitForCompletion();

        yield return new WaitForSeconds(0.5f);

        yield return effectAbilityMaterial.DOFade(0, 0.5f).WaitForCompletion();
    }
}