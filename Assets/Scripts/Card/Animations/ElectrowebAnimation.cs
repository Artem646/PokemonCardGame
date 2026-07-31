//// Просто летят лучи в центр карты

// using System;
// using System.Collections;
// using UnityEngine;
// using DG.Tweening;

// public class ElectrowebAnimation : BaseAttackAnimation
// {
//     private GameObject strandPrefab;

//     public ElectrowebAnimation()
//     {
//         strandPrefab = Resources.Load<GameObject>("VFX/ElectrowebStrand");
//     }

//     public override IEnumerator PlayAnimation(BattleCard3DController attacker, BattleCard3DController defender, AttackManager3D attackManager, Action<Vector3, int> showDamagePopupCallback)
//     {
//         Transform attackerCardTransform = attacker.BattleCardView.CardRoot.transform;
//         Transform defenderCardTransform = defender.BattleCardView.CardRoot.transform;

//         attackerCardTransform.GetPositionAndRotation(out Vector3 startPos, out Quaternion startRot);
//         Transform exitPointTransform = attackerCardTransform.Find("StreamExitPoint");

//         int damage = attackManager.CalculateDamage(attacker, attacker.SelectedAbility.AbilityIndex, defender);

//         yield return attackerCardTransform.DOMove(startPos + Vector3.up * 0.4f, 0.25f).SetEase(Ease.OutQuad).WaitForCompletion();
//         attackerCardTransform.DOShakePosition(0.5f, new Vector3(0.1f, 0.1f, 0), 50);
//         yield return new WaitForSeconds(0.4f);

//         Vector3 dirToDefenderCard = (defenderCardTransform.position - exitPointTransform.position).normalized;
//         Vector3 sideDir = Vector3.Cross(dirToDefenderCard, Vector3.up).normalized;
//         Vector3 upDir = Vector3.Cross(sideDir, dirToDefenderCard).normalized;

//         int strandsCount = 10;
//         GameObject[] strandObjects = new GameObject[strandsCount];
//         LineRenderer[] lines = new LineRenderer[strandsCount];

//         for (int i = 0; i < strandsCount; i++)
//         {
//             strandObjects[i] = UnityEngine.Object.Instantiate(strandPrefab);
//             lines[i] = strandObjects[i].GetComponentInChildren<LineRenderer>();
//             lines[i].positionCount = 20;
//             lines[i].widthMultiplier = 0;
//             lines[i].useWorldSpace = true;
//         }

//         float progress = 0f;
//         Sequence webSequence = DOTween.Sequence();

//         webSequence.Append(attackerCardTransform.DOPunchPosition(attackerCardTransform.forward * 0.5f, 0.2f));
//         webSequence.Join(DOTween.To(() => progress, x => progress = x, 1f, 0.4f).SetEase(Ease.OutQuad));
//         webSequence.OnUpdate(() =>
//         {
//             for (int i = 0; i < strandsCount; i++)
//             {
//                 float maxStrandWidth = 0.5f;
//                 lines[i].widthMultiplier = Mathf.Lerp(0, maxStrandWidth, progress < 0.2f ? progress * 5 : 1f);

//                 Vector3 startPoint = exitPointTransform.position;
//                 Vector3 endPoint = defenderCardTransform.position + Vector3.up * 0.3f;

//                 float angle = i / (float)strandsCount * Mathf.PI * 2f;
//                 Vector3 offsetDir = (sideDir * Mathf.Cos(angle) + upDir * Mathf.Sin(angle)).normalized;

//                 for (int step = 0; step < lines[i].positionCount; step++)
//                 {
//                     float t = step / (float)(lines[i].positionCount - 1);
//                     Vector3 currentBasePoint = Vector3.Lerp(startPoint, endPoint, t * progress);
//                     float webExpansion = Mathf.Sin(t * Mathf.PI) * 2.5f * progress;
//                     Vector3 jitter = UnityEngine.Random.insideUnitSphere * 0.15f;

//                     lines[i].SetPosition(step, currentBasePoint + (offsetDir * webExpansion) + jitter);
//                 }
//             }
//         });

//         yield return webSequence.WaitForCompletion();

//         showDamagePopupCallback?.Invoke(defenderCardTransform.position + Vector3.up * 1.5f, damage);
//         attackManager.PerformAttack(attacker, defender, damage);

