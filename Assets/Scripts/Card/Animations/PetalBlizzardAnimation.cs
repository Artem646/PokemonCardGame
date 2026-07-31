using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;

public class PetalBlizzardAnimation : BaseAttackAnimation
{
    private GameObject vortexPrefab;

    public PetalBlizzardAnimation()
    {
        vortexPrefab = Resources.Load<GameObject>("VFX/PetalBlizzard");
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

        int totalDamage = attackManager.CalculateDamage(attacker, attacker.SelectedAbility.AbilityIndex, defender);

        Vector3 dirToDefender = (defenderCardTransform.position - attackerCardTransform.position).normalized * 1.5f;

        GameObject defenderCardWrapper = new("CardWrapper");
        defenderCardWrapper.transform.SetParent(defenderCardSlotTransform);
        defenderCardWrapper.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        defenderCardWrapper.transform.localScale = Vector3.one;

        defenderCardTransform.SetParent(defenderCardWrapper.transform);
        defenderCardTransform.SetLocalPositionAndRotation(defenderCardLocalPos, defenderCardLocalRot);

        Vector3 upPos = attackerCardTransform.transform.position - dirToDefender * 1.2f + Vector3.up * 0.5f;

        GameObject blizzardObject = UnityEngine.Object.Instantiate(vortexPrefab, attackerCardTransform);
        ParticleSystem blizzard = blizzardObject.GetComponentInChildren<ParticleSystem>();
        blizzard.Stop();

        var blizzardShape = blizzard.shape;
        Vector3 finalBlizzardShapeScale = blizzardShape.scale;
        Vector3 startBlizzardShapeScale = blizzardShape.scale * 0.7f;
        blizzardShape.scale = startBlizzardShapeScale;

        yield return attackerCardTransform.DOMove(upPos, 0.3f).SetEase(Ease.OutQuad).WaitForCompletion();
        yield return attackerCardTransform.DOLocalRotateQuaternion(Quaternion.Euler(-110f, -90f, 180f), 0.2f).WaitForCompletion();
        blizzard.Play();
        yield return attackerCardTransform.DOShakePosition(0.5f, new Vector3(0.15f, 0.15f, 0), 40, 90, false, true).WaitForCompletion();

        Vector3 hitPoint = defenderCardWrapper.transform.position + Vector3.up * 0.5f;

        Sequence blizzardSequence = DOTween.Sequence();

        blizzardSequence.Append(blizzardObject.transform.DOMove(hitPoint, 1f).SetEase(Ease.InQuad));
        blizzardSequence.Join(DOTween.To(() => 0f, t =>
        {
            var shape = blizzard.shape;
            shape.scale = Vector3.Lerp(startBlizzardShapeScale, finalBlizzardShapeScale, t);
        }, 1f, 0.8f).SetEase(Ease.OutQuad));

        blizzardSequence.OnComplete(() =>
        {
            blizzard.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            UnityEngine.Object.Destroy(blizzardObject, 1.5f);

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
                        int attackerSlotIndex = (int)attackerCardSlot.indexSlotInField;
                        int defenderSlotIndex = (int)defenderCardSlot.indexSlotInField;
                        networkGameController.RequestApplyDamage(attackerSlotIndex, attacker.SelectedAbility.AbilityIndex, defenderSlotIndex);
                    }
                }
            }
            if (!AnimationWithOutDamageMode.Mode) attackManager.PerformAttack(attacker, defender, totalDamage);

            Vector3 defenderFlyPos = defenderCardWrapper.transform.position + dirToDefender * 1.8f;

            defenderCardWrapper.transform.DOMove(defenderFlyPos, 0.3f).SetEase(Ease.OutQuint);
            defenderCardTransform.DOShakePosition(0.6f, new Vector3(0.4f, 0.4f, 0), 30);
        });

        yield return blizzardSequence.WaitForCompletion();

        yield return new WaitForSeconds(0.2f);

        Sequence returnSequence = DOTween.Sequence();
        returnSequence.Append(defenderCardWrapper.transform.DOLocalMove(Vector3.zero, 0.3f).SetEase(Ease.OutQuad));
        returnSequence.Join(attackerCardTransform.DOMove(attackerCardStartPos, 0.3f).SetEase(Ease.OutQuad));
        returnSequence.Join(attackerCardTransform.DORotateQuaternion(attackerCardStartRot, 0.4f).SetEase(Ease.OutQuad));
        yield return returnSequence.WaitForCompletion();

        defenderCardTransform.SetParent(defenderCardSlotTransform);
        defenderCardTransform.SetLocalPositionAndRotation(defenderCardLocalPos, defenderCardLocalRot);

        UnityEngine.Object.Destroy(defenderCardWrapper);
    }
}