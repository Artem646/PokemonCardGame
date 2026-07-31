using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;

public class RazorLeafAnimation : BaseAttackAnimation
{
    private GameObject leafPrefab;

    public RazorLeafAnimation()
    {
        leafPrefab = Resources.Load<GameObject>("VFX/RazorLeaf");
    }

    public override IEnumerator PlayAnimation(BattleCardController attacker, BattleCardController defender, AttackManager attackManager, Action<Vector3, int> showDamagePopupCallback)
    {
        Transform attackerCardTransform = attacker.BattleCardView.CardRoot.transform;
        Transform defenderCardTransform = defender.BattleCardView.CardRoot.transform;

        attackerCardTransform.GetPositionAndRotation(out Vector3 cardStartPos, out Quaternion cardStartRot);
        Vector3 attackerCardLocalStartPos = attackerCardTransform.localPosition;
        Vector3 defenderCardLocalStartPos = defenderCardTransform.localPosition;

        int totalDamage = attackManager.CalculateDamage(attacker, attacker.SelectedAbility.AbilityIndex, defender);

        yield return attackerCardTransform.DOLocalRotateQuaternion(Quaternion.Euler(-75f, -90f, 180f), 0.3f).SetEase(Ease.OutBack).WaitForCompletion();
        attackerCardTransform.DOShakePosition(0.4f, new Vector3(0.1f, 0.1f, 0), 40);

        Transform exitPointTransform = attackerCardTransform.Find("AnimationObjects/StreamExitPoint");

        Vector3 dirToEnemy = (defenderCardTransform.position - attackerCardTransform.position).normalized;
        Vector3 sideDir = Vector3.Cross(dirToEnemy, Vector3.up).normalized;

        int leafCount = 20;
        float launchDuration = 1f;
        int damagePops = 3;
        int damagePerPop = totalDamage / damagePops;
        int damageDealt = 0;

        for (int i = 0; i < leafCount; i++)
        {
            int currentHitDamage = 0;
            if (i % (leafCount / damagePops) == 0 || i == leafCount - 1)
            {
                if (damageDealt < totalDamage)
                {
                    currentHitDamage = (i == leafCount - 1) ? (totalDamage - damageDealt) : damagePerPop;
                    damageDealt += currentHitDamage;
                }
            }

            LaunchLeaf(exitPointTransform.position, attackerCardTransform, defenderCardTransform, sideDir, currentHitDamage, showDamagePopupCallback);

            yield return new WaitForSeconds(launchDuration / leafCount);
        }

        yield return new WaitForSeconds(0.3f);

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

        yield return defenderCardTransform.DOLocalMove(defenderCardLocalStartPos, 0.1f).WaitForCompletion();
        yield return attackerCardTransform.DORotateQuaternion(cardStartRot, 0.3f).SetEase(Ease.OutQuad).WaitForCompletion();
        yield return attackerCardTransform.DOMove(cardStartPos, 0.1f).WaitForCompletion();
        yield return attackerCardTransform.DOLocalMove(attackerCardLocalStartPos, 0.05f).WaitForCompletion();
    }

    private void LaunchLeaf(Vector3 spawnPos, Transform attackerCardTransform, Transform defenderCardTransform, Vector3 sideDir, int damageAmount, Action<Vector3, int> showDamagePopupCallback)
    {
        GameObject leafObject = UnityEngine.Object.Instantiate(leafPrefab, spawnPos, Quaternion.identity);
        Transform leafTransform = leafObject.transform.Find("Leaf");
        leafTransform.localScale = new(2.5f, 1f, 2.5f);

        float randomRight = UnityEngine.Random.Range(-2f, 2f);
        float randomForward = UnityEngine.Random.Range(-2.5f, 0.5f);
        Vector3 targetPoint = defenderCardTransform.position + Vector3.up * 0.1f + (defenderCardTransform.right * randomRight) + (defenderCardTransform.forward * randomForward);

        float randomUpArc = UnityEngine.Random.Range(0.5f, 2.0f);
        float randomSideArc = UnityEngine.Random.Range(-1.0f, 1.0f);
        Vector3 midPoint = Vector3.Lerp(spawnPos, targetPoint, 0.5f) + (Vector3.up * randomUpArc) + (sideDir * randomSideArc);

        Vector3[] pathPoints = new Vector3[] { midPoint, targetPoint };

        leafObject.transform.LookAt(midPoint);

        Sequence flightLeafSequence = DOTween.Sequence();
        flightLeafSequence.Append(attackerCardTransform.DOShakePosition(0.1f, new Vector3(0.1f, 0.1f, 0), 20)).SetEase(Ease.OutQuad);
        flightLeafSequence.Join(leafObject.transform.DOPath(pathPoints, 0.35f, PathType.CatmullRom).SetEase(Ease.Linear).SetLookAt(0.01f));
        flightLeafSequence.OnComplete(() =>
        {
            if (damageAmount > 0)
                showDamagePopupCallback?.Invoke(targetPoint + Vector3.up * 1.3f, damageAmount);
            defenderCardTransform.DOShakePosition(0.1f, new Vector3(0.1f, 0.1f, 0), 20).SetUpdate(true);
            UnityEngine.Object.Destroy(leafObject, 0.1f);
        });
    }
}