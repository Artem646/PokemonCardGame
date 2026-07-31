using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;

public class PetalDanceAnimation : BaseAttackAnimation
{
    private GameObject vortexPrefab;

    public PetalDanceAnimation()
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
        Vector3 dirToDefender = (defenderCardTransform.position - attackerCardTransform.position).normalized;

        GameObject defenderCardWrapper = new("CardWrapper");
        defenderCardWrapper.transform.SetParent(defenderCardSlotTransform);
        defenderCardWrapper.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        defenderCardWrapper.transform.localScale = Vector3.one;

        defenderCardTransform.SetParent(defenderCardWrapper.transform);
        defenderCardTransform.SetLocalPositionAndRotation(defenderCardLocalPos, defenderCardLocalRot);

        Vector3 upPos = attackerCardTransform.position - dirToDefender * 1.2f + Vector3.up * 0.5f;

        GameObject blizzardObject = UnityEngine.Object.Instantiate(vortexPrefab, attackerCardTransform);
        ParticleSystem blizzard = blizzardObject.GetComponentInChildren<ParticleSystem>();
        blizzard.Stop();

        var shape = blizzard.shape;
        Vector3 finalShapeScale = shape.scale;
        Vector3 startShapeScale = finalShapeScale * 0.7f;
        shape.scale = startShapeScale;

        yield return attackerCardTransform.DOMove(upPos, 0.3f).SetEase(Ease.OutQuad).WaitForCompletion();
        yield return attackerCardTransform.DOLocalRotateQuaternion(Quaternion.Euler(-110f, -90f, 180f), 0.2f).WaitForCompletion();
        blizzard.Play();
        yield return attackerCardTransform.DOShakePosition(0.5f, new Vector3(0.4f, 0.4f, 0), 40).WaitForCompletion();

        Vector3 hitPoint = defenderCardWrapper.transform.position + Vector3.up * 0.5f;

        Sequence moveBlizzardToDefenderSequence = DOTween.Sequence();

        moveBlizzardToDefenderSequence.Append(blizzardObject.transform.DOMove(hitPoint, 0.6f).SetEase(Ease.InQuad));
        moveBlizzardToDefenderSequence.Join(DOTween.To(() => 0f, t =>
        {
            var s = blizzard.shape;
            s.scale = Vector3.Lerp(startShapeScale, finalShapeScale, t);
        }, 1f, 0.8f));

        yield return moveBlizzardToDefenderSequence.WaitForCompletion();

        blizzardObject.transform.SetParent(defenderCardWrapper.transform);

        Sequence attackerReturnSequence = DOTween.Sequence();
        attackerReturnSequence.Append(attackerCardTransform.DOMove(attackerCardStartPos, 0.4f).SetEase(Ease.OutQuad));
        attackerReturnSequence.Join(attackerCardTransform.DORotateQuaternion(attackerCardStartRot, 0.4f).SetEase(Ease.OutQuad));
        attackerReturnSequence.Append(attackerCardTransform.DOLocalMove(Vector3.zero, 0.4f).SetEase(Ease.OutQuad));

        Tween defenderSpinTween = defenderCardWrapper.transform.DOLocalRotate(new Vector3(0, 0, 360f), 0.4f, RotateMode.LocalAxisAdd)
            .SetLoops(-1, LoopType.Incremental).SetEase(Ease.Linear);

        yield return defenderCardWrapper.transform.DOLocalMove(defenderCardLocalPos + Vector3.back * 1.5f, 0.3f).SetEase(Ease.OutQuad).WaitForCompletion();

        int danceHits = 3;
        int damagePerHit = totalDamage / danceHits;

        for (int i = 0; i < danceHits; i++)
        {
            showDamagePopupCallback?.Invoke(defenderCardWrapper.transform.position + Vector3.up * 1.5f, (i == danceHits - 1) ?
                (totalDamage - (damagePerHit * (danceHits - 1))) : damagePerHit);

            defenderCardTransform.DOShakePosition(0.2f, new Vector3(0.3f, 0.3f, 0), 20);
            defenderCardTransform.DOPunchPosition(dirToDefender * 0.35f, 0.2f);
            defenderCardTransform.DOLocalMove(Vector3.zero, 0.2f);

            yield return new WaitForSeconds(0.3f);
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
                    int attackerSlotIndex = (int)attackerCardSlot.indexSlotInField;
                    int defenderSlotIndex = (int)defenderCardSlot.indexSlotInField;
                    networkGameController.RequestApplyDamage(attackerSlotIndex, attacker.SelectedAbility.AbilityIndex, defenderSlotIndex);
                }
            }
        }

        blizzard.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        UnityEngine.Object.Destroy(blizzardObject);

        yield return new WaitForSeconds(0.2f);

        defenderSpinTween.Kill();

        Sequence returnSequence = DOTween.Sequence();
        returnSequence.Append(defenderCardWrapper.transform.DOLocalRotateQuaternion(Quaternion.identity, 0.3f).SetEase(Ease.OutCubic));
        returnSequence.Join(defenderCardWrapper.transform.DOLocalMove(Vector3.zero, 0.3f).SetEase(Ease.OutQuad));
        yield return returnSequence.WaitForCompletion();

        defenderCardTransform.SetParent(defenderCardSlotTransform);
        defenderCardTransform.SetLocalPositionAndRotation(defenderCardLocalPos, defenderCardLocalRot);

        UnityEngine.Object.Destroy(defenderCardWrapper);
    }
}