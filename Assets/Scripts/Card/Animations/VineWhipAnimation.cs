// using System;
// using System.Collections;
// using UnityEngine;
// using DG.Tweening;

// public class VineWhipAnimation : BaseAttackAnimation
// {
//     private GameObject vinePrefab;

//     public VineWhipAnimation()
//     {
//         vinePrefab = Resources.Load<GameObject>("VFX/VineWhip");
//     }

//     public override IEnumerator PlayAnimation(BattleCard3DController attacker, BattleCard3DController defender, AttackManager3D attackManager, Action<Vector3, int> showDamagePopupCallback)
//     {
//         Transform attackerCardTransform = attacker.BattleCardView.CardRoot.transform;
//         Transform defenderCardTransform = defender.BattleCardView.CardRoot.transform;

//         attackerCardTransform.GetPositionAndRotation(out Vector3 startPos, out Quaternion startRot);
//         Transform exitPointTransform = attackerCardTransform.Find("StreamExitPoint");

//         int damage = attackManager.CalculateDamage(attacker, attacker.SelectedAbility.AbilityIndex, defender);

//         Vector3 dirToDefenderCard = (defenderCardTransform.position - attackerCardTransform.position).normalized;
//         Vector3 sideDir = Vector3.Cross(dirToDefenderCard, Vector3.up).normalized;

//         int vinesCount = 3;
//         GameObject[] vineObjects = new GameObject[vinesCount];
//         LineRenderer[] lines = new LineRenderer[vinesCount];
//         Vector3[] targetPoints = new Vector3[vinesCount];

//         for (int i = 0; i < vinesCount; i++)
//         {
//             vineObjects[i] = UnityEngine.Object.Instantiate(vinePrefab);
//             lines[i] = vineObjects[i].GetComponentInChildren<LineRenderer>();
//             lines[i].positionCount = 20;
//             lines[i].widthMultiplier = 0;

//             float sideOffset = (i - 1) * 1.0f;
//             targetPoints[i] = defenderCardTransform.position + (defenderCardTransform.right * sideOffset) + Vector3.up * 0.2f;
//         }

//         float progress = 0f;
//         Sequence whipSequence = DOTween.Sequence();

//         whipSequence.Append(attackerCardTransform.DOLocalRotateQuaternion(Quaternion.Euler(-115f, -90f, 180f), 0.15f).SetEase(Ease.OutExpo));
//         whipSequence.Join(DOTween.To(() => progress, x => progress = x, 1f, 0.7f).SetEase(Ease.InSine));
//         whipSequence.OnUpdate(() =>
//         {
//             for (int i = 0; i < vinesCount; i++)
//             {
//                 float maxWhipWidth = 0.35f;
//                 lines[i].widthMultiplier = Mathf.Lerp(0, maxWhipWidth, progress < 0.8f ? progress * 1.25f : (1 - progress) * 5f);

//                 Vector3 startPoint = exitPointTransform.position;
//                 Vector3 endPoint = targetPoints[i];

//                 float sideBendStrength = (i - 1) * 2.5f;

//                 for (int step = 0; step < lines[i].positionCount; step++)
//                 {
//                     float t = step / (float)(lines[i].positionCount - 1);
//                     Vector3 currentPoint = Vector3.Lerp(startPoint, endPoint, t * progress);
//                     float curve = Mathf.Sin(t * Mathf.PI);
//                     float arcHeight = curve * 3.5f * progress;
//                     Vector3 sideArc = sideDir * (curve * sideBendStrength * progress);
//                     float dropEffect = Mathf.Lerp(1.5f, 0f, progress);

//                     lines[i].SetPosition(step, currentPoint + Vector3.up * (arcHeight + dropEffect * t) + sideArc);
//                 }
//             }
//         });

//         yield return whipSequence.WaitForCompletion();

//         showDamagePopupCallback?.Invoke(defenderCardTransform.position + Vector3.up * 1.5f, damage);
//         attackManager.PerformAttack(attacker, defender, damage);

//         defenderCardTransform.DOShakePosition(0.3f, new Vector3(0.3f, 0.3f, 0), 30);
//         foreach (GameObject vine in vineObjects)
//             UnityEngine.Object.Destroy(vine, 0.3f);

