using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

public static class AnimationWithOutDamageMode
{
    public static bool Mode = false;
}

public class CardAttackExecutor : MonoBehaviour
{
    [SerializeField] private CardDissolveScript cardDissolveScript;

    private GameManager gameManager;
    private AttackManager attackManager;
    private NetworkGameController networkGameController;
    private AttackTargetingPanelController attackTargetingPanel;

    private GameType currentType = GameTypeConfig.CurrentType;
    public event Action OnAnimationFinished;

    private void InitObjectsIfNeeded()
    {
        if (gameManager == null) gameManager = FindAnyObjectByType<GameManager>();
        attackManager = FindAnyObjectByType<AttackManager>();
        if (attackTargetingPanel == null) attackTargetingPanel = FindAnyObjectByType<AttackTargetingPanelController>();
        if (currentType == GameType.Multiplayer && networkGameController == null)
            networkGameController = FindAnyObjectByType<NetworkGameController>();
    }

    public IEnumerator ExecuteAttackRoutine(BattleCardController attacker, BattleCardController defender)
    {
        InitObjectsIfNeeded();

        AnimationWithOutDamageMode.Mode = false;

        bool isPlayerCard = attacker.BattleState.Owner == CardOwner.Player;

        Transform defenderCardTransform = defender.BattleCardView.CardRoot.transform;
        Transform attackerCardTransform = transform;

        CardStateInteractionManager.LockCard(this, attackerCardTransform);
        CardStateInteractionManager.LockCard(defenderCardTransform.GetComponent<CardAttackExecutor>(), defenderCardTransform);

        if (isPlayerCard)
        {
            attackerCardTransform.Find("CardFrame").gameObject.SetActive(false);
            defenderCardTransform.Find("CardFrame").gameObject.SetActive(false);
        }

        string abilityName = attacker.SelectedAbility.AbilityModel.name;
        BaseAttackAnimation animation = AttackAnimationRegistry.GetAnimationForAbility(abilityName);
        yield return StartCoroutine(animation.PlayAnimation(attacker, defender, attackManager, ShowDamagePanel));

        CardSlot attackerCardSlot = attackerCardTransform.parent.GetComponent<CardSlot>();
        CardSlot defenderCardSlot = defenderCardTransform.parent.GetComponent<CardSlot>();

        bool isDefenderCardDied = defender.BattleState.CurrentHP <= 0;
        if (isDefenderCardDied)
        {
            defenderCardTransform.DOKill();

            defender.BattleCardView.ResetBattleStyle();

            CardSlot[] resetSlots = isPlayerCard ? gameManager.EnemyResetSlots : gameManager.PlayerResetSlots;
            foreach (CardSlot cardSlot in resetSlots)
            {
                if (cardSlot.IsEmpty)
                {
                    StartCoroutine(cardSlot.PlaceCardWithMoveAndFlip(defender));
                    break;
                }
            }

            defenderCardSlot.Clear();

            BattleCardController[] fieldControllers = isPlayerCard ? gameManager.CurrentGame.EnemyFieldControllers : gameManager.CurrentGame.PlayerFieldControllers;
            fieldControllers[(int)defenderCardSlot.indexSlotInField] = null;
        }

        CardStateInteractionManager.UnlockCard(defenderCardTransform.GetComponent<CardAttackExecutor>(), defenderCardTransform);
        CardStateInteractionManager.UnlockCard(this, attackerCardTransform);

        if (isPlayerCard)
        {
            bool activeFrame = gameManager.IsMyTurn;
            transform.Find("CardFrame").gameObject.SetActive(activeFrame);

            OnAnimationFinished?.Invoke();
        }

        if (isDefenderCardDied && attacker.CardModel.evolutions.next.HasValue)
        {
            bool canEvolve;

            if (isPlayerCard) canEvolve = CardRepository.Instance.GetUserCardModelById(attacker.CardModel.evolutions.next.Value) != null;
            else canEvolve = true;

            if (canEvolve)
            {
                yield return new WaitForSeconds(0.6f);
                yield return EvolutionRoutine(attacker, isPlayerCard);
            }
        }
    }

    public void ExecuteNetworkAttack(BattleCardController attacker, BattleCardController defender)
    {
        InitObjectsIfNeeded();
        AnimationWithOutDamageMode.Mode = true;
        StartCoroutine(AnimationRoutine(attacker, defender, ShowDamagePanel, gameManager.IsMyTurn));
    }