//         defenderCardTransform.DOShakePosition(0.5f, new Vector3(0.4f, 0.4f, 0), 40);
//         defenderCardTransform.DOShakeRotation(0.5f, 20f, 50);

//         yield return new WaitForSeconds(0.3f);

//         foreach (GameObject strand in strandObjects)
//             UnityEngine.Object.Destroy(strand, 0.25f);

//         yield return attackerCardTransform.DOMove(startPos, 0.3f).SetEase(Ease.OutQuad);
//         yield return attackerCardTransform.DORotateQuaternion(startRot, 0.3f).SetEase(Ease.OutBack).WaitForCompletion();
//     }
// }


//// Летят лучи в места по всей карте


// using System;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using DG.Tweening;

// public class ElectrowebAnimation : BaseAttackAnimation
// {
//     private GameObject strandPrefab;

//     public ElectrowebAnimation()
//     {
//         strandPrefab = Resources.Load<GameObject>("VFX/ElectrowebStrand");
//     }

//     public override IEnumerator PlayAnimation(BattleCard3DController attacker, BattleCard3DController defender, AttackManager3D attackManager, Action<Vector3, int> showDamagePopupCallback)
//     {
//         Transform attackerCardTransform = attacker.BattleCardView.CardRoot.transform;
//         Transform defenderCardTransform = defender.BattleCardView.CardRoot.transform;

//         attackerCardTransform.GetPositionAndRotation(out Vector3 cardStartPos, out Quaternion cardStartRot);
//         Transform exitPointTransform = attackerCardTransform.Find("StreamExitPoint");

//         Transform targetPlate = defenderCardTransform.Find("ElectrowebTargetPlate5");
//         targetPlate.gameObject.SetActive(true);

//         List<Transform> targetPoints = new();
//         foreach (Transform child in targetPlate)
//             targetPoints.Add(child);

//         int totalDamage = attackManager.CalculateDamage(attacker, attacker.SelectedAbility.AbilityIndex, defender);

//         yield return attackerCardTransform.DOMove(cardStartPos + Vector3.up * 0.5f, 0.3f).SetEase(Ease.OutQuad).WaitForCompletion();
//         attackerCardTransform.DOShakePosition(0.4f, new Vector3(0.1f, 0.1f, 0), 50);
//         yield return new WaitForSeconds(0.4f);

//         Vector3 dirToEnemy = (defenderCardTransform.position - exitPointTransform.position).normalized;
//         Vector3 sideDir = Vector3.Cross(dirToEnemy, Vector3.up).normalized;

//         int strandsCount = targetPoints.Count;
//         GameObject[] strandObjects = new GameObject[strandsCount];
//         LineRenderer[] lines = new LineRenderer[strandsCount];

//         for (int i = 0; i < strandsCount; i++)
//         {
//             strandObjects[i] = UnityEngine.Object.Instantiate(strandPrefab);
//             lines[i] = strandObjects[i].GetComponentInChildren<LineRenderer>();
//             lines[i].positionCount = 20;
//             lines[i].widthMultiplier = 0;
//         }

//         float progress = 0f;
//         Sequence webSequence = DOTween.Sequence();

//         webSequence.Append(attackerCardTransform.DOPunchPosition(attackerCardTransform.forward * 0.5f, 0.2f));
//         webSequence.Join(DOTween.To(() => progress, x => progress = x, 1f, 0.45f).SetEase(Ease.OutCubic));
//         webSequence.OnUpdate(() =>
//         {
//             for (int i = 0; i < strandsCount; i++)
//             {
//                 // lines[i].widthMultiplier = Mathf.Lerp(0, 0.5f, progress < 0.1f ? progress * 10 : (1.1f - progress));
//                 lines[i].widthMultiplier = Mathf.Lerp(0, 0.5f, progress < 0.1f ? progress * 10 : 1f);

//                 Vector3 startPoint = exitPointTransform.position;
//                 Vector3 endPoint = targetPoints[i].position;

//                 float angle = i / (float)strandsCount * Mathf.PI * 2f;
//                 Vector3 expansionDir = (sideDir * Mathf.Cos(angle) + Vector3.up * Mathf.Abs(Mathf.Sin(angle))).normalized;

