using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;

public class SlashAnimation : BaseAttackAnimation
{
    public override IEnumerator PlayAnimation(BattleCardController attacker, BattleCardController defender, AttackManager attackManager, Action<Vector3, int> showDamagePopupCallback)
    {
        Transform attackerCardTransform = attacker.BattleCardView.CardRoot.transform;
        Transform defenderCardTransform = defender.BattleCardView.CardRoot.transform;

        attackerCardTransform.GetPositionAndRotation(out Vector3 cardStartPos, out Quaternion cardStartRot);
        Vector3 cardStartScale = attackerCardTransform.localScale;

        float liftHeight = 0.6f;
        float strikeScaleX = 5.0f;
        int damage = attackManager.CalculateDamage(attacker, attacker.SelectedAbility.AbilityIndex, defender);

        Transform bigScratche = defenderCardTransform.Find("AnimationObjects/Scratches/BigScratche");
        Material bigScratcheMaterial = bigScratche.GetComponent<MeshRenderer>().material;

        Vector3 combatStancePos = defenderCardTransform.position + (attackerCardTransform.position - defenderCardTransform.position).normalized * 1.5f;
        combatStancePos.y += liftHeight;

        yield return attackerCardTransform.DOMove(combatStancePos, 0.4f).SetEase(Ease.InQuad).WaitForCompletion();
        yield return attackerCardTransform.DOLocalRotateQuaternion(Quaternion.Euler(-45f, -90f, 180f), 0.3f).SetEase(Ease.InQuad).WaitForCompletion();

        Vector3 hitOffset = defenderCardTransform.right * -0.4f + Vector3.up * 0.3f;

        float tiltAngle = -50f;

        Sequence slashCycle = DOTween.Sequence();

        Vector3 recoilPos = combatStancePos + (combatStancePos - defenderCardTransform.position).normalized * 0.4f;

        slashCycle.Append(attackerCardTransform.DOMove(recoilPos, 0.1f).SetEase(Ease.OutQuad));
        slashCycle.Append(attackerCardTransform.DOLocalRotateQuaternion(Quaternion.Euler(-90f + tiltAngle, -90f, 180f), 0.1f).SetEase(Ease.OutExpo));

        Vector3 finalHitPoint = defenderCardTransform.position + Vector3.up * liftHeight + hitOffset;

        slashCycle.Append(attackerCardTransform.DOMove(finalHitPoint, 0.07f).SetEase(Ease.InExpo));
        slashCycle.Join(attackerCardTransform.DOScaleX(strikeScaleX, 0.05f));

        slashCycle.Join(bigScratcheMaterial.DOFade(1f, 0.05f));

        yield return slashCycle.WaitForCompletion();

        showDamagePopupCallback?.Invoke(finalHitPoint + Vector3.up * 0.5f, damage);

        defenderCardTransform.DOShakePosition(0.2f, new Vector3(0.3f, 0.3f, 0), 20, 90, false, true);
        defenderCardTransform.DOPunchScale(new Vector3(0.15f, 0.15f, 0.15f), 0.2f);

        if (!AnimationWithOutDamageMode.Mode) attackManager.PerformAttack(attacker, defender, damage);
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

        bigScratcheMaterial.DOFade(0f, 0.5f);
    }
}