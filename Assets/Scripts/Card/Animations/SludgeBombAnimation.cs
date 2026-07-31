using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;

public class SludgeBombAnimation : BaseAttackAnimation
{
    private GameObject sludgeBombPrefab;

    public SludgeBombAnimation()
    {
        sludgeBombPrefab = Resources.Load<GameObject>("VFX/SludgeBomb");
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

        int damage = attackManager.CalculateDamage(attacker, attacker.SelectedAbility.AbilityIndex, defender);
        Vector3 dirToDefender = (defenderCardTransform.position - attackerCardTransform.position).normalized;

        Transform exitPointTransform = attackerCardTransform.Find("AnimationObjects/StreamExitPoint");

        GameObject mudSludgesPlate = defender.BattleCardView.CardRoot.transform.Find("AnimationObjects/MudSludgesPlate").gameObject;
        MeshRenderer mudSludgesPlateMeshRenderer = mudSludgesPlate.GetComponent<MeshRenderer>();
        Material mudSludgesPlateMaterial = mudSludgesPlateMeshRenderer.materials[2];

        GameObject defenderCardWrapper = new("CardWrapper");
        defenderCardWrapper.transform.SetParent(defenderCardSlotTransform);
        defenderCardWrapper.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        defenderCardWrapper.transform.localScale = Vector3.one;

        defenderCardTransform.SetParent(defenderCardWrapper.transform);
        defenderCardTransform.SetLocalPositionAndRotation(defenderCardLocalPos, defenderCardLocalRot);

        Vector3 upPos = attackerCardTransform.position + Vector3.up * 3.5f - dirToDefender * 4f;

        yield return attackerCardTransform.DOMove(upPos, 0.4f).SetEase(Ease.OutQuad).WaitForCompletion();
        yield return attackerCardTransform.DOLocalRotateQuaternion(Quaternion.Euler(-110f, -90f, 180f), 0.2f).SetEase(Ease.OutBack).WaitForCompletion();

        GameObject sludgeBombObject = UnityEngine.Object.Instantiate(sludgeBombPrefab, exitPointTransform);
        sludgeBombObject.transform.LookAt(defenderCardTransform.position);
        ParticleSystem sludgeBombParticles = sludgeBombObject.GetComponentInChildren<ParticleSystem>();
        sludgeBombParticles.Play();

        Sequence collisionSequence = DOTween.Sequence();

        collisionSequence.Append(defenderCardWrapper.transform.DOLocalRotateQuaternion(Quaternion.Euler(0, 0, 30f), 0.2f).SetLoops(2, LoopType.Yoyo));
        collisionSequence.Join(defenderCardTransform.DOShakePosition(0.5f, new Vector3(0.3f, 0.3f, 0), 40));
        collisionSequence.Join(mudSludgesPlateMaterial.DOFade(1f, 0.8f).SetEase(Ease.OutCubic));

        yield return collisionSequence.WaitForCompletion();

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

        yield return new WaitForSeconds(0.2f);

        Sequence returnSequence = DOTween.Sequence();
        returnSequence.Append(attackerCardTransform.DOMove(attackerCardStartPos, 0.4f).SetEase(Ease.OutCubic));
        returnSequence.Join(attackerCardTransform.DORotateQuaternion(attackerCardStartRot, 0.4f).SetEase(Ease.OutBack));
        returnSequence.Join(defenderCardWrapper.transform.DOLocalMove(Vector3.zero, 0.4f).SetEase(Ease.OutQuad));
        returnSequence.Join(defenderCardWrapper.transform.DOLocalRotateQuaternion(Quaternion.identity, 0.4f).SetEase(Ease.OutQuad));
        yield return returnSequence.WaitForCompletion();

        defenderCardTransform.SetParent(defenderCardSlotTransform);
        defenderCardTransform.SetLocalPositionAndRotation(defenderCardLocalPos, defenderCardLocalRot);

        UnityEngine.Object.Destroy(defenderCardWrapper);

        yield return new WaitForSeconds(0.5f);

        yield return mudSludgesPlateMaterial.DOFade(0, 0.5f).WaitForCompletion();
    }
}