//                 for (int step = 0; step < lines[i].positionCount; step++)
//                 {
//                     float t = step / (float)(lines[i].positionCount - 1);
//                     Vector3 currentBase = Vector3.Lerp(startPoint, endPoint, t * progress);
//                     float webExpansion = Mathf.Sin(t * Mathf.PI) * 2.0f * (1 - progress);
//                     Vector3 jitter = UnityEngine.Random.insideUnitSphere * 0.12f;

//                     lines[i].SetPosition(step, currentBase + (expansionDir * webExpansion) + jitter);
//                 }
//             }
//         });

//         yield return webSequence.WaitForCompletion();

//         showDamagePopupCallback?.Invoke(defenderCardTransform.position + Vector3.up * 1.5f, totalDamage);
//         attackManager.PerformAttack(attacker, defender, totalDamage);

//         defenderCardTransform.DOShakePosition(0.5f, new Vector3(0.5f, 0.5f, 0), 60);

//         yield return new WaitForSeconds(0.4f);

//         targetPlate.gameObject.SetActive(false);

//         foreach (GameObject strand in strandObjects)
//              UnityEngine.Object.Destroy(strand);

//         yield return attackerCardTransform.DOMove(cardStartPos, 0.3f).SetEase(Ease.OutQuad);
//         yield return attackerCardTransform.DORotateQuaternion(cardStartRot, 0.3f).SetEase(Ease.OutBack).WaitForCompletion();
//     }
// }


//// Сначала пучок вылетает, потом появляется сетка на карте


// using System;
// using System.Collections;
// using System.Collections.Generic;
// using System.Linq;
// using UnityEngine;
// using DG.Tweening;

// public class ElectrowebAnimation : BaseAttackAnimation
// {
//     private GameObject strandPrefab;

//     public ElectrowebAnimation()
//     {
//         strandPrefab = Resources.Load<GameObject>("VFX/ElectrowebStrand");
//     }

//     public override IEnumerator PlayAnimation(BattleCard3DController attacker, BattleCard3DController defender, AttackManager3D attackManager, Action<Vector3, int> showDamagePopupCallback)
//     {
//         Transform attackerCardTransform = attacker.BattleCardView.CardRoot.transform;
//         Transform defenderCardTransform = defender.BattleCardView.CardRoot.transform;

//         attackerCardTransform.GetPositionAndRotation(out Vector3 cardStartPos, out Quaternion cardStartRot);
//         Transform exitPointTransform = attackerCardTransform.Find("StreamExitPoint");

//         Transform targetPlate = defenderCardTransform.Find("ElectrowebTargetPlate");
//         targetPlate.gameObject.SetActive(true);
//         Vector3 plateCenter = targetPlate.position;
//         List<List<Transform>> gridGroups = new();
//         Transform rows = targetPlate.Find("Rows");
//         Transform cols = targetPlate.Find("Cols");
//         foreach (Transform row in rows) gridGroups.Add(GetChildren(row));
//         foreach (Transform col in cols) gridGroups.Add(GetChildren(col));

//         int totalDamage = attackManager.CalculateDamage(attacker, attacker.SelectedAbility.AbilityIndex, defender);

//         yield return attackerCardTransform.DOMove(cardStartPos + Vector3.up * 0.4f, 0.3f).SetEase(Ease.OutQuad).WaitForCompletion();
//         attackerCardTransform.DOShakePosition(0.4f, new Vector3(0.1f, 0.1f, 0), 50);
//         yield return attackerCardTransform.DOLocalRotateQuaternion(Quaternion.Euler(-60f, -90f, 180f), 0.3f).SetEase(Ease.OutBack).WaitForCompletion();

//         Vector3 dirToEnemy = (defenderCardTransform.position - exitPointTransform.position).normalized;
//         Vector3 sideDir = Vector3.Cross(dirToEnemy, Vector3.up).normalized;

//         int groupsCount = gridGroups.Count;
//         GameObject[] strandObjects = new GameObject[groupsCount];
//         LineRenderer[] lines = new LineRenderer[groupsCount];

//         for (int i = 0; i < groupsCount; i++)
//         {
//             strandObjects[i] = UnityEngine.Object.Instantiate(strandPrefab);
//             lines[i] = strandObjects[i].GetComponentInChildren<LineRenderer>();
//             lines[i].positionCount = gridGroups[i].Count;
//             lines[i].widthMultiplier = 0;
//         }

