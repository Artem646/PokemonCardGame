using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;

public class BugBiteAnimation : BaseAttackAnimation
{
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

        Transform mandibles = defenderCardTransform.Find("AnimationObjects/Mandibles");
        Transform topLeft = mandibles.Find("TopLeft");
        Transform topRight = mandibles.Find("TopRight");
        Transform bottomLeft = mandibles.Find("BottomLeft");
        Transform bottomRight = mandibles.Find("BottomRight");

        Quaternion startTopLeftMandible = topLeft.transform.localRotation;
        Quaternion startTopRightMandible = topRight.transform.localRotation;
        Quaternion startBottomLeftMandible = bottomLeft.transform.localRotation;
        Quaternion startBottomRightMandible = bottomRight.transform.localRotation;

        GameObject defenderCardWrapper = new("CardWrapper");
        defenderCardWrapper.transform.SetParent(defenderCardSlotTransform);
        defenderCardWrapper.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        defenderCardWrapper.transform.localScale = Vector3.one;

        defenderCardTransform.SetParent(defenderCardWrapper.transform);
        defenderCardTransform.SetLocalPositionAndRotation(defenderCardLocalPos, defenderCardLocalRot);

        Vector3 combatStancePos = defenderCardTransform.position - dirToDefender * 4.5f + Vector3.up * 3.5f;

        yield return attackerCardTransform.DOMove(combatStancePos, 0.4f).SetEase(Ease.OutQuad).WaitForCompletion();
        attackerCardTransform.DOLocalRotateQuaternion(Quaternion.Euler(-60f, -90f, 180f), 0.2f);

        mandibles.gameObject.SetActive(true);
        mandibles.localScale = Vector3.zero;
        mandibles.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutQuad);

        float openTime = 0.25f;
        topLeft.DOLocalMove(new Vector3(-0.001f, 0.02573f, 0.0407f), openTime);
        topRight.DOLocalMove(new Vector3(-0.001f, -0.02344f, 0.0409f), openTime);
        bottomLeft.DOLocalMove(new Vector3(-0.00093f, 0.02543f, -0.03721f), openTime);
        bottomRight.DOLocalMove(new Vector3(-0.00082f, -0.02354f, -0.03721f), openTime);

        yield return new WaitForSeconds(0.4f);

        float biteTime = 0.15f;
        float angleY = -91.823f;
        float angleLeftZ = 120f;
        float angleRightZ = 60f;

        float peakHeight = 0.03f;
        float targetX = 0.0001f;

        Vector3 topLeftEndPoint = new(targetX, 0.008f, 0.0095f);
        Vector3 topLeftMidPoint = Vector3.Lerp(topLeft.localPosition, topLeftEndPoint, 0.5f) + new Vector3(peakHeight, 0, 0);
        Vector3 topRightEndPoint = new(targetX, -0.005f, 0.00998f);
        Vector3 topRightMidPoint = Vector3.Lerp(topRight.localPosition, topRightEndPoint, 0.5f) + new Vector3(peakHeight, 0, 0);
        Vector3 bottomLeftEndPoint = new(targetX, 0.008f, -0.00452f);
        Vector3 bottomLeftMidPoint = Vector3.Lerp(bottomLeft.localPosition, bottomLeftEndPoint, 0.5f) + new Vector3(peakHeight, 0, 0);
        Vector3 bottomRightEndPoint = new(targetX, -0.006f, -0.0044f);
        Vector3 bottomRightMidPoint = Vector3.Lerp(bottomRight.localPosition, bottomRightEndPoint, 0.5f) + new Vector3(peakHeight, 0, 0);

        Sequence biteSequence = DOTween.Sequence();

        biteSequence.Append(attackerCardTransform.DOLocalRotateQuaternion(Quaternion.Euler(-120f, -90f, 180f), biteTime));
        biteSequence.Join(attackerCardTransform.DOMove(defenderCardTransform.position + Vector3.up * 0.2f + Vector3.forward * 0.7f, biteTime).SetEase(Ease.InExpo));

        biteSequence.Join(topLeft.DOLocalRotate(new Vector3(0, angleY, angleLeftZ), biteTime));
        biteSequence.Join(topRight.DOLocalRotate(new Vector3(0, angleY, angleRightZ), biteTime));
        biteSequence.Join(bottomLeft.DOLocalRotate(new Vector3(0, angleY, -angleLeftZ), biteTime));
        biteSequence.Join(bottomRight.DOLocalRotate(new Vector3(0, angleY, -angleRightZ), biteTime));

        biteSequence.Join(topLeft.DOLocalPath(new Vector3[] { topLeftMidPoint, topLeftEndPoint }, biteTime, PathType.CatmullRom).SetEase(Ease.InQuad));
        biteSequence.Join(topRight.DOLocalPath(new Vector3[] { topRightMidPoint, topRightEndPoint }, biteTime, PathType.CatmullRom).SetEase(Ease.InQuad));
        biteSequence.Join(bottomLeft.DOLocalPath(new Vector3[] { bottomLeftMidPoint, bottomLeftEndPoint }, biteTime, PathType.CatmullRom).SetEase(Ease.InQuad));
        biteSequence.Join(bottomRight.DOLocalPath(new Vector3[] { bottomRightMidPoint, bottomRightEndPoint }, biteTime, PathType.CatmullRom).SetEase(Ease.InQuad));

        yield return biteSequence.WaitForCompletion();

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

        defenderCardTransform.DOPunchScale(new Vector3(0, -0.15f, 0.15f), 0.3f, 15, 1f);
        defenderCardTransform.DOShakePosition(0.4f, new Vector3(0.4f, 0.4f, 0), 30);

        mandibles.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack).OnComplete(() =>
        {
            mandibles.gameObject.SetActive(false);
            topLeft.transform.localRotation = startTopLeftMandible;
            topRight.transform.localRotation = startTopRightMandible;
            bottomLeft.transform.localRotation = startBottomLeftMandible;
            bottomRight.transform.localRotation = startBottomRightMandible;
        });

        yield return new WaitForSeconds(0.3f);

        Sequence returnSequence = DOTween.Sequence();
        returnSequence.Append(defenderCardWrapper.transform.DOLocalMove(Vector3.zero, 0.4f).SetEase(Ease.OutQuad));
        returnSequence.Join(attackerCardTransform.DOMove(attackerCardStartPos, 0.4f).SetEase(Ease.OutQuad));
        returnSequence.Join(attackerCardTransform.DORotateQuaternion(attackerCardStartRot, 0.4f).SetEase(Ease.OutQuad));
        yield return returnSequence.WaitForCompletion();

        defenderCardTransform.SetParent(defenderCardSlotTransform);
        defenderCardTransform.SetLocalPositionAndRotation(defenderCardLocalPos, defenderCardLocalRot);

        UnityEngine.Object.Destroy(defenderCardWrapper);
    }
}