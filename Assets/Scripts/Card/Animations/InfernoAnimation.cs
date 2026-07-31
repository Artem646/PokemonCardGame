using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;

public class InfernoAnimation : BaseAttackAnimation
{
    private GameObject fireStreamPrefab;
    private GameObject fireAuraPrefab;

    public InfernoAnimation()
    {
        fireStreamPrefab = Resources.Load<GameObject>("VFX/HeatWave");
        fireAuraPrefab = Resources.Load<GameObject>("VFX/FlareBlitzAura");
    }

    public override IEnumerator PlayAnimation(BattleCardController attacker, BattleCardController defender, AttackManager attackManager, Action<Vector3, int> showDamagePopupCallback)
    {
        Transform attackerCardTransform = attacker.BattleCardView.CardRoot.transform;
        Transform defenderCardTransform = defender.BattleCardView.CardRoot.transform;

        attackerCardTransform.GetPositionAndRotation(out Vector3 cardStartPos, out Quaternion cardStartRot);

        Transform exitPointTransform = attackerCardTransform.Find("AnimationObjects/StreamExitPoint");
        Vector3 targetPoint = defenderCardTransform.position + Vector3.up * 0.5f;

        GameObject effectAbilityFilter = defender.BattleCardView.CardRoot.transform.Find("AnimationObjects/EffectAbilityFilter").gameObject;
        MeshRenderer effectAbilityFilterMeshRenderer = effectAbilityFilter.GetComponent<MeshRenderer>();
        Material effectAbilityMaterial = effectAbilityFilterMeshRenderer.materials[2];
        Color32 hotColor = new(179, 73, 73, 141);

        yield return attackerCardTransform.DOLocalMoveY(cardStartPos.y + 0.5f, 0.4f).SetEase(Ease.OutQuad).WaitForCompletion();
        yield return attackerCardTransform.DOLocalRotateQuaternion(Quaternion.Euler(-60f, -90f, 180f), 0.4f).SetEase(Ease.OutBack).WaitForCompletion();

        GameObject fireStreamObject = UnityEngine.Object.Instantiate(fireStreamPrefab, exitPointTransform);
        fireStreamObject.transform.localPosition = Vector3.zero;
        fireStreamObject.transform.LookAt(targetPoint);

        ParticleSystem fireStream = fireStreamObject.GetComponentInChildren<ParticleSystem>();
        fireStream.Play();

        Sequence shotSequence = DOTween.Sequence();

        shotSequence.Append(attackerCardTransform.DOLocalRotateQuaternion(Quaternion.Euler(-120f, -90f, 180f), 0.15f).SetEase(Ease.OutExpo));
        shotSequence.Join(effectAbilityMaterial.DOColor(hotColor, 0.15f).SetEase(Ease.OutCubic));
        shotSequence.Join(attackerCardTransform.DOMove(cardStartPos + attackerCardTransform.forward * 0.5f, 0.15f).SetLoops(2, LoopType.Yoyo));
        shotSequence.OnUpdate(() => { fireStreamObject.transform.LookAt(targetPoint); });

        yield return shotSequence.WaitForCompletion();

        float distance = Vector3.Distance(attackerCardTransform.position, defenderCardTransform.position);
        float avgParticleSpeed = fireStream.main.startSpeed.constant;

        float flightTime = (distance / avgParticleSpeed * 0.5f) - 0.3f;
        if (flightTime > 0) yield return new WaitForSeconds(flightTime);

        defenderCardTransform.DOShakePosition(0.5f, new Vector3(0.3f, 0.3f, 0), 30);

        GameObject fireAuraObject = UnityEngine.Object.Instantiate(fireAuraPrefab, defenderCardTransform);
        fireAuraObject.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        ParticleSystem fireAura = fireAuraObject.GetComponentInChildren<ParticleSystem>();
        fireAura.Play();

        fireStream.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        UnityEngine.Object.Destroy(fireStreamObject, 1f);

        attackerCardTransform.DOMove(cardStartPos, 0.5f).SetEase(Ease.OutCubic);
        attackerCardTransform.DORotateQuaternion(cardStartRot, 0.5f).SetEase(Ease.OutBack);

        yield return defenderCardTransform.DOShakePosition(0.8f, new Vector3(0.3f, 0.3f, 0), 30).WaitForCompletion();

        int damage = attackManager.CalculateDamage(attacker, attacker.SelectedAbility.AbilityIndex, defender);
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

        fireAura.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        UnityEngine.Object.Destroy(fireAuraObject, 1f);

        yield return effectAbilityMaterial.DOFade(0, 0.5f).WaitForCompletion();
    }
}