//         float progress = 0f;
//         Sequence webSequence = DOTween.Sequence();

//         webSequence.Append(attackerCardTransform.DOPunchPosition(attackerCardTransform.forward * 0.5f, 0.2f));
//         webSequence.Join(DOTween.To(() => progress, x => progress = x, 1f, 0.6f).SetEase(Ease.OutCubic));
//         webSequence.OnUpdate(() =>
//         {
//             for (int i = 0; i < groupsCount; i++)
//             {
//                 lines[i].widthMultiplier = Mathf.Lerp(0, 0.5f, progress < 0.1f ? progress * 10 : 1f);

//                 Vector3 startPoint = exitPointTransform.position;

//                 float angle = i / (float)groupsCount * Mathf.PI * 2f;
//                 Vector3 expansionDir = (sideDir * Mathf.Cos(angle) + Vector3.up * Mathf.Abs(Mathf.Sin(angle))).normalized;

//                 for (int p = 0; p < gridGroups[i].Count; p++)
//                 {
//                     Vector3 finalPoint = gridGroups[i][p].position;
//                     Vector3 currentPos;

//                     if (progress < 0.4f)
//                     {
//                         float travelT = progress / 0.4f;
//                         currentPos = Vector3.Lerp(startPoint, plateCenter, travelT);

//                         float wave = Mathf.Sin(travelT * Mathf.PI) * 1.5f;
//                         currentPos += expansionDir * wave;
//                     }
//                     else
//                     {
//                         float spreadT = (progress - 0.4f) / 0.6f;
//                         currentPos = Vector3.Lerp(plateCenter, finalPoint, spreadT);
//                     }

//                     Vector3 jitter = UnityEngine.Random.insideUnitSphere * 0.07f;

//                     lines[i].SetPosition(p, currentPos + jitter);
//                 }
//             }
//         });

//         yield return webSequence.WaitForCompletion();

//         showDamagePopupCallback?.Invoke(defenderCardTransform.position + Vector3.up * 1.5f, totalDamage);
//         attackManager.PerformAttack(attacker, defender, totalDamage);

//         defenderCardTransform.DOShakePosition(0.5f, new Vector3(0.5f, 0.5f, 0), 50);

//         yield return new WaitForSeconds(0.4f);

//         targetPlate.gameObject.SetActive(false);
//         foreach (GameObject strand in strandObjects)
//             UnityEngine.Object.Destroy(strand);

//         yield return attackerCardTransform.DOMove(cardStartPos, 0.3f).SetEase(Ease.OutQuad);
//         yield return attackerCardTransform.DORotateQuaternion(cardStartRot, 0.3f).SetEase(Ease.OutBack).WaitForCompletion();
//     }

//     private List<Transform> GetChildren(Transform parent)
//     {
//         List<Transform> children = new();
//         foreach (Transform child in parent) children.Add(child);
//         return children;
//     }
// }


//// Выстреливается сетка и замирает, но карта трясётся 


// using System;
// using System.Collections;
// using System.Collections.Generic;
// using System.Linq;
// using UnityEngine;
// using DG.Tweening;

// public class ElectrowebAnimation : BaseAttackAnimation
// {
//     private GameObject strandPrefab;

//     public ElectrowebAnimation()
//     {
//         strandPrefab = Resources.Load<GameObject>("VFX/ElectrowebStrand");
//     }

//     public override IEnumerator PlayAnimation(BattleCard3DController attacker, BattleCard3DController defender, AttackManager3D attackManager, Action<Vector3, int> showDamagePopupCallback)
//     {
//         Transform attackerCardTransform = attacker.BattleCardView.CardRoot.transform;
//         Transform defenderCardTransform = defender.BattleCardView.CardRoot.transform;

//         attackerCardTransform.GetPositionAndRotation(out Vector3 cardStartPos, out Quaternion cardStartRot);
//         Transform exitPointTransform = attackerCardTransform.Find("StreamExitPoint");

//         Transform targetPlate = defenderCardTransform.Find("ElectrowebTargetPlate");
//         targetPlate.gameObject.SetActive(true);

//         List<List<Transform>> gridGroups = new();
//         Transform rows = targetPlate.Find("Rows");
//         Transform cols = targetPlate.Find("Cols");
//         foreach (Transform row in rows) gridGroups.Add(GetChildren(row));
//         foreach (Transform col in cols) gridGroups.Add(GetChildren(col));

