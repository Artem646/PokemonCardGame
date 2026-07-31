using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;

public class PoisonStingAnimation : BaseAttackAnimation
{
    private GameObject poisonStingPrefab;

    public PoisonStingAnimation()
    {
        poisonStingPrefab = Resources.Load<GameObject>("VFX/PoisonSting");
    }

    public override IEnumerator PlayAnimation(BattleCardController attacker, BattleCardController defender, AttackManager attackManager, Action<Vector3, int> showDamagePopupCallback)
    {
        Transform attackerCardTransform = attacker.BattleCardView.CardRoot.transform;
        Transform defenderCardTransform = defender.BattleCardView.CardRoot.transform;

        attackerCardTransform.GetPositionAndRotation(out Vector3 cardStartPos, out Quaternion cardStartRot);

        Vector3 halfwayPoint = Vector3.Lerp(cardStartPos, defenderCardTransform.position, 0.45f);
        halfwayPoint.y += 0.4f;

        Vector3 backPos = cardStartPos - (defenderCardTransform.position - cardStartPos).normalized * 0.5f;
        yield return attackerCardTransform.DOMove(backPos, 0.2f).SetEase(Ease.OutQuad).WaitForCompletion();

        Sequence mainActionSequence = DOTween.Sequence();

        mainActionSequence.Append(attackerCardTransform.DOMove(halfwayPoint, 0.3f).SetEase(Ease.OutCubic));
        mainActionSequence.Join(attackerCardTransform.DOLocalRotateQuaternion(Quaternion.Euler(10f, -90f, 180f), 0.35f).SetEase(Ease.InExpo).OnComplete(() =>
            {
                Vector3 spawnPoint;
                Transform exitPointTransform = attackerCardTransform.Find("AnimationObjects/TailExitPoint");
                spawnPoint = exitPointTransform.position + Vector3.back * 2f;

                attacker.BattleCardView.CardRoot.GetComponent<MonoBehaviour>().StartCoroutine(
                    LaunchStingProjectile(spawnPoint, defenderCardTransform, attacker, defender, attackManager, showDamagePopupCallback)
                );
            }
        ));

        yield return mainActionSequence.WaitForCompletion();

        yield return new WaitForSeconds(0.1f);

        Sequence returnSequence = DOTween.Sequence();
        returnSequence.Append(attackerCardTransform.DOMove(cardStartPos, 0.35f).SetEase(Ease.OutQuad));
        returnSequence.Join(attackerCardTransform.DORotateQuaternion(cardStartRot, 0.35f).SetEase(Ease.OutQuad));
        yield return returnSequence.WaitForCompletion();
    }

    private IEnumerator LaunchStingProjectile(Vector3 startPoint, Transform target, BattleCardController attacker, BattleCardController defender, AttackManager attackManager, Action<Vector3, int> showDamagePopup)
    {
        GameObject projectile = new("PoisonStingProjectile");
        projectile.transform.position = startPoint;

        GameObject poisonStingObject = UnityEngine.Object.Instantiate(poisonStingPrefab, projectile.transform);
        poisonStingObject.transform.localPosition = Vector3.zero;
        poisonStingObject.transform.localScale = Vector3.one;

        TrailRenderer sting = poisonStingObject.GetComponentInChildren<TrailRenderer>();
        sting.Clear();

        Vector3 targetPos = target.position + Vector3.up * 0.4f + Vector3.left * 1.5f;
        Vector3 midPoint = Vector3.Lerp(startPoint, targetPos, 0.5f) + Vector3.up * 1.5f + Vector3.right * 2.0f;

        yield return projectile.transform.DOPath(new Vector3[] { midPoint, targetPos }, 0.35f, PathType.CatmullRom)
            .SetEase(Ease.InQuad).WaitForCompletion();

        int damage = attackManager.CalculateDamage(attacker, attacker.SelectedAbility.AbilityIndex, defender);
        showDamagePopup?.Invoke(target.position, damage);

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

        target.DOShakePosition(0.3f, new Vector3(0.5f, 0.5f, 0), 20, 90, false, true);
        target.DOPunchScale(new Vector3(0.2f, 0.2f, 0.2f), 0.3f);

        poisonStingObject.transform.SetParent(null);
        UnityEngine.Object.Destroy(projectile);
        UnityEngine.Object.Destroy(poisonStingObject, 0.7f);
    }
}