using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;

public class WaterGunAnimation : BaseAttackAnimation
{
    private GameObject waterStreamPrefab;

    public WaterGunAnimation()
    {
        waterStreamPrefab = Resources.Load<GameObject>("VFX/WaterGunStream");
    }

    public override IEnumerator PlayAnimation(BattleCardController attacker, BattleCardController defender, AttackManager attackManager, Action<Vector3, int> showDamagePopupCallback)
    {
        Transform attackerCardTransform = attacker.BattleCardView.CardRoot.transform;
        Transform defenderCardTransform = defender.BattleCardView.CardRoot.transform;

        CardSlot attackerCardSlot = attackerCardTransform.parent.GetComponent<CardSlot>();
        CardSlot defenderCardSlot = defenderCardTransform.parent.GetComponent<CardSlot>();

        attackerCardTransform.GetPositionAndRotation(out Vector3 attackerCardStartPos, out Quaternion attackerCardStartRot);

        Transform defenderCardSlotTransform = defenderCardTransform.parent;
        defenderCardTransform.GetLocalPositionAndRotation(out Vector3 defenderCardLocalPos, out Quaternion defenderCardLocalRot);

        Transform streamStartPointTransform = attackerCardTransform.Find("AnimationObjects/StreamExitPoint");
        Vector3 streamEndPoint = defenderCardTransform.position + Vector3.up * 0.4f;

        GameObject defenderCardWrapper = new("CardWrapper");
        defenderCardWrapper.transform.SetParent(defenderCardSlotTransform);
        defenderCardWrapper.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        defenderCardWrapper.transform.localScale = Vector3.one;

        defenderCardTransform.SetParent(defenderCardWrapper.transform);
        defenderCardTransform.SetLocalPositionAndRotation(defenderCardLocalPos, defenderCardLocalRot);

        yield return attackerCardTransform.DOLocalRotateQuaternion(Quaternion.Euler(-70f, -90f, 180f), 0.3f).SetEase(Ease.OutQuad).WaitForCompletion();

        GameObject waterStreamObject = UnityEngine.Object.Instantiate(waterStreamPrefab);
        LineRenderer waterStreamLine = waterStreamObject.GetComponentInChildren<LineRenderer>();
        ParticleSystem waterStreamSplash = waterStreamObject.GetComponentInChildren<ParticleSystem>();

        waterStreamLine.widthMultiplier = 0;
        waterStreamLine.SetPosition(0, streamStartPointTransform.position);
        waterStreamLine.SetPosition(1, streamStartPointTransform.position);

        float growthFactor = 0f;

        Sequence shotSequence = DOTween.Sequence();

        shotSequence.Append(attackerCardTransform.DOLocalRotateQuaternion(Quaternion.Euler(-110f, -90f, 180f), 0.15f).SetEase(Ease.OutExpo));
        shotSequence.Join(DOTween.To(() => growthFactor, x => growthFactor = x, 1f, 0.35f).SetEase(Ease.Linear));
        shotSequence.Join(DOTween.To(() => waterStreamLine.widthMultiplier, x => waterStreamLine.widthMultiplier = x, 1.2f, 0.35f));
        shotSequence.Join(waterStreamLine.material.DOOffset(new Vector2(-2f, 0), "_MainTex", 0.5f).SetEase(Ease.Linear));

        shotSequence.OnUpdate(() =>
        {
            Vector3 currentStreamStartPoint = streamStartPointTransform.position;
            waterStreamLine.SetPosition(0, currentStreamStartPoint);

            Vector3 currentStreamEndPoint = Vector3.Lerp(currentStreamStartPoint, streamEndPoint, growthFactor);
            waterStreamLine.SetPosition(1, currentStreamEndPoint);

            if (growthFactor >= 0.95f && !waterStreamSplash.isPlaying)
            {
                waterStreamSplash.transform.position = streamEndPoint;
                waterStreamSplash.transform.LookAt(currentStreamStartPoint);
                waterStreamSplash.Play();
            }
        });

        Vector3 pushWorldDir = (defenderCardTransform.position - attackerCardTransform.position).normalized;

        shotSequence.Insert(0.25f, defenderCardWrapper.transform.DOMove(defenderCardWrapper.transform.position + pushWorldDir * 1.4f, 0.2f).SetEase(Ease.OutQuad));
        shotSequence.Insert(0.25f, defenderCardTransform.DOShakePosition(0.4f, new Vector3(0.5f, 0.5f, 0), 20, 90, false, true));

        yield return shotSequence.WaitForCompletion();

        int damage = attackManager.CalculateDamage(attacker, attacker.SelectedAbility.AbilityIndex, defender);
        showDamagePopupCallback?.Invoke(defenderCardTransform.position, damage);

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

        waterStreamSplash.Stop();

        DOTween.To(() => waterStreamLine.widthMultiplier, x => waterStreamLine.widthMultiplier = x, 0f, 0.2f)
            .OnComplete(() => UnityEngine.Object.Destroy(waterStreamObject, 0.5f));

        Sequence returnSequence = DOTween.Sequence();
        returnSequence.Append(attackerCardTransform.DORotateQuaternion(attackerCardStartRot, 0.4f).SetEase(Ease.OutBack));
        returnSequence.Join(defenderCardWrapper.transform.DOLocalMove(Vector3.zero, 0.3f).SetEase(Ease.OutQuad));
        yield return returnSequence.WaitForCompletion();

        defenderCardTransform.SetParent(defenderCardSlotTransform);
        defenderCardTransform.SetLocalPositionAndRotation(defenderCardLocalPos, defenderCardLocalRot);

        UnityEngine.Object.Destroy(defenderCardWrapper);
    }
}