//         int damage = attackManager.CalculateDamage(attacker, attacker.SelectedAbility.AbilityIndex, defender);

//         yield return attackerCardTransform.DOMove(cardStartPos + Vector3.up * 0.4f, 0.3f).SetEase(Ease.OutQuad).WaitForCompletion();
//         attackerCardTransform.DOShakePosition(0.4f, new Vector3(0.1f, 0.1f, 0), 50);
//         yield return attackerCardTransform.DOLocalRotateQuaternion(Quaternion.Euler(-60f, -90f, 180f), 0.3f).SetEase(Ease.OutBack).WaitForCompletion();

//         int groupsCount = gridGroups.Count;
//         GameObject[] strandObjects = new GameObject[groupsCount];
//         LineRenderer[] lines = new LineRenderer[groupsCount];

//         for (int i = 0; i < groupsCount; i++)
//         {
//             strandObjects[i] = UnityEngine.Object.Instantiate(strandPrefab);
//             lines[i] = strandObjects[i].GetComponentInChildren<LineRenderer>();
//             lines[i].positionCount = gridGroups[i].Count;
//             lines[i].widthMultiplier = 0;
//         }

//         float progress = 0f;
//         Sequence webSequence = DOTween.Sequence();

//         webSequence.Append(attackerCardTransform.DOPunchPosition(attackerCardTransform.forward * 0.5f, 0.2f));
//         webSequence.Join(DOTween.To(() => progress, x => progress = x, 1f, 1f).SetEase(Ease.OutQuad));
//         webSequence.OnUpdate(() =>
//         {
//             for (int i = 0; i < groupsCount; i++)
//             {
//                 lines[i].widthMultiplier = Mathf.Lerp(0, 0.45f, progress < 0.2f ? progress * 5 : 1f);
//                 Vector3 startPoint = exitPointTransform.position;

//                 for (int p = 0; p < gridGroups[i].Count; p++)
//                 {
//                     Vector3 finalPoint = gridGroups[i][p].position;
//                     Vector3 currentPos = Vector3.Lerp(startPoint, finalPoint, progress);

//                     float arcHeight = Mathf.Sin(progress * Mathf.PI) * 2.0f;
//                     currentPos += Vector3.up * arcHeight;

//                     Vector3 jitter = UnityEngine.Random.insideUnitSphere * 0.08f;

//                     lines[i].SetPosition(p, currentPos + jitter);
//                 }
//             }
//         });

//         yield return webSequence.WaitForCompletion();

//         showDamagePopupCallback?.Invoke(defenderCardTransform.position + Vector3.up * 1.5f, damage);
//         attackManager.PerformAttack(attacker, defender, damage);

//         yield return new WaitForSeconds(0.4f);

//         defenderCardTransform.DOShakePosition(0.6f, new Vector3(0.4f, 0.4f, 0), 50);

//         yield return new WaitForSeconds(0.4f);

//         targetPlate.gameObject.SetActive(false);
//         foreach (GameObject strand in strandObjects)
//             UnityEngine.Object.Destroy(strand);

//         yield return attackerCardTransform.DOMove(cardStartPos, 0.3f).SetEase(Ease.OutQuad);
//         yield return attackerCardTransform.DORotateQuaternion(cardStartRot, 0.3f).SetEase(Ease.OutBack).WaitForCompletion();
//     }

//     private List<Transform> GetChildren(Transform parent)
//     {
//         List<Transform> children = new();
//         foreach (Transform child in parent) children.Add(child);
//         return children;
//     }
// }


//// Выстреливается сетка, карта трясется сетка вместе с ней,как будто прилипает.  


using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;

public class ElectrowebAnimation : BaseAttackAnimation
{
    private GameObject strandPrefab;

    public ElectrowebAnimation()
    {
        strandPrefab = Resources.Load<GameObject>("VFX/Electroweb");
    }

