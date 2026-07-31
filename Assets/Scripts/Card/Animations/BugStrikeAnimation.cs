using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;

public class BugStrikeAnimation : BaseAttackAnimation
{
    private GameObject bugPrefab;

    public BugStrikeAnimation()
    {
        bugPrefab = Resources.Load<GameObject>("VFX/BugStrike");
    }

    public override IEnumerator PlayAnimation(BattleCardController attacker, BattleCardController defender, AttackManager attackManager, Action<Vector3, int> showDamagePopupCallback)
    {
        Transform attackerCardTransform = attacker.BattleCardView.CardRoot.transform;
        Transform defenderCardTransform = defender.BattleCardView.CardRoot.transform;

        CardSlot attackerCardSlot = attackerCardTransform.parent.GetComponent<CardSlot>();
        CardSlot defenderCardSlot = defenderCardTransform.parent.GetComponent<CardSlot>();

        attackerCardTransform.GetPositionAndRotation(out Vector3 attackerStartPos, out Quaternion attackerStartRot);

        Transform defenderCardSlotTransform = defenderCardTransform.parent;
        defenderCardTransform.GetLocalPositionAndRotation(out Vector3 defenderCardLocalPos, out Quaternion defenderCardLocalRot);

        int damage = attackManager.CalculateDamage(attacker, attacker.SelectedAbility.AbilityIndex, defender);
        Vector3 dirToDefender = (defenderCardTransform.position - attackerCardTransform.position).normalized;

        GameObject defenderCardWrapper = new("CardWrapper");
        defenderCardWrapper.transform.SetParent(defenderCardSlotTransform);
        defenderCardWrapper.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        defenderCardWrapper.transform.localScale = Vector3.one;

        defenderCardTransform.SetParent(defenderCardWrapper.transform);
        defenderCardTransform.SetLocalPositionAndRotation(defenderCardLocalPos, defenderCardLocalRot);

        GameObject bugObject = UnityEngine.Object.Instantiate(bugPrefab, attackerCardTransform);
        Transform bugTransform = bugObject.transform.Find("Bug");
        bugTransform.localScale = new(0.005f, 0.005f, 0);
        bugTransform.localRotation = Quaternion.Euler(0, -90f, -90f);

        bugTransform.DOLocalMoveX(0.0012f, 0.2f).SetEase(Ease.OutQuad);
        yield return bugTransform.DOScale(new Vector3(0.035f, 0.035f, 0), 0.3f).SetEase(Ease.OutBack).WaitForCompletion();

        yield return attackerCardTransform.DOShakePosition(0.4f, new Vector3(0.1f, 0.1f, 0), 40).WaitForCompletion();

        bugTransform.DOPunchRotation(new Vector3(0, 40f, 30f), 0.2f, 8, 1f).SetLoops(-1, LoopType.Restart);

        Vector3 combatStancePos = defenderCardTransform.position - dirToDefender * 3.5f + Vector3.up * 0.2f;
        Vector3 midPoint = Vector3.Lerp(bugTransform.position, combatStancePos, 0.5f) + Vector3.up * 1.2f - Vector3.right;
        yield return bugTransform.DOPath(new Vector3[] { midPoint, combatStancePos }, 0.6f, PathType.CatmullRom)
            .SetEase(Ease.InSine)
            .OnUpdate(() =>
            {
                Vector3 direction = (combatStancePos - bugTransform.position).normalized;
                direction.y = 0;
                if (direction != Vector3.zero)
                {
                    Quaternion lookRot = Quaternion.LookRotation(direction);
                    bugTransform.rotation = lookRot * Quaternion.Euler(90f, 0, 0);
                }
            })
            .WaitForCompletion();

        bugTransform.DOKill();

        bugTransform.localRotation = Quaternion.Euler(0, -90f, -90f);
        yield return bugTransform.DOLocalMoveX(0.0012f, 0.2f).SetEase(Ease.OutQuad).WaitForCompletion();

        Vector3 recoilPos = combatStancePos - dirToDefender * 0.5f + Vector3.up * 0.4f;
        Vector3 hitPoint = defenderCardTransform.position + Vector3.up * 0.4f;

        Sequence strikeSequence = DOTween.Sequence();

        strikeSequence.Append(bugTransform.DOMove(recoilPos, 0.15f).SetEase(Ease.OutQuad));
        strikeSequence.Join(bugTransform.DORotateQuaternion(Quaternion.LookRotation(dirToDefender) * Quaternion.Euler(110f, 0, 0), 0.15f));
        strikeSequence.Append(bugTransform.DOMove(hitPoint, 0.1f).SetEase(Ease.InExpo));

        yield return strikeSequence.WaitForCompletion();

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

        Sequence impactSequence = DOTween.Sequence();

        impactSequence.Append(defenderCardWrapper.transform.DOMove(defenderCardWrapper.transform.position + dirToDefender * 1.8f, 0.25f).SetEase(Ease.OutQuint));
        impactSequence.Join(defenderCardTransform.DOShakePosition(0.5f, new Vector3(0.5f, 0.5f, 0), 30));
        impactSequence.Join(defenderCardTransform.DOPunchRotation(new Vector3(30f, 0, 0), 0.4f));

        yield return impactSequence.WaitForCompletion();

        bugTransform.DOScale(0, 0.1f);
        UnityEngine.Object.Destroy(bugObject);

        yield return new WaitForSeconds(0.3f);

        yield return defenderCardWrapper.transform.DOLocalMove(defenderCardLocalPos, 0.4f).SetEase(Ease.OutQuad).WaitForCompletion();

        defenderCardTransform.SetParent(defenderCardSlotTransform);
        defenderCardTransform.SetLocalPositionAndRotation(defenderCardLocalPos, defenderCardLocalRot);

        UnityEngine.Object.Destroy(defenderCardWrapper);
    }
}