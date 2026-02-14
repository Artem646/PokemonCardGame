using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;

public class CardAttackDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Transform cardParent;
    private Transform tempCardParent;

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform rectTransform;

    private GameManagerScript gameManager;
    private AttackManager attackManager;
    private BotTurnManager botTurnManager;
    private NetworkGameTurnManager networkTurnManager;
    private NetworkGameController networkGameController;

    private BattleCardController cardController;
    private BattleCardController enemyCard;

    private Canvas canvas;
    private Vector3 originalScale;
    private GameObject tempCard;

    private FieldType fieldType;
    private bool isAttackDraggable;

    private GameType currentType = GameTypeConfig.CurrentType;

    private void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        originalScale = rectTransform.localScale;
        tempCard = GameObject.Find("TempSlot");
        gameManager = FindAnyObjectByType<GameManagerScript>();
        attackManager = FindAnyObjectByType<AttackManager>();
        botTurnManager = FindAnyObjectByType<BotTurnManager>();
        networkTurnManager = FindAnyObjectByType<NetworkGameTurnManager>();
        networkGameController = FindAnyObjectByType<NetworkGameController>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        cardParent = tempCardParent = transform.parent;

        if (cardParent.TryGetComponent<DropPlaceScript>(out var dropPlace))
            fieldType = dropPlace.type;
        else
            fieldType = FieldType.NONE;

        if (TryGetComponent<CardControllerLink>(out var link))
            cardController = link.Controller;

        isAttackDraggable = gameManager.IsMyTurn && fieldType == FieldType.SELF_FIELD
            && cardController.CanAttack && (currentType == GameType.Multiplayer ?
               networkTurnManager.CurrentPhase == TurnPhase.Attack :
               botTurnManager.CurrentPhase == TurnPhase.Attack);

        if (isAttackDraggable && CardStateInteractionManager.TryBeginDrag(this))
        {
            tempCard.transform.SetParent(tempCardParent);
            tempCard.transform.SetSiblingIndex(transform.GetSiblingIndex());

            transform.SetParent(canvas.transform, true);

            canvasGroup.blocksRaycasts = false;
            rectTransform.DOScale(originalScale * 1.15f, 0.1f);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isAttackDraggable && CardStateInteractionManager.IsDraggingBy(this))
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform, eventData.position,
                canvas.worldCamera, out Vector2 localPoint);

            rectTransform.localPosition = localPoint;

            DetectEnemyCardUnderCursor(eventData);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isAttackDraggable && CardStateInteractionManager.IsDraggingBy(this))
        {
            CardStateInteractionManager.EndDrag();
            canvasGroup.blocksRaycasts = true;
            StartCoroutine(OnEndDragRoutine());
        }
    }

    private IEnumerator OnEndDragRoutine()
    {
        if (enemyCard != null)
        {
            Transform enemyCardTransform = enemyCard.BattleCardView.CardRoot.transform;
            Transform enemyResetStack = gameManager.EnemyResetStack;

            Sequence hitSequence = DOTween.Sequence();

            hitSequence.Append(transform.DORotate(new Vector3(0, 0, 20f), 0.1f, RotateMode.Fast));
            hitSequence.Join(transform.DOShakeRotation(0.25f, 30f, 20, 90f, true));
            hitSequence.Join(enemyCardTransform.DOScale(1.1f, 0.1f).SetEase(Ease.OutQuad));
            hitSequence.Join(enemyCardTransform.DORotate(new Vector3(0, 0, -10f), 0.12f, RotateMode.Fast));
            hitSequence.Join(enemyCardTransform.DOShakeRotation(0.20f, 15f, 15, 90f, true));

            hitSequence.Append(transform.DORotate(Vector3.zero, 0.1f));
            hitSequence.Join(enemyCardTransform.DOScale(1f, 0.1f).SetEase(Ease.OutQuad));
            hitSequence.Join(enemyCardTransform.DORotate(Vector3.zero, 0.1f));

            yield return hitSequence.WaitForCompletion();

            if (currentType == GameType.Bot)
                attackManager.PerformAttack(cardController, enemyCard);
            else if (currentType == GameType.Multiplayer)
            {
                networkGameController.RpcRequestAttack(cardController.CardModel.id, enemyCard.CardModel.id);
                attackManager.PerformAttack(cardController, enemyCard);
            }

            if (enemyCard.BattleState.CurrentHP <= 0 &&
                enemyCard.BattleCardView.CardRoot.TryGetComponent<CardMovemantScript>(out var card))
            {
                enemyCard.BattleCardView.ResetBattleStyle();
                StartCoroutine(card.MoveCardTransformToAnotherField(enemyResetStack, enemyResetStack.childCount, transform));
                gameManager.CurrentGame.EnemyFieldListController.CardControllers.Remove(enemyCard);
            }

            HighlightEnemyCard(null);
        }

        yield return transform.DOMove(tempCard.transform.position, 0.3f).SetEase(Ease.OutQuad).WaitForCompletion();

        transform.SetParent(cardParent, true);
        transform.SetSiblingIndex(tempCard.transform.GetSiblingIndex());

        tempCard.transform.SetParent(canvas.transform);
        tempCard.transform.localPosition = new Vector3(2600, 0);

        rectTransform.DOScale(originalScale, 0.1f);
    }

    private void DetectEnemyCardUnderCursor(PointerEventData eventData)
    {
        List<RaycastResult> results = new();
        EventSystem.current.RaycastAll(eventData, results);

        BattleCardController newEnemyCard = null;

        foreach (RaycastResult hit in results)
        {
            if (hit.gameObject.TryGetComponent<CardControllerLink>(out var link))
            {
                BattleCardController card = link.Controller;
                if (gameManager.CurrentGame.EnemyFieldListController.CardControllers.Contains(card))
                {
                    newEnemyCard = card;
                    break;
                }
            }
        }

        if (newEnemyCard != enemyCard)
        {
            HighlightEnemyCard(newEnemyCard);
            enemyCard = newEnemyCard;
        }
    }

    private void HighlightEnemyCard(BattleCardController newEnemyCard)
    {
        if (enemyCard != null)
        {
            GameObject oldCardRoot = enemyCard.BattleCardView.CardRoot;
            GameObject highlighted = oldCardRoot.transform.Find("Highlighted").gameObject;
            highlighted.SetActive(false);
        }

        if (newEnemyCard != null)
        {
            GameObject newCardRoot = newEnemyCard.BattleCardView.CardRoot;
            GameObject highlighted = newCardRoot.transform.Find("Highlighted").gameObject;
            highlighted.SetActive(true);
        }
    }

    public IEnumerator MoveEnemyCardTransformForAttack(Transform targetTransform, BattleCardController attacker, BattleCardController defender)
    {
        Transform enemyFieldTransform = gameManager.EnemyFieldContainer;
        Transform playerResetStack = gameManager.PlayerResetStack;

        Vector3 startPosition = transform.position;
        Vector3 targetPosition = targetTransform.position;
        int siblingIndex = transform.GetSiblingIndex();

        enemyFieldTransform.GetComponent<HorizontalLayoutGroup>().enabled = false;

        transform.SetParent(canvas.transform);

        yield return transform.DOMove(targetPosition, 0.5f).SetEase(Ease.OutQuad).WaitForCompletion();

        Sequence hitSequence = DOTween.Sequence();

        hitSequence.Append(transform.DOScale(1.15f, 0.1f).SetEase(Ease.OutQuad));
        hitSequence.Join(transform.DORotate(new Vector3(0, 0, 20f), 0.1f, RotateMode.Fast));
        hitSequence.Join(transform.DOShakeRotation(0.25f, 30f, 20, 90f, true));
        hitSequence.Join(targetTransform.DOScale(1.1f, 0.1f).SetEase(Ease.OutQuad));
        hitSequence.Join(targetTransform.DORotate(new Vector3(0, 0, -10f), 0.12f, RotateMode.Fast));
        hitSequence.Join(targetTransform.DOShakeRotation(0.20f, 15f, 15, 90f, true));

        hitSequence.Append(transform.DOScale(1f, 0.1f).SetEase(Ease.OutQuad));
        hitSequence.Join(transform.DORotate(Vector3.zero, 0.1f));
        hitSequence.Join(targetTransform.DOScale(1f, 0.1f).SetEase(Ease.OutQuad));
        hitSequence.Join(targetTransform.DORotate(Vector3.zero, 0.1f));

        yield return hitSequence.WaitForCompletion();

        attackManager.PerformAttack(attacker, defender);

        if (defender.BattleState.CurrentHP <= 0 &&
            defender.BattleCardView.CardRoot.TryGetComponent<CardMovemantScript>(out var card))
        {
            defender.BattleCardView.ResetBattleStyle();
            StartCoroutine(card.MoveCardTransformToAnotherField(playerResetStack, playerResetStack.childCount, transform));
            gameManager.CurrentGame.PlayerFieldListController.CardControllers.Remove(defender);
        }

        yield return transform.DOMove(startPosition, 0.5f).SetEase(Ease.OutQuad).WaitForCompletion();

        transform.SetParent(enemyFieldTransform);
        transform.SetSiblingIndex(siblingIndex);

        enemyFieldTransform.GetComponent<HorizontalLayoutGroup>().enabled = true;
    }
}
