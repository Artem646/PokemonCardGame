using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;

public class NuzzleAnimation : BaseAttackAnimation
{
    private GameObject splashPrefab;

    public NuzzleAnimation()
    {
        splashPrefab = Resources.Load<GameObject>("VFX/NuzzleSplash");
    }

    public override IEnumerator PlayAnimation(BattleCardController attacker, BattleCardController defender, AttackManager attackManager, Action<Vector3, int> showDamagePopupCallback)
    {
        Transform attackerCardTransform = attacker.BattleCardView.CardRoot.transform;
        Transform defenderCardTransform = defender.BattleCardView.CardRoot.transform;

        attackerCardTransform.GetPositionAndRotation(out Vector3 cardStartPos, out Quaternion cardStartRot);
        Vector3 cardStartScale = attackerCardTransform.localScale;

        float liftHeight = 0.5f;
        int damage = attackManager.CalculateDamage(attacker, attacker.SelectedAbility.AbilityIndex, defender);

        Vector3 combatStancePos = defenderCardTransform.position + (attackerCardTransform.position - defenderCardTransform.position).normalized * 1.0f;
        combatStancePos.y += liftHeight;

        yield return attackerCardTransform.DOMove(combatStancePos, 0.4f).SetEase(Ease.InBack).WaitForCompletion();

        int biteCount = 6;
        Vector3 hitPoint = defenderCardTransform.position + Vector3.up * 0.2f;

        for (int i = 0; i < biteCount; i++)
        {
            Sequence biteSequence = DOTween.Sequence();

            biteSequence.Append(attackerCardTransform.DOLocalRotateQuaternion(Quaternion.Euler(-80f, -90f, 180f), 0.1f).SetEase(Ease.OutCubic));
            biteSequence.Join(attackerCardTransform.DOLocalRotateQuaternion(Quaternion.Euler(-100f, -90f, 180f), 0.1f).SetEase(Ease.InExpo));

            yield return biteSequence.WaitForCompletion();

            yield return new WaitForSeconds(0.05f);
        }

        defenderCardTransform.DOShakePosition(0.3f, new Vector3(0.5f, 0.3f, 0), 35);

        GameObject electricSplashObject = UnityEngine.Object.Instantiate(splashPrefab, hitPoint, Quaternion.identity);
        ParticleSystem electricSplash = electricSplashObject.GetComponentInChildren<ParticleSystem>();
        electricSplash.Play();
        UnityEngine.Object.Destroy(electricSplashObject, 2f);

        showDamagePopupCallback?.Invoke(hitPoint + Vector3.up * 1f, damage);

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

        Sequence returnSequence = DOTween.Sequence();
        returnSequence.Append(attackerCardTransform.DOMove(cardStartPos, 0.4f).SetEase(Ease.OutCubic));
        returnSequence.Join(attackerCardTransform.DORotateQuaternion(cardStartRot, 0.4f).SetEase(Ease.OutQuad));
        yield return returnSequence.WaitForCompletion();
    }
}