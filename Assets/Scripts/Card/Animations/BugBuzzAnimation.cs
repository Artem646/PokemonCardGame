using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;

public class BugBuzzAnimation : BaseAttackAnimation
{
    private GameObject buzzPrefab;

    public BugBuzzAnimation()
    {
        buzzPrefab = Resources.Load<GameObject>("VFX/BugBuzz");
    }

    public override IEnumerator PlayAnimation(BattleCardController attacker, BattleCardController defender, AttackManager attackManager, Action<Vector3, int> showDamagePopupCallback)
    {
        Transform attackerCardTransform = attacker.BattleCardView.CardRoot.transform;
        Transform defenderCardTransform = defender.BattleCardView.CardRoot.transform;

        attackerCardTransform.GetPositionAndRotation(out Vector3 startPos, out Quaternion startRot);
        Vector3 attackerCardLocalStartPos = attackerCardTransform.localPosition;
        Vector3 defenderCardLocalStartPos = defenderCardTransform.localPosition;

        Vector3 dirToDefender = (defenderCardTransform.position - attackerCardTransform.position).normalized;
        int damage = attackManager.CalculateDamage(attacker, attacker.SelectedAbility.AbilityIndex, defender);

        yield return attackerCardTransform.DOMove(startPos + Vector3.up * 0.5f, 0.3f).SetEase(Ease.OutQuad).WaitForCompletion();

        attackerCardTransform.DOShakePosition(1.0f, new Vector3(0.15f, 0.15f, 0), 50, 90, false, true);
        attackerCardTransform.DOLocalRotateQuaternion(Quaternion.Euler(-100f, -90f, 180f), 0.2f);

        GameObject buzzObject = UnityEngine.Object.Instantiate(buzzPrefab, attackerCardTransform);
        buzzObject.transform.LookAt(defenderCardTransform.position);

        ParticleSystem buzzParticles = buzzObject.GetComponentInChildren<ParticleSystem>();
        buzzParticles.Play();

        float timer = 0;
        while (timer < 1f)
        {
            timer += Time.deltaTime;
            defenderCardTransform.DOShakePosition(0.1f, new Vector3(0.2f, 0.2f, 0), 20);
            yield return null;
        }

        buzzParticles.Stop();

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

        defenderCardTransform.DOShakeRotation(0.5f, new Vector3(1f, 1f, 0), 30);

        yield return new WaitForSeconds(0.3f);

        Sequence returnSequence = DOTween.Sequence();
        returnSequence.Append(attackerCardTransform.DOMove(startPos, 0.4f).SetEase(Ease.OutQuad));
        returnSequence.Join(attackerCardTransform.DORotateQuaternion(startRot, 0.4f).SetEase(Ease.OutBack));
        returnSequence.Join(defenderCardTransform.DOLocalMove(defenderCardLocalStartPos, 0.1f));
        returnSequence.Append(attackerCardTransform.DOLocalMove(attackerCardLocalStartPos, 0.05f));
        yield return returnSequence.WaitForCompletion();
    }
}