using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;

public class DefaultMeleeAnimation : BaseAttackAnimation
{
    public override IEnumerator PlayAnimation(BattleCardController attacker, BattleCardController defender, AttackManager attackManager, Action<Vector3, int> showDamagePopupCallback)
    {
        Transform attackerCardTransform = attacker.BattleCardView.CardRoot.transform;
        Transform defenderCardTransform = defender.BattleCardView.CardRoot.transform;

        attackerCardTransform.GetLocalPositionAndRotation(out Vector3 cardStartPos, out Quaternion cardStartRot);
        Vector3 cardStartScale = attackerCardTransform.localScale;

        float dragHeight = 0.5f;
        float attackLift = 0.4f;
        float scaleXStrike = 5.5f;

        yield return attackerCardTransform.DOMove(attackerCardTransform.position + Vector3.up * dragHeight, 0.2f).SetEase(Ease.OutQuad).WaitForCompletion();

        Vector3 preStrikePos = defenderCardTransform.position + (attackerCardTransform.position - defenderCardTransform.position).normalized * 1.5f;
        preStrikePos.y = defenderCardTransform.position.y + dragHeight;

        yield return attackerCardTransform.DOMove(preStrikePos, 0.4f).SetEase(Ease.OutCubic).WaitForCompletion();

        Sequence strikeSequence = DOTween.Sequence();

        strikeSequence.Append(attackerCardTransform.DOMove(preStrikePos + (preStrikePos - defenderCardTransform.position).normalized * 0.5f + Vector3.up * 0.3f, 0.2f).SetEase(Ease.OutQuad));
        strikeSequence.Join(attackerCardTransform.DOLocalRotateQuaternion(Quaternion.Euler(-100f, -90f, 180f), 0.2f));

        strikeSequence.Append(attackerCardTransform.DOMove(defenderCardTransform.position + Vector3.up * attackLift, 0.1f).SetEase(Ease.InExpo));
        strikeSequence.Join(attackerCardTransform.DOScaleX(scaleXStrike, 0.05f));

        yield return strikeSequence.WaitForCompletion();

        int damage = attackManager.CalculateDamage(attacker, attacker.SelectedAbility.AbilityIndex, defender);
        showDamagePopupCallback?.Invoke(defenderCardTransform.position, damage);

        Sequence reactionOnStrikeSequence = DOTween.Sequence();

        reactionOnStrikeSequence.Append(defenderCardTransform.DOShakePosition(0.3f, new Vector3(0.4f, 0.4f, 0), 20, 90, false, true));
        reactionOnStrikeSequence.Join(defenderCardTransform.DOPunchScale(new Vector3(0.2f, 0.2f, 0.2f), 0.3f));

        reactionOnStrikeSequence.Append(attackerCardTransform.DOLocalRotateQuaternion(cardStartRot, 0.2f));
        reactionOnStrikeSequence.Join(attackerCardTransform.DOScaleX(cardStartScale.x, 0.2f));

        yield return reactionOnStrikeSequence.WaitForCompletion();

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

        if (attackerCardTransform.parent.TryGetComponent<CardSlot>(out var slot))
            yield return PlaceToSlot(slot, attackerCardTransform);
    }

    public IEnumerator PlaceToSlot(CardSlot slot, Transform cardTransform)
    {
        yield return cardTransform.DOMove(slot.transform.position, 0.3f).SetEase(Ease.OutQuad).WaitForCompletion();
        if (cardTransform.TryGetComponent<CardControllerLink>(out var link))
            yield return slot.PlaceCard(link.Controller);
    }
}