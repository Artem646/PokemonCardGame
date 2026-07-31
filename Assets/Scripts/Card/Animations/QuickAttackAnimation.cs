using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;

public class QuickAttackAnimation : BaseAttackAnimation
{
    public override IEnumerator PlayAnimation(BattleCardController attacker, BattleCardController defender, AttackManager attackManager, Action<Vector3, int> showDamagePopupCallback)
    {
        Transform attackerCardTransform = attacker.BattleCardView.CardRoot.transform;
        Transform defenderCardTransform = defender.BattleCardView.CardRoot.transform;

        CardSlot attackerCardSlot = attackerCardTransform.parent.GetComponent<CardSlot>();
        CardSlot defenderCardSlot = defenderCardTransform.parent.GetComponent<CardSlot>();

        attackerCardTransform.GetPositionAndRotation(out Vector3 attackerCardStartPos, out Quaternion attackerCardStartRot);
        Vector3 attackerCardStartScale = attackerCardTransform.localScale;

        Transform defenderCardSlotTransform = defenderCardTransform.parent;
        defenderCardTransform.GetLocalPositionAndRotation(out Vector3 defenderCardLocalPos, out Quaternion defenderCardLocalRot);

        int damage = attackManager.CalculateDamage(attacker, attacker.SelectedAbility.AbilityIndex, defender);
        Vector3 dirToDefender = (defenderCardTransform.position - attackerCardTransform.position).normalized * 1.5f;

        GameObject defenderCardWrapper = new("CardWrapper");
        defenderCardWrapper.transform.SetParent(defenderCardSlotTransform);
        defenderCardWrapper.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        defenderCardWrapper.transform.localScale = Vector3.one;

        defenderCardTransform.SetParent(defenderCardWrapper.transform);
        defenderCardTransform.SetLocalPositionAndRotation(defenderCardLocalPos, defenderCardLocalRot);

        Vector3 upPos = attackerCardTransform.position - Vector3.forward * 0.5f;

        yield return attackerCardTransform.DOMove(upPos, 0.05f).SetEase(Ease.OutQuad).WaitForCompletion();
        yield return attackerCardTransform.DOLocalRotateQuaternion(Quaternion.Euler(-110f, -90f, 180f), 0.05f).WaitForCompletion();

        Vector3 hitPoint = defenderCardWrapper.transform.position + Vector3.up * 0.5f;

        Sequence strikeSequence = DOTween.Sequence();

        strikeSequence.Append(attackerCardTransform.DOMove(hitPoint, 0.05f).SetEase(Ease.InExpo));
        strikeSequence.Join(attackerCardTransform.DOScaleX(6.0f, 0.05f));

        yield return strikeSequence.WaitForCompletion();

        showDamagePopupCallback?.Invoke(defenderCardTransform.position + Vector3.up * 1.5f, damage);

        Vector3 defenderFlyPos = defenderCardWrapper.transform.position + dirToDefender * 1.8f;

        Sequence collisionSequence = DOTween.Sequence();

        collisionSequence.Append(defenderCardWrapper.transform.DOMove(defenderFlyPos, 0.2f).SetEase(Ease.OutQuint));
        collisionSequence.Join(defenderCardTransform.DOShakePosition(0.3f, new Vector3(0.4f, 0.4f, 0), 20));

        collisionSequence.Join(attackerCardTransform.DOShakePosition(0.3f, new Vector3(0.3f, 0.3f, 0), 20));
        collisionSequence.Join(attackerCardTransform.DOScale(attackerCardStartScale, 0.1f));

        yield return collisionSequence.WaitForCompletion();

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

        yield return new WaitForSeconds(0.25f);

        Sequence returnSequence = DOTween.Sequence();
        returnSequence.Append(defenderCardWrapper.transform.DOLocalMove(Vector3.zero, 0.3f).SetEase(Ease.OutQuad));
        returnSequence.Join(attackerCardTransform.DOMove(attackerCardStartPos, 0.3f).SetEase(Ease.OutQuad));
        returnSequence.Join(attackerCardTransform.DORotateQuaternion(attackerCardStartRot, 0.3f).SetEase(Ease.OutQuad));
        yield return returnSequence.WaitForCompletion();

        defenderCardTransform.SetParent(defenderCardSlotTransform);
        defenderCardTransform.SetLocalPositionAndRotation(defenderCardLocalPos, defenderCardLocalRot);

        UnityEngine.Object.Destroy(defenderCardWrapper);
    }
}