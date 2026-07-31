using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;

public class FlashCannonAnimation : BaseAttackAnimation
{
    private GameObject beamPrefab;

    public FlashCannonAnimation()
    {
        beamPrefab = Resources.Load<GameObject>("VFX/FlashCannonBeam");
    }

    public override IEnumerator PlayAnimation(BattleCardController attacker, BattleCardController defender, AttackManager attackManager, Action<Vector3, int> showDamagePopupCallback)
    {
        Transform attackerCardTransform = attacker.BattleCardView.CardRoot.transform;
        Transform defenderCardTransform = defender.BattleCardView.CardRoot.transform;

        attackerCardTransform.GetPositionAndRotation(out Vector3 cardStartPos, out Quaternion cardStartRot);
        Vector3 cardStartScale = attackerCardTransform.localScale;

        Transform exitPointTransform = attackerCardTransform.Find("AnimationObjects/StreamExitPoint");

        Sequence chargeSequence = DOTween.Sequence();

        Vector3 chargePos = cardStartPos + new Vector3(0, 1.5f, 1.5f);
        chargeSequence.Append(attackerCardTransform.DOMove(chargePos, 0.5f).SetEase(Ease.OutQuad));
        chargeSequence.Join(attackerCardTransform.DOLocalRotateQuaternion(Quaternion.Euler(-70f, -90f, 180f), 0.5f));
        chargeSequence.Append(attackerCardTransform.DOShakePosition(0.5f, 0.15f, 30, 90, false, true));

        yield return chargeSequence.WaitForCompletion();

        GameObject beamObject = UnityEngine.Object.Instantiate(beamPrefab);
        LineRenderer beamLine = beamObject.GetComponentInChildren<LineRenderer>();

        beamLine.SetPosition(0, exitPointTransform.position + Vector3.back * 0.5f);
        beamLine.SetPosition(1, defenderCardTransform.position + Vector3.up * 0.4f);
        beamLine.widthMultiplier = 0f;

        Sequence shotSequence = DOTween.Sequence();

        shotSequence.Append(DOTween.To(() => beamLine.widthMultiplier, x => beamLine.widthMultiplier = x, 1.5f, 0.05f).SetEase(Ease.OutExpo));
        shotSequence.Join(attackerCardTransform.DOMoveZ(chargePos.z - 2f, 0.1f).SetEase(Ease.OutExpo));
        shotSequence.Join(defenderCardTransform.DOShakePosition(0.4f, new Vector3(0.5f, 0.5f, 0), 20, 90, false, true));
        shotSequence.Join(defenderCardTransform.DOPunchScale(new Vector3(0.6f, -0.4f, 0.6f), 0.4f, 10, 1f));

        shotSequence.OnUpdate(() => { beamLine.SetPosition(0, exitPointTransform.position + Vector3.back * 0.5f); });

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
                    int attackerSlotIndex = (int)attacker.BattleCardView.CardRoot.transform.parent.GetComponent<CardSlot>().indexSlotInField;
                    int defenderSlotIndex = (int)defender.BattleCardView.CardRoot.transform.parent.GetComponent<CardSlot>().indexSlotInField;
                    networkGameController.RequestApplyDamage(attackerSlotIndex, attacker.SelectedAbility.AbilityIndex, defenderSlotIndex);
                }
            }
        }

        DOTween.To(() => beamLine.widthMultiplier, x => beamLine.widthMultiplier = x, 0f, 0.3f)
            .SetEase(Ease.InQuad).OnComplete(() => UnityEngine.Object.Destroy(beamObject));

        Sequence returnSequence = DOTween.Sequence();
        returnSequence.Append(attackerCardTransform.DOMove(cardStartPos, 0.4f)).SetEase(Ease.OutCubic);
        returnSequence.Join(attackerCardTransform.DORotateQuaternion(cardStartRot, 0.3f)).SetEase(Ease.OutQuad);
        returnSequence.Join(attackerCardTransform.DOScale(cardStartScale, 0.3f)).SetEase(Ease.OutQuad);
        yield return returnSequence.WaitForCompletion();
    }
}