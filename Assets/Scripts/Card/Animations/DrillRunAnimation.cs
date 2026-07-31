using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;

public class DrillRunAnimation : BaseAttackAnimation
{
    public override IEnumerator PlayAnimation(BattleCardController attacker, BattleCardController defender, AttackManager attackManager, Action<Vector3, int> showDamagePopupCallback)
    {
        Transform attackerCardTransform = attacker.BattleCardView.CardRoot.transform;
        Transform defenderCardTransform = defender.BattleCardView.CardRoot.transform;

        attackerCardTransform.GetLocalPositionAndRotation(out Vector3 cardStartPos, out Quaternion cardStartRot);
        Vector3 cardStartScale = attackerCardTransform.localScale;

        attackerCardTransform.DOLocalMove(new Vector3(cardStartPos.x, cardStartPos.y + 9.5f, cardStartPos.z - 2.9f), 0.5f).SetEase(Ease.OutQuad);
        attackerCardTransform.DOScale(new Vector3(50f, 100f, 100f), 0.5f).SetEase(Ease.OutQuad);
        yield return attackerCardTransform.DOLocalRotateQuaternion(Quaternion.Euler(0, 45f, 0), 0.5f).SetEase(Ease.OutSine).WaitForCompletion();

        Tween drillTween = attackerCardTransform.DOLocalRotate(new Vector3(0, 0, 360f), 0.15f, RotateMode.LocalAxisAdd)
            .SetLoops(-1, LoopType.Incremental).SetEase(Ease.Linear);

        yield return new WaitForSeconds(0.4f);

        Vector3 hitPoint = defenderCardTransform.position + (attackerCardTransform.position - defenderCardTransform.position).normalized;
        hitPoint.y = 1.8f;
        hitPoint.x -= 1.1f;

        yield return attackerCardTransform.DOMove(hitPoint, 0.15f).SetEase(Ease.InQuad).WaitForCompletion();

        int damage = attackManager.CalculateDamage(attacker, attacker.SelectedAbility.AbilityIndex, defender);
        showDamagePopupCallback?.Invoke(defenderCardTransform.position + Vector3.up * 1.2f, damage);

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

        yield return new WaitForSeconds(0.1f);

        defenderCardTransform.DOShakePosition(0.4f, new Vector3(0.3f, 0.3f, 0), 30);
        yield return attackerCardTransform.DOShakePosition(0.5f, new Vector3(0.2f, 0.2f, 0.2f), 40).WaitForCompletion();

        yield return new WaitForSeconds(0.1f);

        Sequence returnSequence = DOTween.Sequence();
        returnSequence.Append(attackerCardTransform.DOLocalMove(new Vector3(cardStartPos.x, cardStartPos.y + 1f, cardStartPos.z - 2.9f), 0.7f).SetEase(Ease.OutQuad));
        returnSequence.Join(attackerCardTransform.DOScale(cardStartScale, 0.7f)).SetEase(Ease.OutQuad);
        returnSequence.Join(attackerCardTransform.DOLocalRotateQuaternion(cardStartRot, 0.7f).SetEase(Ease.OutCubic).OnComplete(() =>
        {
            drillTween.Kill();
        }));
        returnSequence.Append(attackerCardTransform.DOLocalMove(cardStartPos, 0.7f)).SetEase(Ease.OutCubic);
        yield return returnSequence.WaitForCompletion();
    }
}