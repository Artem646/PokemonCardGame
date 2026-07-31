using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;

public class DigAnimation : BaseAttackAnimation
{
    public override IEnumerator PlayAnimation(BattleCardController attacker, BattleCardController defender, AttackManager attackManager, Action<Vector3, int> showDamagePopupCallback)
    {
        Transform attackerCardTransform = attacker.BattleCardView.CardRoot.transform;
        Transform defenderCardTransform = defender.BattleCardView.CardRoot.transform;

        attackerCardTransform.GetLocalPositionAndRotation(out Vector3 cardLocalPos, out Quaternion cardLocalRot);
        Vector3 cardLocalScale = attackerCardTransform.localScale;

        attackerCardTransform.DOLocalMove(cardLocalPos - Vector3.forward * 1.7f, 0.5f).SetEase(Ease.OutQuad);
        attackerCardTransform.DOScale(new Vector3(50f, 50f, 50f), 0.5f).SetEase(Ease.OutQuad);
        yield return attackerCardTransform.DOLocalRotateQuaternion(Quaternion.identity, 0.5f).SetEase(Ease.OutSine).WaitForCompletion();

        Tween drillTween = attackerCardTransform.DOLocalRotate(new Vector3(0, 0, 360f), 0.15f, RotateMode.LocalAxisAdd)
            .SetLoops(-1, LoopType.Incremental).SetEase(Ease.Linear);

        Vector3 underGroundPos = cardLocalPos + Vector3.forward * 3.96f;
        yield return attackerCardTransform.DOLocalMove(underGroundPos, 0.5f).SetEase(Ease.InCubic).WaitForCompletion();

        Vector3 underDefenderPos = defenderCardTransform.position - Vector3.up * 3f;
        attackerCardTransform.DOMove(underDefenderPos, 0.5f).SetEase(Ease.OutQuad);
        yield return attackerCardTransform.DOLocalRotate(new Vector3(0, 180f, 0), 0.5f, RotateMode.LocalAxisAdd).SetEase(Ease.OutSine).WaitForCompletion();

        yield return new WaitForSeconds(0.1f);

        Vector3 hitPoint = defenderCardTransform.position + Vector3.up * 0.8f;
        yield return attackerCardTransform.DOMove(hitPoint, 0.15f).SetEase(Ease.OutExpo).WaitForCompletion();

        int totalDamage = attackManager.CalculateDamage(attacker, attacker.SelectedAbility.AbilityIndex, defender);
        showDamagePopupCallback?.Invoke(defenderCardTransform.position + Vector3.up * 1.5f, totalDamage);

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

        defenderCardTransform.DOShakePosition(0.5f, new Vector3(0.7f, 0.7f, 0), 60);
        yield return attackerCardTransform.DOShakePosition(0.5f, new Vector3(0.2f, 0.2f, 0.2f), 40).WaitForCompletion();

        yield return new WaitForSeconds(0.2f);

        Sequence returnSequence = DOTween.Sequence();
        returnSequence.Append(attackerCardTransform.DOMove(hitPoint + Vector3.up * 1.5f, 0.7f).SetEase(Ease.OutQuad));
        returnSequence.Join(attackerCardTransform.DOLocalRotateQuaternion(cardLocalRot, 0.7f).SetEase(Ease.OutCubic).OnComplete(() =>
        {
            drillTween.Kill();
        }));
        returnSequence.Join(attackerCardTransform.DOScale(cardLocalScale, 0.7f)).SetEase(Ease.OutQuad);
        returnSequence.Append(attackerCardTransform.DOLocalMove(cardLocalPos - Vector3.forward * 1.7f, 0.7f).SetEase(Ease.OutQuad));
        returnSequence.Append(attackerCardTransform.DOLocalMove(cardLocalPos, 0.5f)).SetEase(Ease.OutCubic);
        yield return returnSequence.WaitForCompletion();
    }
}