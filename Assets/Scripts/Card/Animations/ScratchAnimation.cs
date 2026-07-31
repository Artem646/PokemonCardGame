using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ScratchAnimation : BaseAttackAnimation
{
    public override IEnumerator PlayAnimation(BattleCardController attacker, BattleCardController defender, AttackManager attackManager, Action<Vector3, int> showDamagePopupCallback)
    {
        Transform attackerCardTransform = attacker.BattleCardView.CardRoot.transform;
        Transform defenderCardTransform = defender.BattleCardView.CardRoot.transform;

        attackerCardTransform.GetPositionAndRotation(out Vector3 cardStartPos, out Quaternion cardStartRot);
        Vector3 cardStartScale = attackerCardTransform.localScale;

        float liftHeight = 0.6f;
        float strikeScaleX = 5.0f;
        int totalDamage = attackManager.CalculateDamage(attacker, attacker.SelectedAbility.AbilityIndex, defender);

        Transform scratchesContainer = defenderCardTransform.Find("AnimationObjects/Scratches");
        List<Material> scratchMaterials = new();

        foreach (Transform child in scratchesContainer)
            scratchMaterials.Add(child.GetComponent<MeshRenderer>().material);

        Vector3 combatStancePos = defenderCardTransform.position + (attackerCardTransform.position - defenderCardTransform.position).normalized * 1.5f;
        combatStancePos.y += liftHeight;

        yield return attackerCardTransform.DOMove(combatStancePos, 0.4f).SetEase(Ease.InQuad).WaitForCompletion();

        int slashCount = 3;
        for (int i = 0; i < slashCount; i++)
        {
            Vector3 hitOffset = (i == 0) ? (defenderCardTransform.right * -0.5f + Vector3.up * 0.3f) :
                                (i == 1) ? (defenderCardTransform.right * 0.5f + Vector3.up * -0.3f) :
                                Vector3.zero;

            float tiltAngle = (i == 0) ? -25f : (i == 1) ? 25f : 0f;

            Sequence slashCycle = DOTween.Sequence();

            Vector3 recoilPos = combatStancePos + (combatStancePos - defenderCardTransform.position).normalized * 0.4f;

            slashCycle.Append(attackerCardTransform.DOMove(recoilPos, 0.1f).SetEase(Ease.OutQuad));
            slashCycle.Append(attackerCardTransform.DOLocalRotateQuaternion(Quaternion.Euler(-90f + tiltAngle, -90f, 180f), 0.1f).SetEase(Ease.OutExpo));

            Vector3 finalHitPoint = defenderCardTransform.position + Vector3.up * liftHeight + hitOffset;

            slashCycle.Append(attackerCardTransform.DOMove(finalHitPoint, 0.07f).SetEase(Ease.InExpo));
            slashCycle.Join(attackerCardTransform.DOScaleX(strikeScaleX, 0.05f));

            if (scratchMaterials.Count > i)
                slashCycle.Join(scratchMaterials[i].DOFade(1f, 0.05f));

            yield return slashCycle.WaitForCompletion();

            int partialDamage = (i == slashCount - 1) ? (totalDamage - (totalDamage / 3 * 2)) : totalDamage / 3;
            showDamagePopupCallback?.Invoke(finalHitPoint + Vector3.up * 0.5f, partialDamage);

            defenderCardTransform.DOShakePosition(0.2f, new Vector3(0.3f, 0.3f, 0), 20, 90, false, true);
            defenderCardTransform.DOPunchScale(new Vector3(0.15f, 0.15f, 0.15f), 0.2f);

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
        returnSequence.Join(attackerCardTransform.DORotateQuaternion(cardStartRot, 0.3f).SetEase(Ease.OutQuad));
        returnSequence.Join(attackerCardTransform.DOScale(cardStartScale, 0.3f));
        yield return returnSequence.WaitForCompletion();

        yield return new WaitForSeconds(0.8f);

        foreach (Material material in scratchMaterials)
            material.DOFade(0f, 0.5f);
    }
}