    public IEnumerator ExecuteOnlyAnimation(BattleCardController attacker, BattleCardController defender, Action onCompleteCallback)
    {
        attackManager = FindAnyObjectByType<AttackManager>();
        AnimationWithOutDamageMode.Mode = true;
        yield return AnimationRoutine(attacker, defender, null, false);
        onCompleteCallback?.Invoke();
    }

    private IEnumerator AnimationRoutine(BattleCardController attacker, BattleCardController defender, Action<Vector3, int> showDamagePanelCallback, bool isMyAttack)
    {
        Transform enemyCardTransform = defender.BattleCardView.CardRoot.transform;

        CardStateInteractionManager.LockCard(this, transform);
        CardStateInteractionManager.LockCard(enemyCardTransform.GetComponent<CardAttackExecutor>(), enemyCardTransform);

        if (isMyAttack)
        {
            transform.Find("CardFrame").gameObject.SetActive(false);
            enemyCardTransform.Find("CardFrame").gameObject.SetActive(false);
        }

        string abilityName = attacker.SelectedAbility.AbilityModel.name;
        BaseAttackAnimation animation = AttackAnimationRegistry.GetAnimationForAbility(abilityName);
        yield return animation.PlayAnimation(attacker, defender, attackManager, showDamagePanelCallback);

        CardStateInteractionManager.UnlockCard(enemyCardTransform.GetComponent<CardAttackExecutor>(), enemyCardTransform);
        CardStateInteractionManager.UnlockCard(this, transform);

        if (isMyAttack)
        {
            attacker.MarkAsAttacked();
            attacker.BattleCardView.ApplyBattleStyle(attacker.BattleState);

            bool activeFrame = gameManager.IsMyTurn;
            transform.Find("CardFrame").gameObject.SetActive(activeFrame);

            yield return new WaitForSeconds(0.5f);

            networkGameController.RequestClearDeadCards();

            if (attacker.CardModel.evolutions.next.HasValue)
            {
                int newCardId = attacker.CardModel.evolutions.next.Value;
                if (CardRepository.Instance.GetUserCardModelById(newCardId) != null)
                {
                    yield return new WaitForSeconds(0.6f);
                    int fieldSlotIndex = (int)attacker.BattleCardView.CardRoot.transform.parent.GetComponent<CardSlot>().indexSlotInField;
                    networkGameController.RequestEvolutionCard(fieldSlotIndex, newCardId);
                }
            }

            OnAnimationFinished?.Invoke();
        }
    }

    public IEnumerator EvolutionRoutine(BattleCardController oldCardController, bool isMyCard)
    {
        CardStateInteractionManager.LockCard(this, transform);
        yield return cardDissolveScript.DissolveCardForEvolutionRoutine(oldCardController, oldCardController.CardModel.evolutions.next.Value, isMyCard,
                () => CardStateInteractionManager.UnlockCard(this, transform));
    }

    // private void ShowDamagePanel(Vector3 position, int amount)
    // {
    //     GameObject damagePanel = Instantiate(damagePanelPrefab, position + Vector3.up * 3f, Quaternion.Euler(0, 0, 0));

    //     TextMeshPro damageText = damagePanel.GetComponentInChildren<TextMeshPro>();
    //     MeshRenderer damagePlateRenderer = damagePanel.GetComponentInChildren<MeshRenderer>();

    //     damageText.text = $"-{amount}";

    //     damagePanel.transform.localScale = Vector3.zero;

    //     Sequence sequence = DOTween.Sequence();

    //     sequence.Append(damagePanel.transform.DOScale(1.2f, 0.2f).SetEase(Ease.OutBack));
    //     sequence.Join(damagePanel.transform.DOMoveY(damagePanel.transform.position.y + 1.5f, 1.2f).SetEase(Ease.OutCubic));

    //     sequence.Insert(0.6f, damageText.DOFade(0, 0.6f));

    //     sequence.Join(damagePlateRenderer.material.DOFade(0, 0.6f));

    //     sequence.OnComplete(() =>
    //     {
    //         Destroy(damagePlateRenderer.material);
    //         Destroy(damagePanel);
    //     });
    // }

    private void ShowDamagePanel(Vector3 position, int amount)
    {
        DamagePopupManager.Instance.ShowDamage(position, amount);
    }
}