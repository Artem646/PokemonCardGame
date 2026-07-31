using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;

public class SpitUpAnimation : BaseAttackAnimation
{
    private GameObject heatWavePrefab;

    public SpitUpAnimation()
    {
        heatWavePrefab = Resources.Load<GameObject>("VFX/SpitUp");
    }

    public override IEnumerator PlayAnimation(BattleCardController attacker, BattleCardController defender, AttackManager attackManager, Action<Vector3, int> showDamagePopupCallback)
    {
        Transform attackerCardTransform = attacker.BattleCardView.CardRoot.transform;
        Transform defenderCardTransform = defender.BattleCardView.CardRoot.transform;

        attackerCardTransform.GetPositionAndRotation(out Vector3 cardStartPos, out Quaternion cardStartRot);

        Transform exitPointTransform = attackerCardTransform.Find("AnimationObjects/StreamExitPoint");
        Vector3 targetPoint = defenderCardTransform.position + Vector3.up * 0.5f;

        yield return attackerCardTransform.DOLocalMoveY(cardStartPos.y + 0.5f, 0.4f).SetEase(Ease.OutQuad).WaitForCompletion();
        yield return attackerCardTransform.DOLocalRotateQuaternion(Quaternion.Euler(-60f, -90f, 180f), 0.4f).SetEase(Ease.OutBack).WaitForCompletion();

        GameObject heatWaveObject = UnityEngine.Object.Instantiate(heatWavePrefab, exitPointTransform);
        heatWaveObject.transform.localPosition = Vector3.zero;
        heatWaveObject.transform.LookAt(targetPoint);

        ParticleSystem heatWaveParticles = heatWaveObject.GetComponentInChildren<ParticleSystem>();
        heatWaveParticles.Play();

        float destroyDelay = heatWaveParticles.main.duration + heatWaveParticles.main.startLifetime.constant;
        UnityEngine.Object.Destroy(heatWaveObject, destroyDelay);

        Sequence shotSequence = DOTween.Sequence();

        shotSequence.Append(attackerCardTransform.DOLocalRotateQuaternion(Quaternion.Euler(-120f, -90f, 180f), 0.15f).SetEase(Ease.OutExpo));
        shotSequence.Join(attackerCardTransform.DOMove(cardStartPos + attackerCardTransform.forward * 0.5f, 0.15f).SetLoops(2, LoopType.Yoyo));
        shotSequence.OnUpdate(() => { heatWaveObject.transform.LookAt(targetPoint); });

        yield return shotSequence.WaitForCompletion();

        float distance = Vector3.Distance(attackerCardTransform.position, defenderCardTransform.position);
        float avgParticleSpeed = heatWaveParticles.main.startSpeed.constant;

        float flightTime = (distance / avgParticleSpeed * 0.5f) - 0.3f;
        if (flightTime > 0) yield return new WaitForSeconds(flightTime);

        defenderCardTransform.DOShakePosition(0.5f, new Vector3(0.3f, 0.3f, 0), 30);

        int damage = attackManager.CalculateDamage(attacker, attacker.SelectedAbility.AbilityIndex, defender);
        showDamagePopupCallback?.Invoke(defenderCardTransform.position + Vector3.up * 1f, damage);

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

        heatWaveParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);

        yield return new WaitForSeconds(0.3f);

        Sequence returnSequence = DOTween.Sequence();
        returnSequence.Append(attackerCardTransform.DOMove(cardStartPos, 0.4f).SetEase(Ease.OutCubic));
        returnSequence.Join(attackerCardTransform.DORotateQuaternion(cardStartRot, 0.4f).SetEase(Ease.OutBack));
        yield return returnSequence.WaitForCompletion();
    }
}