    public override IEnumerator PlayAnimation(BattleCardController attacker, BattleCardController defender, AttackManager attackManager, Action<Vector3, int> showDamagePopupCallback)
    {
        Transform attackerCardTransform = attacker.BattleCardView.CardRoot.transform;
        Transform defenderCardTransform = defender.BattleCardView.CardRoot.transform;

        attackerCardTransform.GetPositionAndRotation(out Vector3 cardStartPos, out Quaternion cardStartRot);
        Transform exitPointTransform = attackerCardTransform.Find("AnimationObjects/StreamExitPoint");

        Transform groupedWebPointsContainer = defenderCardTransform.Find("AnimationObjects/GroupedWebPoints");
        groupedWebPointsContainer.gameObject.SetActive(true);

        List<List<Transform>> gridGroups = new();
        Transform rows = groupedWebPointsContainer.Find("Rows");
        Transform cols = groupedWebPointsContainer.Find("Cols");
        foreach (Transform row in rows) gridGroups.Add(GetChildren(row));
        foreach (Transform col in cols) gridGroups.Add(GetChildren(col));

        int damage = attackManager.CalculateDamage(attacker, attacker.SelectedAbility.AbilityIndex, defender);

        yield return attackerCardTransform.DOMove(cardStartPos + Vector3.up * 0.4f, 0.3f).SetEase(Ease.OutQuad).WaitForCompletion();
        attackerCardTransform.DOShakePosition(0.4f, new Vector3(0.1f, 0.1f, 0), 50);
        yield return attackerCardTransform.DOLocalRotateQuaternion(Quaternion.Euler(-60f, -90f, 180f), 0.3f).SetEase(Ease.OutBack).WaitForCompletion();

        int groupsCount = gridGroups.Count;
        GameObject[] strandObjects = new GameObject[groupsCount];
        LineRenderer[] lines = new LineRenderer[groupsCount];

        for (int i = 0; i < groupsCount; i++)
        {
            strandObjects[i] = UnityEngine.Object.Instantiate(strandPrefab);
            lines[i] = strandObjects[i].GetComponentInChildren<LineRenderer>();
            lines[i].positionCount = gridGroups[i].Count;
            lines[i].widthMultiplier = 0;
        }

        float progress = 0f;
        float heightAboveCard = 0.06f;

        Sequence webSequence = DOTween.Sequence();

        webSequence.Append(attackerCardTransform.DOPunchPosition(attackerCardTransform.forward * 0.5f, 0.2f));
        webSequence.Join(DOTween.To(() => progress, x => progress = x, 1f, 0.6f).SetEase(Ease.OutQuad));
        webSequence.OnUpdate(() =>
        {
            UpdateNetPositions(lines, gridGroups, exitPointTransform.position, progress, heightAboveCard);
        });

        yield return webSequence.WaitForCompletion();

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

        defenderCardTransform.DOShakePosition(0.6f, new Vector3(0.4f, 0.4f, 0), 50);

        float timer = 0f;
        while (timer < 0.6f)
        {
            timer += Time.deltaTime;
            UpdateNetPositions(lines, gridGroups, exitPointTransform.position, 1f, heightAboveCard);
            yield return null;
        }

        groupedWebPointsContainer.gameObject.SetActive(false);
        foreach (GameObject strand in strandObjects)
            UnityEngine.Object.Destroy(strand, 0.25f);

        yield return attackerCardTransform.DOMove(cardStartPos, 0.3f).SetEase(Ease.OutQuad);
        yield return attackerCardTransform.DORotateQuaternion(cardStartRot, 0.3f).SetEase(Ease.OutBack).WaitForCompletion();
    }

    private void UpdateNetPositions(LineRenderer[] lines, List<List<Transform>> gridGroups, Vector3 startPoint, float progress, float heightOffset)
    {
        for (int i = 0; i < lines.Length; i++)
        {
            lines[i].widthMultiplier = Mathf.Lerp(0, 0.45f, progress < 0.2f ? progress * 5 : 1f);

            for (int p = 0; p < gridGroups[i].Count; p++)
            {
                Vector3 finalPoint = gridGroups[i][p].position + Vector3.up * heightOffset;
                Vector3 currentPos = Vector3.Lerp(startPoint, finalPoint, progress);

                float arcHeight = Mathf.Sin(progress * Mathf.PI) * 2f;
                currentPos += Vector3.up * arcHeight;

                Vector3 jitter = UnityEngine.Random.insideUnitSphere * 0.08f;

                lines[i].SetPosition(p, currentPos + jitter);
            }
        }
    }

    private List<Transform> GetChildren(Transform parent)
    {
        List<Transform> children = new();
        foreach (Transform child in parent) children.Add(child);
        return children;
    }
}