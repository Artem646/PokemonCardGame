using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class GrassKnotAnimation : BaseAttackAnimation
{
    private GameObject vinePrefab;

    public GrassKnotAnimation()
    {
        vinePrefab = Resources.Load<GameObject>("VFX/GrassKnot");
    }

    public override IEnumerator PlayAnimation(BattleCardController attacker, BattleCardController defender, AttackManager attackManager, Action<Vector3, int> showDamagePopupCallback)
    {
        Transform attackerCardTransform = attacker.BattleCardView.CardRoot.transform;
        Transform defenderCardTransform = defender.BattleCardView.CardRoot.transform;

        attackerCardTransform.GetPositionAndRotation(out Vector3 cardStartPos, out Quaternion cardStartRot);

        Transform pointsContainer = defenderCardTransform.Find("AnimationObjects/EmptyPoints");
        pointsContainer.gameObject.SetActive(true);

        List<Transform> allPoints = new();
        foreach (Transform child in pointsContainer)
            allPoints.Add(child);

        int totalDamage = attackManager.CalculateDamage(attacker, attacker.SelectedAbility.AbilityIndex, defender);

        yield return attackerCardTransform.DOMove(cardStartPos + Vector3.up * 0.8f, 0.4f).SetEase(Ease.OutQuad).WaitForCompletion();
        attackerCardTransform.DOShakePosition(0.5f, new Vector3(0.1f, 0.1f, 0), 30);

        int vinesCount = 20;
        GameObject[] vineObjects = new GameObject[vinesCount];
        LineRenderer[] lines = new LineRenderer[vinesCount];
        Vector3[] vineStartPoints = new Vector3[vinesCount];
        Vector3[] vineEndPoints = new Vector3[vinesCount];

        for (int i = 0; i < vinesCount; i++)
        {
            vineObjects[i] = UnityEngine.Object.Instantiate(vinePrefab);
            lines[i] = vineObjects[i].GetComponentInChildren<LineRenderer>();
            lines[i].positionCount = 20;
            lines[i].widthMultiplier = 0;
            lines[i].useWorldSpace = true;

            vineEndPoints[i] = allPoints[i].position;

            Vector3 offset = (vineEndPoints[i] - defenderCardTransform.position).normalized * 0.7f;
            vineStartPoints[i] = defenderCardTransform.position + offset + Vector3.down * 1.0f;
        }

        float progress = 0f;
        yield return DOTween.To(() => progress, x => progress = x, 1f, 0.8f)
            .SetEase(Ease.OutBack)
            .OnUpdate(() => UpdateVines(lines, vineStartPoints, vineEndPoints, progress, vinesCount))
            .WaitForCompletion();

        showDamagePopupCallback?.Invoke(defenderCardTransform.position + Vector3.up * 1.5f, totalDamage);

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

        defenderCardTransform.DOPunchScale(new Vector3(-0.15f, -0.15f, -0.15f), 0.5f);
        defenderCardTransform.DOShakePosition(0.5f, new Vector3(0.3f, 0.3f, 0), 40);

        yield return new WaitForSeconds(1f);

        yield return DOTween.To(() => progress, x => progress = x, 0f, 0.8f)
            .SetEase(Ease.InSine)
            .OnUpdate(() => UpdateVines(lines, vineStartPoints, vineEndPoints, progress, vinesCount))
            .WaitForCompletion();

        pointsContainer.gameObject.SetActive(false);
        foreach (var vine in vineObjects) UnityEngine.Object.Destroy(vine, 0.3f);

        yield return attackerCardTransform.DOMove(cardStartPos, 0.4f).SetEase(Ease.OutQuad).WaitForCompletion();
    }

    private void UpdateVines(LineRenderer[] lines, Vector3[] startPoints, Vector3[] endPoints, float progress, int vinesCount)
    {
        float maxWhipWidth = 0.25f;

        for (int i = 0; i < vinesCount; i++)
        {
            lines[i].widthMultiplier = Mathf.Lerp(0, maxWhipWidth, progress);
            for (int step = 0; step < lines[i].positionCount; step++)
            {
                float t = step / (float)(lines[i].positionCount - 1);
                float growth = t * progress;
                Vector3 currentPoint = Vector3.Lerp(startPoints[i], endPoints[i], growth);
                float arc = Mathf.Sin(growth * Mathf.PI);
                float noise = Mathf.Sin(Time.time * 5f + i + step) * 0.03f;

                lines[i].SetPosition(step, currentPoint + Vector3.up * arc + new Vector3(noise, 0, noise));
            }
        }
    }
}