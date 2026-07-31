using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class AttackTargetingPanelController : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private BotTurnManager botTurnManager;

    [SerializeField] private Button attackButton;
    [SerializeField] private Sprite noCardOnSlotSprite;

    [SerializeField] private List<Button> playerFieldCardsButtons;
    [SerializeField] private List<Image> playerFieldCardsImages;
    [SerializeField] private List<GameObject> playerFieldCardsGrayFilters;
    [SerializeField] private List<GameObject> playerFieldCardsCheckmarks;

    [SerializeField] private List<Button> enemyFieldCardsButtons;
    [SerializeField] private List<Image> enemyFieldCardsImages;
    [SerializeField] private List<GameObject> enemyFieldCardsCheckmarks;

    [SerializeField] private CanvasGroup attackTargetingPanelCanvasGroup;

    private GameType currentGameType = GameTypeConfig.CurrentType;

    private Color32 notSelectedCardFrameColor = new(247, 234, 117, 255);
    private Color32 selectedCardFrameColor = new(77, 217, 87, 255);

    private int selectedPlayerCardSlotIndex = -1;
    private int selectedEnemyCardSlotIndex = -1;

    private void Start()
    {
        if (GameTypeConfig.CurrentType == GameType.Bot)
            botTurnManager.OnPhaseChanged += HandlePhaseChanged;
        else if (GameTypeConfig.CurrentType == GameType.Multiplayer)
        {
            NetworkGameTurnManager networkGameTurnManager = FindAnyObjectByType<NetworkGameTurnManager>();
            networkGameTurnManager.OnPhaseChanged += HandlePhaseChanged;
        }

        for (int i = 0; i < 3; i++)
        {
            int index = i;
            playerFieldCardsButtons[i].onClick.AddListener(() => OnPlayerCardSlotClicked(index));
            enemyFieldCardsButtons[i].onClick.AddListener(() => OnEnemyCardSlotClicked(index));
        }

        attackButton.onClick.AddListener(OnAttackButtonClicked);
        ResetSelection();

        attackTargetingPanelCanvasGroup.alpha = 0;
        attackTargetingPanelCanvasGroup.interactable = false;
    }

    private void HandlePhaseChanged(TurnPhase phase)
    {
        if (gameManager.IsMyTurn && phase == TurnPhase.Attack)
        {
            attackTargetingPanelCanvasGroup.alpha = 1;
            attackTargetingPanelCanvasGroup.interactable = true;
            UpdatePanel();
        }
        else
        {
            ResetSelection();
            attackTargetingPanelCanvasGroup.interactable = false;
            attackTargetingPanelCanvasGroup.alpha = 0;
        }
    }

    public void UpdatePanel()
    {
        if (gameManager.CurrentGame.PlayerFieldControllers == null ||
            gameManager.CurrentGame.EnemyFieldControllers == null)
            return;

        BattleCardController[] playerCards = gameManager.CurrentGame.PlayerFieldControllers;
        BattleCardController[] enemyCards = gameManager.CurrentGame.EnemyFieldControllers;

        for (int i = 0; i < 3; i++)
        {
            if (playerCards[i] != null)
            {
                playerFieldCardsImages[i].sprite = Resources.Load<Sprite>($"Sprites/PokemonImages/{playerCards[i].CardModel.imageName}");
                bool canAttack = playerCards[i].CanAttack && playerCards[i].SelectedAbility != null && playerCards[i].SelectedAbility.IsAttackType();
                playerFieldCardsGrayFilters[i].SetActive(!canAttack);
                playerFieldCardsButtons[i].interactable = true;
            }
            else
            {
                playerFieldCardsImages[i].sprite = noCardOnSlotSprite;
                playerFieldCardsGrayFilters[i].SetActive(false);
                playerFieldCardsButtons[i].interactable = false;
            }

            if (enemyCards[i] != null)
            {
                enemyFieldCardsImages[i].sprite = Resources.Load<Sprite>($"Sprites/PokemonImages/{enemyCards[i].CardModel.imageName}");
                enemyFieldCardsButtons[i].interactable = selectedPlayerCardSlotIndex != -1;
            }
            else
            {
                enemyFieldCardsImages[i].sprite = noCardOnSlotSprite;
                enemyFieldCardsButtons[i].interactable = false;
            }
        }
    }

    private void OnPlayerCardSlotClicked(int index)
    {
        if (selectedPlayerCardSlotIndex == index)
        {
            BattleCardController playerCardController = gameManager.CurrentGame.PlayerFieldControllers[selectedPlayerCardSlotIndex];
            Transform playerCardTransform = playerCardController.BattleCardView.CardRoot.transform;
            GameObject playerCardFrame = playerCardTransform.Find("CardFrame").gameObject;
            playerCardFrame.GetComponent<MeshRenderer>().material.color = notSelectedCardFrameColor;

            if (selectedEnemyCardSlotIndex != -1)
            {
                BattleCardController enemyCardController = gameManager.CurrentGame.EnemyFieldControllers[selectedEnemyCardSlotIndex];
                Transform enemyCardTransform = enemyCardController.BattleCardView.CardRoot.transform;
                GameObject enemyCardFrame = enemyCardTransform.Find("CardFrame").gameObject;
                enemyCardFrame.SetActive(false);
            }

            selectedPlayerCardSlotIndex = -1;
            selectedEnemyCardSlotIndex = -1;
        }
        else
        {
            BattleCardController newPlayerCardController = gameManager.CurrentGame.PlayerFieldControllers[index];
            if (!CheckButtonForAttack(newPlayerCardController)) return;

            if (selectedPlayerCardSlotIndex != -1)
            {
                BattleCardController oldPlayerCardController = gameManager.CurrentGame.PlayerFieldControllers[selectedPlayerCardSlotIndex];
                Transform oldPlayerCardTransform = oldPlayerCardController.BattleCardView.CardRoot.transform;
                GameObject oldPlayerCardFrame = oldPlayerCardTransform.Find("CardFrame").gameObject;
                oldPlayerCardFrame.GetComponent<MeshRenderer>().material.color = notSelectedCardFrameColor;
            }

            if (selectedEnemyCardSlotIndex != -1)
            {
                BattleCardController enemyCardController = gameManager.CurrentGame.EnemyFieldControllers[selectedEnemyCardSlotIndex];
                Transform enemyCardTransform = enemyCardController.BattleCardView.CardRoot.transform;
                GameObject enemyCardFrame = enemyCardTransform.Find("CardFrame").gameObject;
                enemyCardFrame.SetActive(false);
            }

            Transform newPlayerCardTransform = newPlayerCardController.BattleCardView.CardRoot.transform;
            GameObject newPlayerCardFrame = newPlayerCardTransform.Find("CardFrame").gameObject;
            newPlayerCardFrame.GetComponent<MeshRenderer>().material.color = selectedCardFrameColor;

            selectedPlayerCardSlotIndex = index;
            selectedEnemyCardSlotIndex = -1;
        }

        UpdateCheckmarks();
        UpdatePanel();
        UpdateAttackButtonState();
    }

    private void OnEnemyCardSlotClicked(int index)
    {
        BattleCardController newEnemyCardController = gameManager.CurrentGame.EnemyFieldControllers[index];
        if (!IsTargetInReach(selectedPlayerCardSlotIndex, index)) return;

        if (selectedEnemyCardSlotIndex == index)
        {
            BattleCardController enemyCardController = gameManager.CurrentGame.EnemyFieldControllers[selectedEnemyCardSlotIndex];
            Transform enemyCardTransform = enemyCardController.BattleCardView.CardRoot.transform;
            GameObject enemyCardFrame = enemyCardTransform.Find("CardFrame").gameObject;
            enemyCardFrame.SetActive(false);

            selectedEnemyCardSlotIndex = -1;
        }
        else
        {
            if (selectedEnemyCardSlotIndex != -1)
            {
                BattleCardController oldEnemyCardController = gameManager.CurrentGame.EnemyFieldControllers[selectedEnemyCardSlotIndex];
                Transform oldEnemyCardTransform = oldEnemyCardController.BattleCardView.CardRoot.transform;
                GameObject oldEnemyCardFrame = oldEnemyCardTransform.Find("CardFrame").gameObject;
                oldEnemyCardFrame.SetActive(false);
            }

            Transform newEnemyCardTransform = newEnemyCardController.BattleCardView.CardRoot.transform;
            GameObject newEnemyCardFrame = newEnemyCardTransform.Find("CardFrame").gameObject;
            newEnemyCardFrame.GetComponent<MeshRenderer>().material.color = selectedCardFrameColor;
            newEnemyCardFrame.SetActive(true);

            selectedEnemyCardSlotIndex = index;
        }

        UpdateCheckmarks();
        UpdateAttackButtonState();
    }

    private void UpdateCheckmarks()
    {
        for (int i = 0; i < 3; i++)
        {
            playerFieldCardsCheckmarks[i].SetActive(i == selectedPlayerCardSlotIndex);
            enemyFieldCardsCheckmarks[i].SetActive(i == selectedEnemyCardSlotIndex);
        }
    }

    private void UpdateAttackButtonState()
    {
        attackButton.interactable = selectedPlayerCardSlotIndex != -1 && selectedEnemyCardSlotIndex != -1;
    }

    private void OnAttackButtonClicked()
    {
        BattleCardController attacker = gameManager.CurrentGame.PlayerFieldControllers[selectedPlayerCardSlotIndex];
        BattleCardController defender = gameManager.CurrentGame.EnemyFieldControllers[selectedEnemyCardSlotIndex];

        if (attacker != null && defender != null)
        {
            attackTargetingPanelCanvasGroup.alpha = 0;
            attackTargetingPanelCanvasGroup.interactable = false;

            CardAttackExecutor attackExecutor = attacker.BattleCardView.CardRoot.GetComponent<CardAttackExecutor>();
            attackExecutor.OnAnimationFinished += OnAttackAnimationFinished;

            if (currentGameType == GameType.Multiplayer)
            {
                NetworkGameController networkGameController = FindAnyObjectByType<NetworkGameController>();

                int playerCardSlot = selectedPlayerCardSlotIndex;
                int abilityIndex = attacker.SelectedAbility.AbilityIndex;
                int enemyCardSlot = selectedEnemyCardSlotIndex;

                networkGameController.RequestAttackAnimation(playerCardSlot, abilityIndex, enemyCardSlot);
            }
            else if (currentGameType == GameType.Bot)
                attackExecutor.StartCoroutine(attackExecutor.ExecuteAttackRoutine(attacker, defender));
        }
    }

    private void OnAttackAnimationFinished()
    {
        ResetSelection();
        if (gameManager.IsMyTurn)
        {
            attackTargetingPanelCanvasGroup.alpha = 1;
            attackTargetingPanelCanvasGroup.interactable = true;
            UpdatePanel();
        }
    }

    private void ResetSelection()
    {
        selectedPlayerCardSlotIndex = -1;
        selectedEnemyCardSlotIndex = -1;

        UpdateCheckmarks();
        UpdateAttackButtonState();
    }

    private bool IsTargetInReach(int attackerIndex, int defenderIndex)
    {
        if ((attackerIndex == 0 && defenderIndex == 2) || (attackerIndex == 2 && defenderIndex == 0))
        {
            NotificationManager.ShowNotification("Карты не достают до друг друга", NotificationType.Info);
            return false;
        }

        return true;
    }

    private bool CheckButtonForAttack(BattleCardController cardController)
    {
        if (cardController.BattleState.IsFresh)
        {
            NotificationManager.ShowNotification("Эта карта не может атаковать в этом раунде", NotificationType.Info);
            return false;
        }
        else if (cardController.BattleState.HasAttacked)
        {
            NotificationManager.ShowNotification("Эта карта уже атаковала в этом раунде", NotificationType.Info);
            return false;
        }
        else if (cardController.CanAttack && cardController.SelectedAbility == null)
        {
            NotificationManager.ShowNotification("Нужно обязательно выбрать атакующую способность", NotificationType.Info);
            return false;
        }
        else if (cardController.CanAttack && cardController.SelectedAbility != null && !cardController.SelectedAbility.IsAttackType())
        {
            NotificationManager.ShowNotification("Выбранная способность не является атакующей", NotificationType.Info);
            return false;
        }

        return true;
    }
}