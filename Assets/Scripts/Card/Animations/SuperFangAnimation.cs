using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;

public class SuperFangAnimation : BaseAttackAnimation
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
        Vector3 dirToDefender = (defenderCardTransform.position - attackerCardTransform.position).normalized;

        Transform fangs = attackerCardTransform.Find("AnimationObjects/Fangs");
        Transform topLeft = fangs.Find("TopLeft");
        Transform topRight = fangs.Find("TopRight");
        Transform bottomLeft = fangs.Find("BottomLeft");
        Transform bottomRight = fangs.Find("BottomRight");

        GameObject defenderCardWrapper = new("CardWrapper");
        defenderCardWrapper.transform.SetParent(defenderCardSlotTransform);
        defenderCardWrapper.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        defenderCardWrapper.transform.localScale = Vector3.one;

        defenderCardTransform.SetParent(defenderCardWrapper.transform);
        defenderCardTransform.SetLocalPositionAndRotation(defenderCardLocalPos, defenderCardLocalRot);

        Vector3 combatStancePos = defenderCardTransform.position - dirToDefender * 5f + Vector3.up * 3f + Vector3.forward * 2.5f;

        yield return attackerCardTransform.DOMove(combatStancePos, 0.3f).SetEase(Ease.OutQuad).WaitForCompletion();
        attackerCardTransform.DOLocalRotateQuaternion(Quaternion.Euler(-60f, -90f, 180f), 0.2f).SetEase(Ease.OutBack);

        fangs.gameObject.SetActive(true);
        fangs.localScale = Vector3.zero;
        fangs.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutQuad);

        yield return new WaitForSeconds(0.5f);

        Sequence biteSequence = DOTween.Sequence();

        biteSequence.Append(attackerCardTransform.DOLocalRotate(new Vector3(0, 0, 180f), 0.25f, RotateMode.LocalAxisAdd).SetEase(Ease.OutQuad));
        biteSequence.Append(attackerCardTransform.DOMove(defenderCardTransform.position + Vector3.up * 0.2f, 0.2f).SetEase(Ease.InExpo));

        yield return biteSequence.WaitForCompletion();

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

        defenderCardTransform.DOPunchScale(new Vector3(-0.35f, -0.35f, 0), 0.3f, 15, 1f);
        defenderCardTransform.DOShakePosition(0.3f, new Vector3(0.5f, 0.5f, 0), 30);

        fangs.DOScale(Vector3.zero, 0.15f).SetEase(Ease.InBack).OnComplete(() => fangs.gameObject.SetActive(false));

        yield return new WaitForSeconds(0.3f);

        Sequence returnSequence = DOTween.Sequence();
        returnSequence.Append(defenderCardWrapper.transform.DOLocalMove(Vector3.zero, 0.1f).SetEase(Ease.OutQuad));
        returnSequence.Join(attackerCardTransform.DOMove(combatStancePos, 0.3f).SetEase(Ease.OutQuad));
        returnSequence.Append(attackerCardTransform.DOLocalRotate(new Vector3(0, 0, 180f), 0.25f, RotateMode.LocalAxisAdd).SetEase(Ease.OutQuad));
        returnSequence.Append(attackerCardTransform.DOMove(attackerCardStartPos, 0.3f).SetEase(Ease.OutQuad));
        returnSequence.Join(attackerCardTransform.DORotateQuaternion(attackerCardStartRot, 0.15f).SetEase(Ease.OutQuad));
        yield return returnSequence.WaitForCompletion();

        defenderCardTransform.SetParent(defenderCardSlotTransform);
        defenderCardTransform.SetLocalPositionAndRotation(defenderCardLocalPos, defenderCardLocalRot);

        UnityEngine.Object.Destroy(defenderCardWrapper);
    }
}