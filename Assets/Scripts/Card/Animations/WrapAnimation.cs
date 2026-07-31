using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class WrapAnimation : BaseAttackAnimation
{
    private GameObject snakePrefab;

    public WrapAnimation()
    {
        snakePrefab = Resources.Load<GameObject>("VFX/Wrap");
    }

    public override IEnumerator PlayAnimation(BattleCardController attacker, BattleCardController defender, AttackManager attackManager, Action<Vector3, int> showDamagePopupCallback)
    {
        Transform attackerCardTransform = attacker.BattleCardView.CardRoot.transform;
        Transform defenderCardTransform = defender.BattleCardView.CardRoot.transform;

        attackerCardTransform.GetPositionAndRotation(out Vector3 cardStartPos, out Quaternion cardStartRot);
        Transform exitPointTransform = attackerCardTransform.Find("AnimationObjects/StreamExitPoint");

        GameObject snakeObject = UnityEngine.Object.Instantiate(snakePrefab, exitPointTransform.position, exitPointTransform.rotation);
        SnakePathBinder binder = snakeObject.GetComponentInChildren<SnakePathBinder>();

        float snakeLength = binder.maxLength;
        Vector3 center = defenderCardTransform.position;

        float startHalfW = 3.05f;
        float startHalfL = 4.025f;

        float perimeter = (startHalfW * 2 + startHalfL * 2) * 2;
        float loops = (snakeLength / perimeter) + 0.5f;

        List<Vector3> pathPoints = new() { exitPointTransform.position + Vector3.up * 0.5f };

        int totalCorners = Mathf.CeilToInt(4 * loops);

        float heightPerCorner = 0.05f;
        float shrinkPerLoop = 0.3f;

        for (int i = 0; i <= totalCorners; i++)
        {
            int cornerIndex = i % 4;
            float currentLoopReal = i / 4.0f;

            float currentHalfW = Mathf.Max(0.5f, startHalfW * (1f - (currentLoopReal * shrinkPerLoop)));
            float currentHalfL = Mathf.Max(0.5f, startHalfL * (1f - (currentLoopReal * shrinkPerLoop)));

            float height = i * heightPerCorner;

            Vector3 offset = Vector3.zero;

            switch (cornerIndex)
            {
                case 0: offset = (-defenderCardTransform.up * currentHalfW) - (defenderCardTransform.forward * currentHalfL); break;
                case 1: offset = (-defenderCardTransform.up * currentHalfW) + (defenderCardTransform.forward * currentHalfL); break;
                case 2: offset = (defenderCardTransform.up * currentHalfW) + (defenderCardTransform.forward * currentHalfL); break;
                case 3: offset = (defenderCardTransform.up * currentHalfW) - (defenderCardTransform.forward * currentHalfL); break;
            }

            Vector3 cornerPoint = center + offset + Vector3.up * 0.3f + Vector3.up * height;
            pathPoints.Add(cornerPoint);
        }

        Vector3[] worldPath = pathPoints.ToArray();
        int totalDamage = attackManager.CalculateDamage(attacker, attacker.SelectedAbility.AbilityIndex, defender);

        float pathProgress = 0f;
        float duration = 1.5f;

        yield return DOTween.To(() => pathProgress, x =>
        {
            pathProgress = x;
            binder.SetState(worldPath, pathProgress);
        }, 1f, duration).SetEase(Ease.OutSine).WaitForCompletion();

        for (int i = 0; i < 3; i++)
        {
            int dmg = (i == 2) ? (totalDamage / 3 + totalDamage % 3) : totalDamage / 3;
            showDamagePopupCallback?.Invoke(center + Vector3.up * 1.5f, dmg);

            defenderCardTransform.DOPunchScale(new Vector3(-0.15f, -0.15f, -0.15f), 0.3f);
            defenderCardTransform.DOShakePosition(0.3f, new Vector3(0.2f, 0.2f, 0), 30);
            yield return new WaitForSeconds(0.5f);
        }

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

        yield return DOTween.To(() => pathProgress, x =>
        {
            pathProgress = x;
            binder.SetState(worldPath, pathProgress);
        }, 0f, duration).SetEase(Ease.InSine).WaitForCompletion();

        UnityEngine.Object.Destroy(snakeObject);
    }
}