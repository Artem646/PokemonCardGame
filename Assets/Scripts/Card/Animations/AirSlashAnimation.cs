using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;

public class AirSlashAnimation : BaseAttackAnimation
{
    private GameObject bladePrefab;

    public AirSlashAnimation()
    {
        bladePrefab = Resources.Load<GameObject>("VFX/AirSlashBlade");
    }

    public override IEnumerator PlayAnimation(BattleCardController attacker, BattleCardController defender, AttackManager attackManager, Action<Vector3, int> showDamagePopupCallback)
    {
        Transform attackerCardTransform = attacker.BattleCardView.CardRoot.transform;
        Transform defenderCardTransform = defender.BattleCardView.CardRoot.transform;

        attackerCardTransform.GetPositionAndRotation(out Vector3 cardStartPos, out Quaternion cardStartRot);
        Vector3 defenderCardStartPos = defenderCardTransform.localPosition;

        int totalDamage = attackManager.CalculateDamage(attacker, attacker.SelectedAbility.AbilityIndex, defender);

        yield return attackerCardTransform.DOShakePosition(0.3f, new Vector3(0.1f, 0.1f, 0), 30).WaitForCompletion();

        Transform exitPointTransform = attackerCardTransform.Find("AnimationObjects/StreamExitPoint");

        int bladeCount = 3;
        float interval = 0.2f;

        int damagePerBlade = Mathf.FloorToInt((float)totalDamage / bladeCount);
        int accumulatedDamage = 0;

        for (int i = 0; i < bladeCount; i++)
        {
            int currentHitDamage = (i == bladeCount - 1) ? (totalDamage - accumulatedDamage) : damagePerBlade;
            accumulatedDamage += currentHitDamage;

            LaunchBlade(i, exitPointTransform.position, attackerCardTransform, defenderCardTransform, currentHitDamage, showDamagePopupCallback);

            yield return new WaitForSeconds(interval);
        }

        yield return new WaitForSeconds(0.4f);

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

        yield return defenderCardTransform.DOLocalMove(defenderCardStartPos, 0.2f).WaitForCompletion();
        yield return attackerCardTransform.DORotateQuaternion(cardStartRot, 0.2f).WaitForCompletion();
        yield return attackerCardTransform.DOMove(cardStartPos, 0.1f).WaitForCompletion();
    }

    private void LaunchBlade(int index, Vector3 spawnPos, Transform attackerCardTransform, Transform defenderCardTransform, int damageAmount, Action<Vector3, int> showDamagePopupCallback)
    {
        GameObject bladeObject = UnityEngine.Object.Instantiate(bladePrefab, spawnPos, Quaternion.identity);

        float randomOffsetX = (index == 0) ? -0.4f : (index == 1) ? 0.4f : 0f;
        Vector3 targetPoint = defenderCardTransform.position + Vector3.up * 0.4f + (defenderCardTransform.right * randomOffsetX);

        Vector3 flightDirection = targetPoint - spawnPos;
        flightDirection.y = 0;
        bladeObject.transform.rotation = Quaternion.LookRotation(flightDirection, Vector3.up);

        Transform meshChild = bladeObject.transform.GetChild(0);

        float tiltAngle = (index == 0) ? 45f : (index == 1) ? -45f : 90f;
        meshChild.localRotation = Quaternion.Euler(0, 0, tiltAngle);

        Vector3 midPoint = Vector3.Lerp(spawnPos, targetPoint, 0.5f);
        float arcOffset = (index == 0) ? 1.0f : (index == 1) ? -1.0f : 0f;
        midPoint += bladeObject.transform.right * arcOffset;

        Vector3[] pathPoints = new Vector3[] { midPoint, targetPoint };

        Sequence flightBladeSequence = DOTween.Sequence();
        flightBladeSequence.Append(attackerCardTransform.DOShakePosition(0.2f, new Vector3(0.2f, 0.2f, 0), 20)).SetEase(Ease.OutQuad);
        flightBladeSequence.Join(bladeObject.transform.DOPath(pathPoints, 0.25f, PathType.CatmullRom).SetEase(Ease.InSine));
        flightBladeSequence.OnComplete(() =>
        {
            meshChild.gameObject.SetActive(false);
            showDamagePopupCallback?.Invoke(defenderCardTransform.position + Vector3.up * 1.5f + 2f * randomOffsetX * Vector3.right, damageAmount);
            defenderCardTransform.DOShakePosition(0.2f, new Vector3(0.2f, 0.2f, 0), 20).SetUpdate(true);
            UnityEngine.Object.Destroy(bladeObject, 0.2f);
        });
    }
}