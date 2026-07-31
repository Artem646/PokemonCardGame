using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;

public class TakeDownAnimation : BaseAttackAnimation
{
    public override IEnumerator PlayAnimation(BattleCardController attacker, BattleCardController defender, AttackManager attackManager, Action<Vector3, int> showDamagePopupCallback)
    {
        Transform attackerCardTransform = attacker.BattleCardView.CardRoot.transform;
        Transform defenderCardTransform = defender.BattleCardView.CardRoot.transform;

        CardSlot attackerCardSlot = attackerCardTransform.parent.GetComponent<CardSlot>();
        CardSlot defenderCardSlot = defenderCardTransform.parent.GetComponent<CardSlot>();

        attackerCardTransform.GetPositionAndRotation(out Vector3 attackerCardStartPos, out Quaternion attackerCardStartRot);
        Vector3 attackerCardStartScale = attackerCardTransform.localScale;
        Vector3 defenderCardStartPos = defenderCardTransform.position;

        Transform attackerCardSlotTransform = attackerCardTransform.parent;
        attackerCardTransform.GetLocalPositionAndRotation(out Vector3 attackerCardLocalPos, out Quaternion attackerCardLocalRot);

        Transform defenderCardSlotTransform = defenderCardTransform.parent;
        defenderCardTransform.GetLocalPositionAndRotation(out Vector3 defenderCardLocalPos, out Quaternion defenderCardLocalRot);

        int mainDamage = attackManager.CalculateDamage(attacker, attacker.SelectedAbility.AbilityIndex, defender);
        int recoilDamage = Mathf.Max(1, mainDamage / 4);

        Vector3 dirToDefender = (defenderCardTransform.position - attackerCardTransform.position).normalized * 1.5f;

        GameObject attackerCardWrapper = new("cardWrapper");
        attackerCardWrapper.transform.SetParent(attackerCardSlotTransform);
        attackerCardWrapper.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        attackerCardWrapper.transform.localScale = Vector3.one;

        GameObject defenderCardWrapper = new("CardWrapper");
        defenderCardWrapper.transform.SetParent(defenderCardSlotTransform);
        defenderCardWrapper.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        defenderCardWrapper.transform.localScale = Vector3.one;

        attackerCardTransform.SetParent(attackerCardWrapper.transform);
        attackerCardTransform.SetLocalPositionAndRotation(attackerCardLocalPos, attackerCardLocalRot);

        defenderCardTransform.SetParent(defenderCardWrapper.transform);
        defenderCardTransform.SetLocalPositionAndRotation(defenderCardLocalPos, defenderCardLocalRot);

        Vector3 upPos = attackerCardWrapper.transform.position - dirToDefender * 1.2f + Vector3.up * 0.5f;

        yield return attackerCardWrapper.transform.DOMove(upPos, 0.4f).SetEase(Ease.OutBack).WaitForCompletion();
        yield return attackerCardWrapper.transform.DOLocalRotateQuaternion(Quaternion.Euler(0, 0, -20f), 0.2f).WaitForCompletion();
        yield return attackerCardTransform.DOShakePosition(0.4f, new Vector3(0.3f, 0.3f, 0), 30).WaitForCompletion();

        Vector3 hitPoint = defenderCardWrapper.transform.position + Vector3.up * 0.5f;

        Sequence strikeSequence = DOTween.Sequence();

        strikeSequence.Append(attackerCardWrapper.transform.DOMove(hitPoint, 0.1f).SetEase(Ease.InExpo));
        strikeSequence.Join(attackerCardTransform.DOScaleX(6.0f, 0.05f));

        yield return strikeSequence.WaitForCompletion();

        showDamagePopupCallback?.Invoke(defenderCardTransform.position + Vector3.up * 1.5f, mainDamage);
        showDamagePopupCallback?.Invoke(attackerCardTransform.position - Vector3.up * 2.5f, recoilDamage);

        Vector3 attackerFlyPos = hitPoint - dirToDefender * 3.5f + Vector3.up * 0.8f;
        Vector3 defenderFlyPos = defenderCardWrapper.transform.position + dirToDefender * 1.8f;

        Sequence collisionSequence = DOTween.Sequence();

        collisionSequence.Append(defenderCardWrapper.transform.DOMove(defenderFlyPos, 0.3f).SetEase(Ease.OutQuint));
        collisionSequence.Join(defenderCardTransform.DOShakePosition(0.5f, new Vector3(0.4f, 0.4f, 0), 20));
        collisionSequence.Join(attackerCardWrapper.transform.DOMove(attackerFlyPos, 0.3f).SetEase(Ease.OutQuint));
        collisionSequence.Join(attackerCardTransform.DOShakePosition(0.5f, new Vector3(0.3f, 0.3f, 0), 20));

        collisionSequence.Join(attackerCardTransform.DOScale(attackerCardStartScale, 0.2f));

        yield return collisionSequence.WaitForCompletion();

        if (!AnimationWithOutDamageMode.Mode)
        {
            attackManager.PerformAttack(attacker, defender, mainDamage);
            attacker.BattleState.ApplyDamage(recoilDamage);
        }
        else
        {
            GameManager gameManager = UnityEngine.Object.FindAnyObjectByType<GameManager>();
            if (gameManager != null && gameManager.IsMyTurn)
            {
                NetworkGameController networkGameController = UnityEngine.Object.FindAnyObjectByType<NetworkGameController>();
                if (networkGameController != null)
                {
                    int attackerSlotIndex = (int)attackerCardSlot.indexSlotInField;
                    int defenderSlotIndex = (int)defenderCardSlot.indexSlotInField;
                    networkGameController.RequestApplyDamage(attackerSlotIndex, attacker.SelectedAbility.AbilityIndex, defenderSlotIndex, recoilDamage);
                }
            }
        }

        yield return new WaitForSeconds(0.2f);

        Sequence returnSequence = DOTween.Sequence();
        returnSequence.Append(defenderCardWrapper.transform.DOLocalMove(Vector3.zero, 0.4f).SetEase(Ease.OutQuad));
        returnSequence.Join(defenderCardWrapper.transform.DOLocalRotateQuaternion(Quaternion.identity, 0.4f).SetEase(Ease.OutQuad));
        returnSequence.Join(attackerCardWrapper.transform.DOLocalMove(Vector3.zero, 0.4f).SetEase(Ease.OutQuad));
        returnSequence.Join(attackerCardWrapper.transform.DOLocalRotateQuaternion(Quaternion.identity, 0.4f).SetEase(Ease.OutQuad));
        yield return returnSequence.WaitForCompletion();

        attackerCardTransform.SetParent(attackerCardSlotTransform);
        attackerCardTransform.SetLocalPositionAndRotation(attackerCardLocalPos, attackerCardLocalRot);

        defenderCardTransform.SetParent(defenderCardSlotTransform);
        defenderCardTransform.SetLocalPositionAndRotation(defenderCardLocalPos, defenderCardLocalRot);

        UnityEngine.Object.Destroy(attackerCardWrapper);
        UnityEngine.Object.Destroy(defenderCardWrapper);
    }
}