using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;

public class RapidSpinAnimation : BaseAttackAnimation
{
    private GameObject spinPrefab;

    public RapidSpinAnimation()
    {
        spinPrefab = Resources.Load<GameObject>("VFX/RapidSpin");
    }

    public override IEnumerator PlayAnimation(BattleCardController attacker, BattleCardController defender, AttackManager attackManager, Action<Vector3, int> showDamagePopupCallback)
    {
        Transform attackerCardTransform = attacker.BattleCardView.CardRoot.transform;
        Transform defenderCardTransform = defender.BattleCardView.CardRoot.transform;

        attackerCardTransform.GetLocalPositionAndRotation(out Vector3 cardStartPos, out Quaternion cardStartRot);
        Vector3 cardStartScale = attackerCardTransform.localScale;

        yield return attackerCardTransform.DOLocalMove(new Vector3(cardStartPos.x, cardStartPos.y - 0.8f, cardStartPos.z - 0.6f), 0.2f).SetEase(Ease.OutQuad).WaitForCompletion();

        GameObject spinObject = UnityEngine.Object.Instantiate(spinPrefab, attackerCardTransform);
        spinObject.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

        Tween spinTween = attackerCardTransform.DOLocalRotate(new Vector3(360f, 0, 0), 0.2f, RotateMode.LocalAxisAdd)
            .SetLoops(-1, LoopType.Incremental).SetEase(Ease.Linear);

        yield return new WaitForSeconds(0.4f);

        Vector3 hitPoint = defenderCardTransform.position + (attackerCardTransform.position - defenderCardTransform.position).normalized * 0.5f;

        Sequence strikeSequence = DOTween.Sequence();

        strikeSequence.Append(attackerCardTransform.DOMove(hitPoint, 0.2f).SetEase(Ease.InQuad));
        strikeSequence.InsertCallback(0.2f, () =>
        {
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

            defenderCardTransform.DOShakePosition(0.3f, new Vector3(0.4f, 0.4f, 0), 20, 90, false, true);
        });

        yield return strikeSequence.WaitForCompletion();

        UnityEngine.Object.Destroy(spinObject, 0.25f);

        yield return attackerCardTransform.DOLocalMove(cardStartPos - Vector3.up * 0.8f - Vector3.forward * 0.6f, 0.2f).SetEase(Ease.OutQuad).WaitForCompletion();
        yield return attackerCardTransform.DOLocalRotateQuaternion(cardStartRot, 0.3f).SetEase(Ease.OutCubic).OnComplete(() =>
        {
            spinTween.Kill();
        }).WaitForCompletion();
        yield return attackerCardTransform.DOLocalMove(cardStartPos, 0.2f).SetEase(Ease.OutQuad).WaitForCompletion();
    }
}