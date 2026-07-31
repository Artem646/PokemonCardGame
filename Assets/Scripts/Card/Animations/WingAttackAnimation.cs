using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;

public class WingAttackAnimation : BaseAttackAnimation
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

        int damage = attackManager.CalculateDamage(attacker, attacker.SelectedAbility.AbilityIndex, defender);
        Vector3 dirToDefender = (defenderCardTransform.position - attackerCardTransform.position).normalized;

        Transform leftWingWrapper = attackerCardTransform.Find("AnimationObjects/Wings/LeftWingWrapper");
        Transform rightWingWrapper = attackerCardTransform.Find("AnimationObjects/Wings/RightWingWrapper");

        Vector3 leftWingWrapperStartScale = leftWingWrapper.localScale;
        Vector3 rightWingWrapperStartScale = rightWingWrapper.localScale;

        GameObject defenderCardWrapper = new("CardWrapper");
        defenderCardWrapper.transform.SetParent(defenderCardSlotTransform);
        defenderCardWrapper.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        defenderCardWrapper.transform.localScale = Vector3.one;

        defenderCardTransform.SetParent(defenderCardWrapper.transform);
        defenderCardTransform.SetLocalPositionAndRotation(defenderCardLocalPos, defenderCardLocalRot);

        Vector3 upPos = attackerCardTransform.position + Vector3.up * 1.5f;

        leftWingWrapper.gameObject.SetActive(true);
        rightWingWrapper.gameObject.SetActive(true);

        Sequence unfoldSequence = DOTween.Sequence();

        unfoldSequence.Append(attackerCardTransform.DOMove(upPos, 0.4f).SetEase(Ease.OutBack));
        unfoldSequence.Join(attackerCardTransform.DOLocalRotateQuaternion(Quaternion.Euler(-70f, -90f, 180f), 0.35f).SetEase(Ease.OutQuad));
        unfoldSequence.Join(leftWingWrapper.DOScale(1f, 0.4f).SetEase(Ease.OutBack));
        unfoldSequence.Join(rightWingWrapper.DOScale(1f, 0.4f).SetEase(Ease.OutBack));

        yield return unfoldSequence.WaitForCompletion();

        yield return new WaitForSeconds(0.2f);

        Vector3 hitPoint = defenderCardWrapper.transform.position + Vector3.up * 0.5f;

        Sequence attackSequence = DOTween.Sequence();

        attackSequence.Append(attackerCardTransform.DOLocalRotateQuaternion(Quaternion.Euler(-130f, -90f, 180f), 0.15f).SetEase(Ease.InExpo));
        attackSequence.Join(attackerCardTransform.DOMove(hitPoint, 0.15f).SetEase(Ease.InExpo));

        attackSequence.Join(leftWingWrapper.DOScale(1.2f, 0.1f));
        attackSequence.Join(rightWingWrapper.DOScale(1.2f, 0.1f));

        yield return attackSequence.WaitForCompletion();

        showDamagePopupCallback?.Invoke(defenderCardTransform.position + Vector3.up * 1.5f, damage);

        if (!AnimationWithOutDamageMode.Mode) attackManager.PerformAttack(attacker, defender, damage);
        else if (GameTypeConfig.CurrentType == GameType.Multiplayer)
        {
            GameManager gameManager = UnityEngine.Object.FindAnyObjectByType<GameManager>();
            if (gameManager != null && gameManager.IsMyTurn)
            {
                NetworkGameController networkGameController = UnityEngine.Object.FindAnyObjectByType<NetworkGameController>();
                if (networkGameController != null)
                {
                    int attackerSlotIndex = (int)attackerCardSlot.indexSlotInField;
                    int defenderSlotIndex = (int)defenderCardSlot.indexSlotInField;
                    networkGameController.RequestApplyDamage(attackerSlotIndex, attacker.SelectedAbility.AbilityIndex, defenderSlotIndex);
                }
            }
        }

        Vector3 defenderFlyPos = defenderCardWrapper.transform.position + dirToDefender * 1.5f;

        Sequence collisionSequence = DOTween.Sequence();

        collisionSequence.Append(defenderCardWrapper.transform.DOMove(defenderFlyPos, 0.3f).SetEase(Ease.OutQuint));
        collisionSequence.Join(defenderCardTransform.DOShakePosition(0.3f, new Vector3(0.5f, 0.5f, 0), 30));
        collisionSequence.Join(leftWingWrapper.DOScale(leftWingWrapperStartScale, 0.1f).SetEase(Ease.InQuad));
        collisionSequence.Join(rightWingWrapper.DOScale(rightWingWrapperStartScale, 0.1f).SetEase(Ease.InQuad));

        yield return collisionSequence.WaitForCompletion();

        leftWingWrapper.gameObject.SetActive(false);
        rightWingWrapper.gameObject.SetActive(false);

        Sequence returnSequence = DOTween.Sequence();
        returnSequence.Append(defenderCardWrapper.transform.DOLocalMove(Vector3.zero, 0.4f).SetEase(Ease.OutQuad));
        returnSequence.Join(attackerCardTransform.DOLocalMove(attackerCardLocalPos, 0.4f).SetEase(Ease.OutQuad));
        returnSequence.Join(attackerCardTransform.DORotateQuaternion(attackerCardStartRot, 0.4f).SetEase(Ease.OutQuad));
        yield return returnSequence.WaitForCompletion();

        defenderCardTransform.SetParent(defenderCardSlotTransform);
        defenderCardTransform.SetLocalPositionAndRotation(defenderCardLocalPos, defenderCardLocalRot);

        UnityEngine.Object.Destroy(defenderCardWrapper);
    }
}