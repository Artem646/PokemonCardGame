using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;

public class SolarBeamAnimation : BaseAttackAnimation
{
    private GameObject gatherPrefab;
    private GameObject beamPrefab;

    public SolarBeamAnimation()
    {
        gatherPrefab = Resources.Load<GameObject>("VFX/SolarBeamGather");
        beamPrefab = Resources.Load<GameObject>("VFX/SolarBeam");
    }

    public override IEnumerator PlayAnimation(BattleCardController attacker, BattleCardController defender, AttackManager attackManager, Action<Vector3, int> showDamagePopupCallback)
    {
        Transform attackerCardTransform = attacker.BattleCardView.CardRoot.transform;
        Transform defenderCardTransform = defender.BattleCardView.CardRoot.transform;

        attackerCardTransform.GetPositionAndRotation(out Vector3 cardStartPos, out Quaternion cardStartRot);
        Vector3 cardStartScale = attackerCardTransform.localScale;

        Transform exitPointTransform = attackerCardTransform.Find("AnimationObjects/StreamExitPoint");

        Sequence chargeSequence = DOTween.Sequence();

        Vector3 highPos = cardStartPos + new Vector3(0, 1.5f, 1.5f);
        chargeSequence.Append(attackerCardTransform.DOMove(highPos, 0.7f).SetEase(Ease.OutBack));
        chargeSequence.Join(attackerCardTransform.DOLocalRotateQuaternion(Quaternion.Euler(-60f, -90f, 180f), 0.7f));

        yield return chargeSequence.WaitForCompletion();

        GameObject gatherObject = UnityEngine.Object.Instantiate(gatherPrefab, exitPointTransform.position, Quaternion.identity);
        gatherObject.transform.SetParent(exitPointTransform);

        ParticleSystem particleSystem = gatherObject.GetComponentInChildren<ParticleSystem>();
        particleSystem.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        particleSystem.Play();

        yield return attackerCardTransform.DOShakePosition(1.0f, 0.2f, 40, 90, false, true).WaitForCompletion();

        GameObject beamObject = UnityEngine.Object.Instantiate(beamPrefab);
        LineRenderer beamLine = beamObject.GetComponentInChildren<LineRenderer>();

        beamLine.SetPosition(0, exitPointTransform.position - Vector3.forward * 0.3f);
        beamLine.SetPosition(1, defenderCardTransform.position + Vector3.up * 0.4f);
        beamLine.widthMultiplier = 0f;

        UnityEngine.Object.Destroy(gatherObject);

        Sequence shotSequence = DOTween.Sequence();

        shotSequence.Append(DOTween.To(() => beamLine.widthMultiplier, x => beamLine.widthMultiplier = x, 3.0f, 0.07f).SetEase(Ease.OutExpo));
        shotSequence.Join(attackerCardTransform.DOMoveZ(highPos.z - 2.0f, 0.15f).SetEase(Ease.OutExpo));
        shotSequence.Join(defenderCardTransform.DOShakePosition(0.6f, new Vector3(0.5f, 0.8f, 0), 30, 90, false, true));
        shotSequence.Join(defenderCardTransform.DOPunchScale(new Vector3(0.8f, -0.7f, 0.8f), 0.5f, 15, 1f));

        shotSequence.OnUpdate(() => { beamLine.SetPosition(0, exitPointTransform.position - Vector3.forward * 0.3f); });

        yield return shotSequence.WaitForCompletion();

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

        DOTween.To(() => beamLine.widthMultiplier, x => beamLine.widthMultiplier = x, 0f, 0.4f)
            .SetEase(Ease.InSine).OnComplete(() => UnityEngine.Object.Destroy(beamObject));

        Sequence returnSequence = DOTween.Sequence();
        returnSequence.Append(attackerCardTransform.DOMove(cardStartPos, 0.5f).SetEase(Ease.OutQuad));
        returnSequence.Join(attackerCardTransform.DORotateQuaternion(cardStartRot, 0.4f));
        returnSequence.Join(attackerCardTransform.DOScale(cardStartScale, 0.4f));
        yield return returnSequence.WaitForCompletion();
    }
}