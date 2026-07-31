using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;

public class HurricaneAnimation : BaseAttackAnimation
{
    private GameObject hurricanePrefab;

    public HurricaneAnimation()
    {
        hurricanePrefab = Resources.Load<GameObject>("VFX/Hurricane");
    }

    public override IEnumerator PlayAnimation(BattleCardController attacker, BattleCardController defender, AttackManager attackManager, Action<Vector3, int> showDamagePopupCallback)
    {
        Transform attackerCardTransform = attacker.BattleCardView.CardRoot.transform;
        Transform defenderCardTransform = defender.BattleCardView.CardRoot.transform;

        CardSlot attackerCardSlot = attackerCardTransform.parent.GetComponent<CardSlot>();
        CardSlot defenderCardSlot = defenderCardTransform.parent.GetComponent<CardSlot>();

        attackerCardTransform.GetPositionAndRotation(out Vector3 attackerCardStartPos, out Quaternion attackerCardStartRot);

        Transform attackerCardSlotTransform = attackerCardTransform.parent;
        attackerCardTransform.GetLocalPositionAndRotation(out Vector3 attackerCardLocalPos, out Quaternion attackerCardLocalRot);

        Transform defenderCardSlotTransform = defenderCardTransform.parent;
        defenderCardTransform.GetLocalPositionAndRotation(out Vector3 defenderCardLocalPos, out Quaternion defenderCardLocalRot);

        int totalDamage = attackManager.CalculateDamage(attacker, attacker.SelectedAbility.AbilityIndex, defender);
        Vector3 dirToDefender = (defenderCardTransform.position - attackerCardTransform.position).normalized;

        Transform leftWingWrapper = attackerCardTransform.Find("AnimationObjects/Wings/LeftWingWrapper");
        Transform rightWingWrapper = attackerCardTransform.Find("AnimationObjects/Wings/RightWingWrapper");

        Vector3 leftWingWrapperFullScale = leftWingWrapper.localScale;
        Vector3 rightWingWrapperFullScale = rightWingWrapper.localScale;

        GameObject defenderCardWrapper = new("CardWrapper");
        defenderCardWrapper.transform.SetParent(defenderCardSlotTransform);
        defenderCardWrapper.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        defenderCardWrapper.transform.localScale = Vector3.one;

        defenderCardTransform.SetParent(defenderCardWrapper.transform);
        defenderCardTransform.SetLocalPositionAndRotation(defenderCardLocalPos, defenderCardLocalRot);

        Vector3 upPos = attackerCardTransform.position + Vector3.up * 1.5f - dirToDefender * 4f;

        leftWingWrapper.gameObject.SetActive(true);
        rightWingWrapper.gameObject.SetActive(true);

        Sequence unfoldSequence = DOTween.Sequence();

        unfoldSequence.Append(attackerCardTransform.DOMove(upPos, 0.4f).SetEase(Ease.OutBack));
        unfoldSequence.Join(attackerCardTransform.DOLocalRotateQuaternion(Quaternion.Euler(-110f, -90f, 180f), 0.35f));
        unfoldSequence.Join(leftWingWrapper.DOScale(1f, 0.4f).SetEase(Ease.OutBack));
        unfoldSequence.Join(rightWingWrapper.DOScale(1f, 0.4f).SetEase(Ease.OutBack));

        yield return unfoldSequence.WaitForCompletion();

        Sequence flapSequence = DOTween.Sequence();

        flapSequence.Append(leftWingWrapper.DOScaleY(0.3f, 0.15f).SetEase(Ease.InOutSine));
        flapSequence.Join(rightWingWrapper.DOScaleY(0.3f, 0.15f).SetEase(Ease.InOutSine));
        flapSequence.SetLoops(6, LoopType.Yoyo);
        attackerCardTransform.DOShakePosition(0.9f, 0.5f, 30);

        yield return flapSequence.WaitForCompletion();

        GameObject hurricaneObject = UnityEngine.Object.Instantiate(hurricanePrefab, attackerCardTransform);
        ParticleSystem hurricane = hurricaneObject.GetComponentInChildren<ParticleSystem>();
        hurricane.Stop();

        var shape = hurricane.shape;
        Vector3 finalShapeScale = shape.scale;
        Vector3 startShapeScale = finalShapeScale * 0.3f;
        shape.scale = startShapeScale;

        yield return new WaitForSeconds(0.1f);

        hurricane.Play();

        Vector3 hitPoint = defenderCardWrapper.transform.position + Vector3.up * 0.5f;

        Sequence moveVortexToDefenderSequence = DOTween.Sequence();

        moveVortexToDefenderSequence.Append(hurricaneObject.transform.DOMove(hitPoint, 0.6f).SetEase(Ease.InQuad));
        moveVortexToDefenderSequence.Join(DOTween.To(() => 0f, t =>
        {
            var s = hurricane.shape;
            s.scale = Vector3.Lerp(startShapeScale, finalShapeScale, t);
        }, 1f, 0.8f));

        yield return moveVortexToDefenderSequence.WaitForCompletion();

        hurricaneObject.transform.SetParent(defenderCardWrapper.transform);

        Sequence attackerReturnSequence = DOTween.Sequence();
        attackerReturnSequence.Append(attackerCardTransform.DOMove(attackerCardStartPos, 0.4f).SetEase(Ease.OutQuad));
        attackerReturnSequence.Join(attackerCardTransform.DORotateQuaternion(attackerCardStartRot, 0.4f).SetEase(Ease.OutQuad));
        attackerReturnSequence.Append(attackerCardTransform.DOLocalMove(Vector3.zero, 0.4f).SetEase(Ease.OutQuad));
        attackerReturnSequence.Join(leftWingWrapper.DOScale(leftWingWrapperFullScale, 0.2f).SetEase(Ease.InQuad));
        attackerReturnSequence.Join(rightWingWrapper.DOScale(rightWingWrapperFullScale, 0.2f).SetEase(Ease.InQuad));

        Tween defenderSpinTween = defenderCardWrapper.transform.DOLocalRotate(new Vector3(0, 0, 360f), 0.4f, RotateMode.LocalAxisAdd)
            .SetLoops(-1, LoopType.Incremental).SetEase(Ease.Linear);

        yield return defenderCardWrapper.transform.DOLocalMove(defenderCardLocalPos + Vector3.back * 1.5f, 0.3f).SetEase(Ease.OutQuad).WaitForCompletion();

        int hits = 3;
        int damagePerHit = totalDamage / hits;

        for (int i = 0; i < hits; i++)
        {
            showDamagePopupCallback?.Invoke(defenderCardWrapper.transform.position + Vector3.up * 1.5f,
                (i == hits - 1) ? (totalDamage - (damagePerHit * (hits - 1))) : damagePerHit);

            defenderCardTransform.DOShakePosition(0.2f, new Vector3(0.3f, 0.3f, 0), 25);
            defenderCardTransform.DOPunchPosition(dirToDefender * 0.35f, 0.2f);
            defenderCardTransform.DOLocalMove(Vector3.zero, 0.2f);

            yield return new WaitForSeconds(0.4f);
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

        hurricane.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        UnityEngine.Object.Destroy(hurricaneObject);

        yield return new WaitForSeconds(0.2f);

        defenderSpinTween.Kill();

        leftWingWrapper.gameObject.SetActive(false);
        rightWingWrapper.gameObject.SetActive(false);

        Sequence returnSequence = DOTween.Sequence();
        returnSequence.Append(defenderCardWrapper.transform.DOLocalRotateQuaternion(Quaternion.identity, 0.3f).SetEase(Ease.OutCubic));
        returnSequence.Join(defenderCardWrapper.transform.DOLocalMove(Vector3.zero, 0.3f).SetEase(Ease.OutQuad));
        yield return returnSequence.WaitForCompletion();

        defenderCardTransform.SetParent(defenderCardSlotTransform);
        defenderCardTransform.SetLocalPositionAndRotation(defenderCardLocalPos, defenderCardLocalRot);

        UnityEngine.Object.Destroy(defenderCardWrapper);
    }
}