//         yield return attackerCardTransform.DORotateQuaternion(startRot, 0.4f).SetEase(Ease.OutBack).WaitForCompletion();
//     }
// }

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class VineWhipAnimation : BaseAttackAnimation
{
    private GameObject vinePrefab;

    public VineWhipAnimation()
    {
        vinePrefab = Resources.Load<GameObject>("VFX/VineWhip");
    }

    public override IEnumerator PlayAnimation(BattleCardController attacker, BattleCardController defender, AttackManager attackManager, Action<Vector3, int> showDamagePopupCallback)
    {
        Transform attackerCardTransform = attacker.BattleCardView.CardRoot.transform;
        Transform defenderCardTransform = defender.BattleCardView.CardRoot.transform;

        attackerCardTransform.GetPositionAndRotation(out Vector3 cardStartPos, out Quaternion cardStartRot);
        Transform exitPointTransform = attackerCardTransform.Find("AnimationObjects/StreamExitPoint");

        Transform pointsContainer = defenderCardTransform.Find("AnimationObjects/EmptyPoints");
        pointsContainer.gameObject.SetActive(true);

        List<Transform> allPoints = new();
        foreach (Transform child in pointsContainer)
            allPoints.Add(child);

        int damage = attackManager.CalculateDamage(attacker, attacker.SelectedAbility.AbilityIndex, defender);

        int vinesCount = 15;
        GameObject[] vineObjects = new GameObject[vinesCount];
        LineRenderer[] lines = new LineRenderer[vinesCount];
        Vector3[] targetPoints = new Vector3[vinesCount];

        for (int i = 0; i < vinesCount; i++)
        {
            vineObjects[i] = UnityEngine.Object.Instantiate(vinePrefab);
            lines[i] = vineObjects[i].GetComponentInChildren<LineRenderer>();
            lines[i].positionCount = 20;
            lines[i].widthMultiplier = 0;

            targetPoints[i] = allPoints[UnityEngine.Random.Range(0, allPoints.Count)].position;
        }

        float progress = 0f;
        Sequence whipSequence = DOTween.Sequence();

        whipSequence.Append(attackerCardTransform.DOPunchPosition(attackerCardTransform.up + attackerCardTransform.right, 0.3f));
        whipSequence.Join(DOTween.To(() => progress, x => progress = x, 1f, 0.6f).SetEase(Ease.InSine));
        whipSequence.OnUpdate(() =>
        {
            for (int i = 0; i < vinesCount; i++)
            {
                float maxWhipWidth = 0.25f;
                lines[i].widthMultiplier = Mathf.Lerp(0, maxWhipWidth, progress);

                Vector3 startPoint = exitPointTransform.position;
                Vector3 endPoint = targetPoints[i];

                for (int step = 0; step < lines[i].positionCount; step++)
                {
                    float t = step / (float)(lines[i].positionCount - 1);
                    Vector3 currentPoint = Vector3.Lerp(startPoint, endPoint, t * progress);

                    float waveScale = (1 - t) * 1.5f;
                    float freq = 5f;
                    float xOff = Mathf.Sin(t * freq + progress * 10f + i) * waveScale;
                    float zOff = Mathf.Cos(t * freq + progress * 10f + i) * waveScale;

                    float arc = Mathf.Sin(t * Mathf.PI) * 2.0f * (1 - progress);

                    lines[i].SetPosition(step, currentPoint + new Vector3(xOff, arc, zOff));
                }
            }
        });

        yield return whipSequence.WaitForCompletion();

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
                    int attackerSlotIndex = (int)attacker.BattleCardView.CardRoot.transform.parent.GetComponent<CardSlot>().indexSlotInField;
                    int defenderSlotIndex = (int)defender.BattleCardView.CardRoot.transform.parent.GetComponent<CardSlot>().indexSlotInField;
                    networkGameController.RequestApplyDamage(attackerSlotIndex, attacker.SelectedAbility.AbilityIndex, defenderSlotIndex);
                }
            }
        }

        defenderCardTransform.DOShakePosition(0.5f, new Vector3(0.2f, 0.2f, 0), 30);
        defenderCardTransform.DOPunchScale(new Vector3(-0.1f, -0.1f, -0.1f), 0.5f);

        yield return new WaitForSeconds(0.5f);

        pointsContainer.gameObject.SetActive(false);
        foreach (var vine in vineObjects) UnityEngine.Object.Destroy(vine, 0.3f);

        yield return attackerCardTransform.DOMove(cardStartPos, 0.3f).SetEase(Ease.OutQuad).WaitForCompletion();
    }
}