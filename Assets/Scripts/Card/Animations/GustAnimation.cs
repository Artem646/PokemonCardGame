using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;

public class GustAnimation : BaseAttackAnimation
{
    private GameObject gustPrefab;

    public GustAnimation()
    {
        gustPrefab = Resources.Load<GameObject>("VFX/Gust");
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

        int damage = attackManager.CalculateDamage(attacker, attacker.SelectedAbility.AbilityIndex, defender);
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

        unfoldSequence.Append(attackerCardTransform.DOMove(upPos, 0.4f).SetEase(Ease.OutQuad));
        unfoldSequence.Join(attackerCardTransform.DOLocalRotateQuaternion(Quaternion.Euler(-110f, -90f, 180f), 0.35f));
        unfoldSequence.Join(leftWingWrapper.DOScale(new Vector3(1f, 1.5f, 1f), 0.3f).SetEase(Ease.OutQuad));
        unfoldSequence.Join(rightWingWrapper.DOScale(new Vector3(1f, 1.5f, 1f), 0.3f).SetEase(Ease.OutQuad));

        yield return unfoldSequence.WaitForCompletion();

        Sequence flapSequence = DOTween.Sequence();

        flapSequence.Append(leftWingWrapper.DOScaleY(0.3f, 0.05f).SetEase(Ease.InOutSine));
        flapSequence.Join(rightWingWrapper.DOScaleY(0.3f, 0.05f).SetEase(Ease.InOutSine));
        attackerCardTransform.DOShakePosition(0.05f, 0.5f, 30);

        yield return flapSequence.WaitForCompletion();

        GameObject gustObject = UnityEngine.Object.Instantiate(gustPrefab, attackerCardTransform);
        gustObject.transform.LookAt(defenderCardTransform.position);
        ParticleSystem gustParticles = gustObject.GetComponentInChildren<ParticleSystem>();
        gustParticles.Play();

        Sequence attackerReturnSequence = DOTween.Sequence();
        attackerReturnSequence.Append(attackerCardTransform.DOMove(attackerCardStartPos, 0.4f).SetEase(Ease.OutQuad));
        attackerReturnSequence.Join(attackerCardTransform.DORotateQuaternion(attackerCardStartRot, 0.4f).SetEase(Ease.OutQuad));
        attackerReturnSequence.Append(attackerCardTransform.DOLocalMove(Vector3.zero, 0.4f).SetEase(Ease.OutQuad));
        attackerReturnSequence.Join(leftWingWrapper.DOScale(leftWingWrapperFullScale, 0.2f).SetEase(Ease.InQuad));
        attackerReturnSequence.Join(rightWingWrapper.DOScale(rightWingWrapperFullScale, 0.2f).SetEase(Ease.InQuad));

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
                    int attackerSlotIndex = (int)attackerCardSlot.indexSlotInField;
                    int defenderSlotIndex = (int)defenderCardSlot.indexSlotInField;
                    networkGameController.RequestApplyDamage(attackerSlotIndex, attacker.SelectedAbility.AbilityIndex, defenderSlotIndex);
                }
            }
        }

        Vector3 defenderFlyPos = defenderCardWrapper.transform.position + dirToDefender * 1.8f;

        Sequence collisionSequence = DOTween.Sequence();

        collisionSequence.Append(defenderCardWrapper.transform.DOMove(defenderFlyPos, 0.3f).SetEase(Ease.OutQuint));
        collisionSequence.Join(defenderCardTransform.DOShakePosition(0.5f, new Vector3(1, 1, 0), 40));

        yield return collisionSequence.WaitForCompletion();

        yield return new WaitForSeconds(0.2f);

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