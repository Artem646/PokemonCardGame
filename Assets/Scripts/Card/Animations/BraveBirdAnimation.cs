using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;

public class BraveBirdAnimation : BaseAttackAnimation
{
    public override IEnumerator PlayAnimation(BattleCardController attacker, BattleCardController defender, AttackManager attackManager, Action<Vector3, int> showDamagePopupCallback)
    {
        Transform attackerCardTransform = attacker.BattleCardView.CardRoot.transform;
        Transform defenderCardTransform = defender.BattleCardView.CardRoot.transform;

        CardSlot attackerCardSlot = attackerCardTransform.parent.GetComponent<CardSlot>();
        CardSlot defenderCardSlot = defenderCardTransform.parent.GetComponent<CardSlot>();

        attackerCardTransform.GetPositionAndRotation(out Vector3 attackerCardStartPos, out Quaternion attackerCardStartRot);

        Transform attackerCardSlotTransform = attackerCardTransform.parent;
        attackerCardTransform.GetLocalPositionAndRotation(out Vector3 attackerCardLocalPos, out Quaternion attackerCardLocalRot);

        Transform defenderCardSlotTransform = defenderCardTransform.parent;
        defenderCardTransform.GetLocalPositionAndRotation(out Vector3 defenderCardLocalPos, out Quaternion defenderCardLocalRot);

        int mainDamage = attackManager.CalculateDamage(attacker, attacker.SelectedAbility.AbilityIndex, defender);
        int recoilDamage = Mathf.Max(1, mainDamage / 4);

        Vector3 dirToDefender = (defenderCardTransform.position - attackerCardTransform.position).normalized;

        Transform leftWingWrapper = attackerCardTransform.Find("Wings/LeftWingWrapper");
        Transform rightWingWrapper = attackerCardTransform.Find("Wings/RightWingWrapper");

        Vector3 leftWingWrapperStartScale = leftWingWrapper.localScale;
        Vector3 rightWingWrapperStartScale = rightWingWrapper.localScale;

        GameObject defenderCardWrapper = new("CardWrapper");
        defenderCardWrapper.transform.SetParent(defenderCardSlotTransform);
        defenderCardWrapper.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        defenderCardWrapper.transform.localScale = Vector3.one;

        defenderCardTransform.SetParent(defenderCardWrapper.transform);
        defenderCardTransform.SetLocalPositionAndRotation(defenderCardLocalPos, defenderCardLocalRot);

        Vector3 upPos = attackerCardTransform.position - dirToDefender * 4f + Vector3.up * 1.5f - Vector3.forward * 1.5f;

        leftWingWrapper.gameObject.SetActive(true);
        rightWingWrapper.gameObject.SetActive(true);

        Sequence unfoldSequence = DOTween.Sequence();

        unfoldSequence.Append(attackerCardTransform.DOMove(upPos, 0.4f).SetEase(Ease.OutQuad));
        unfoldSequence.Join(attackerCardTransform.DOLocalRotateQuaternion(Quaternion.Euler(-100f, -90f, 180f), 0.35f).SetEase(Ease.OutQuad));
        unfoldSequence.Join(leftWingWrapper.DOScale(1f, 0.1f).SetEase(Ease.OutQuad));
        unfoldSequence.Join(rightWingWrapper.DOScale(1f, 0.1f).SetEase(Ease.OutQuad));

        yield return unfoldSequence.WaitForCompletion();

        yield return new WaitForSeconds(0.1f);

        Vector3 hitPoint = defenderCardWrapper.transform.position + Vector3.up * 0.5f;

        Sequence attackSequence = DOTween.Sequence();

        attackSequence.Append(attackerCardTransform.DOLocalRotateQuaternion(Quaternion.Euler(-130f, -90f, 180f), 0.1f).SetEase(Ease.InExpo));
        attackSequence.Join(attackerCardTransform.DOMove(hitPoint, 0.1f).SetEase(Ease.InExpo));

        attackSequence.Join(leftWingWrapper.DOScale(new Vector3(1f, 0.3f, 1f), 0.1f).SetEase(Ease.OutQuad));
        attackSequence.Join(rightWingWrapper.DOScale(new Vector3(1f, 0.3f, 1f), 0.1f).SetEase(Ease.OutQuad));

        yield return attackSequence.WaitForCompletion();

        showDamagePopupCallback?.Invoke(defenderCardTransform.position + Vector3.up * 1.5f, mainDamage);
        showDamagePopupCallback?.Invoke(attackerCardTransform.position - Vector3.up * 2.5f, recoilDamage);

        Vector3 attackerFlyPos = hitPoint - dirToDefender * 3.5f + Vector3.up * 0.8f;
        Vector3 defenderFlyPos = defenderCardWrapper.transform.position + dirToDefender * 1.5f;

        Sequence collisionSequence = DOTween.Sequence();

        collisionSequence.Append(defenderCardWrapper.transform.DOMove(defenderFlyPos, 0.3f).SetEase(Ease.OutQuint));
        collisionSequence.Join(defenderCardTransform.DOShakePosition(0.3f, new Vector3(0.5f, 0.5f, 0), 30));
        collisionSequence.Join(attackerCardTransform.DOMove(attackerFlyPos, 0.3f).SetEase(Ease.OutQuint));
        collisionSequence.Append(leftWingWrapper.DOScale(leftWingWrapperStartScale, 0.1f).SetEase(Ease.InQuad));
        collisionSequence.Join(rightWingWrapper.DOScale(rightWingWrapperStartScale, 0.1f).SetEase(Ease.InQuad));

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

        leftWingWrapper.gameObject.SetActive(false);
        rightWingWrapper.gameObject.SetActive(false);

        Sequence returnSequence = DOTween.Sequence();
        returnSequence.Append(defenderCardWrapper.transform.DOLocalMove(Vector3.zero, 0.4f).SetEase(Ease.OutQuad));
        returnSequence.Join(attackerCardTransform.DOMove(attackerCardStartPos, 0.4f).SetEase(Ease.OutQuad));
        returnSequence.Join(attackerCardTransform.DORotateQuaternion(attackerCardStartRot, 0.4f).SetEase(Ease.OutQuad));
        yield return returnSequence.WaitForCompletion();

        defenderCardTransform.SetParent(defenderCardSlotTransform);
        defenderCardTransform.SetLocalPositionAndRotation(defenderCardLocalPos, defenderCardLocalRot);

        UnityEngine.Object.Destroy(defenderCardWrapper);
    }
}