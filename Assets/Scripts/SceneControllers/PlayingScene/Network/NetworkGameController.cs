using Fusion;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkGameController : NetworkBehaviour
{
    private GameManager gameManager;
    private AttackManager attackManager;

    [Networked] public PlayerRef FirstPlayerRef { get; private set; }
    [Networked] public PlayerRef SecondPlayerRef { get; private set; }

    [Networked] public NetworkString<_32> FirstPlayerName { get; private set; }
    [Networked] public NetworkString<_32> SecondPlayerName { get; private set; }

    [Networked] public NetworkString<_512> FirstPlayerDeckCsv { get; private set; }
    [Networked] public NetworkString<_512> SecondPlayerDeckCsv { get; private set; }

    [Networked] public bool FirstPlayerReady { get; private set; }
    [Networked] public bool SecondPlayerReady { get; private set; }

    [Networked] public bool IsFirstPlayerGameFullLoaded { get; set; }
    [Networked] public bool IsSecondPlayerGameFullLoaded { get; set; }

    [Networked] public bool IsGameStarted { get; set; }

    [Networked, Capacity(5)] public NetworkArray<int> FirstPlayerHand { get; }
    [Networked, Capacity(3)] public NetworkArray<NetworkCardData> FirstPlayerField { get; }
    [Networked, Capacity(5)] public NetworkArray<int> FirstPlayerReset { get; }

    [Networked, Capacity(5)] public NetworkArray<int> SecondPlayerHand { get; }
    [Networked, Capacity(3)] public NetworkArray<NetworkCardData> SecondPlayerField { get; }
    [Networked, Capacity(5)] public NetworkArray<int> SecondPlayerReset { get; }

    private ChangeDetector changeDetector;

    public int[] prevFirstPlayerHand;
    public NetworkCardData[] prevFirstPlayerField;
    public int[] prevFirstPlayerReset;

    public int[] prevSecondPlayerHand;
    public NetworkCardData[] prevSecondPlayerField;
    public int[] prevSecondPlayerReset;

    private HashSet<BattleCardController> cardsCurrentlyBeingAttacked = new();

    public bool IsFirstPlayer => Runner.LocalPlayer == FirstPlayerRef;
    public bool IsSecondPlayer => Runner.LocalPlayer == SecondPlayerRef;

    public void FindObjects()
    {
        gameManager = FindAnyObjectByType<GameManager>();
        attackManager = FindAnyObjectByType<AttackManager>();
    }

    public override void Spawned()
    {
        changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
        bool isSpectator = ConnectionConfig.IsSpectator;

        if (HasStateAuthority)
        {
            IsGameStarted = true;

            if (!isSpectator)
            {
                AssignPlayerRole(Runner.LocalPlayer);
                FirstPlayerName = UserSession.Instance.ActiveUser.userData.userName;
                FirstPlayerDeckCsv = SelectedDeckManager.GetSelectedDeckCsv();
                FirstPlayerReady = true;
            }
        }
        else
        {
            if (!isSpectator)
            {
                RpcRequestAssignRole(Runner.LocalPlayer);
                RpcSubmitName(UserSession.Instance.ActiveUser.userData.userName);
                RpcSubmitDeck(SelectedDeckManager.GetSelectedDeckCsv());
            }
        }

        prevFirstPlayerHand = new int[FirstPlayerHand.Length];
        prevFirstPlayerField = new NetworkCardData[FirstPlayerField.Length];
        prevFirstPlayerReset = new int[FirstPlayerReset.Length];
        prevSecondPlayerHand = new int[SecondPlayerHand.Length];
        prevSecondPlayerField = new NetworkCardData[SecondPlayerField.Length];
        prevSecondPlayerReset = new int[SecondPlayerReset.Length];

        SyncPrevState();

        NotificationManager.ShowNotification("Синхронизация данных...", NotificationType.Info);
    }

    private void SyncPrevState()
    {
        for (int i = 0; i < FirstPlayerHand.Length; i++) prevFirstPlayerHand[i] = FirstPlayerHand[i];
        for (int i = 0; i < FirstPlayerField.Length; i++) prevFirstPlayerField[i] = FirstPlayerField[i];
        for (int i = 0; i < FirstPlayerReset.Length; i++) prevFirstPlayerReset[i] = FirstPlayerReset[i];
        for (int i = 0; i < SecondPlayerHand.Length; i++) prevSecondPlayerHand[i] = SecondPlayerHand[i];
        for (int i = 0; i < SecondPlayerField.Length; i++) prevSecondPlayerField[i] = SecondPlayerField[i];
        for (int i = 0; i < SecondPlayerReset.Length; i++) prevSecondPlayerReset[i] = SecondPlayerReset[i];
    }

    public override void Render()
    {
        foreach (string change in changeDetector.DetectChanges(this))
        {
            switch (change)
            {
                case nameof(FirstPlayerHand): HandleHandChanges(true); break;
                case nameof(SecondPlayerHand): HandleHandChanges(false); break;

                case nameof(FirstPlayerField): HandleFieldChanges(true); break;
                case nameof(SecondPlayerField): HandleFieldChanges(false); break;

                case nameof(FirstPlayerReset): SyncSimpleArray(FirstPlayerReset, prevFirstPlayerReset); break;
                case nameof(SecondPlayerReset): SyncSimpleArray(SecondPlayerReset, prevSecondPlayerReset); break;
            }
        }
    }

    private void HandleHandChanges(bool isFirstPlayer)
    {
        NetworkArray<int> currentHandArray = isFirstPlayer ? FirstPlayerHand : SecondPlayerHand;
        int[] prevHandArray = isFirstPlayer ? prevFirstPlayerHand : prevSecondPlayerHand;

        bool isSpectator = ConnectionConfig.IsSpectator;

        for (int i = 0; i < currentHandArray.Length; i++)
        {
            if (currentHandArray[i] != prevHandArray[i])
            {
                int newCardId = currentHandArray[i];
                if (newCardId != 0)
                {
                    if (isSpectator)
                    {
                        CardSlot slot = GetVisualSlot(isFirstPlayer, i, "hand");
                        if (slot != null && slot.IsEmpty)
                        {
                            Debug.Log($"Спавним карту в руке: {newCardId}");
                            SpawnSimpleCard(newCardId, slot);
                        }
                    }
                }

                prevHandArray[i] = newCardId;
            }
        }
    }

    private async void HandleFieldChanges(bool isFirstPlayer)
    {
        while (FindAnyObjectByType<GameLoadingSceneController>() != null)
            await Task.Yield();

        NetworkArray<NetworkCardData> currentFieldArray = isFirstPlayer ? FirstPlayerField : SecondPlayerField;
        NetworkCardData[] prevFieldArray = isFirstPlayer ? prevFirstPlayerField : prevSecondPlayerField;

        bool isMine = IsLocalPlayersCard(isFirstPlayer);

        for (int i = 0; i < currentFieldArray.Length; i++)
        {
            NetworkCardData oldCardState = prevFieldArray[i];
            NetworkCardData newCardState = currentFieldArray[i];

            if (!oldCardState.isActive && newCardState.isActive && !isMine)
                StartCoroutine(OnCardPlayedRoutine(newCardState.originalHandSlot, i, isFirstPlayer));

            if (oldCardState.isActive && newCardState.isActive && newCardState.attackCounter > oldCardState.attackCounter)
                StartCoroutine(OnCardAttackedRoutine(i, newCardState.lastUsedAbilityIndex, newCardState.lastTargetSlotIndex, isFirstPlayer));

            if (oldCardState.isActive && newCardState.health != oldCardState.health)
            {
                BattleCardController targetController = GetControllerAtFieldSlot(isFirstPlayer, i);
                if (targetController != null)
                {
                    if (targetController.BattleState.CurrentHP != newCardState.health)
                    {
                        if (newCardState.health < targetController.BattleState.CurrentHP)
                            targetController.BattleState.ApplyDamage(targetController.BattleState.CurrentHP - newCardState.health);
                    }
                }
            }

            if (oldCardState.isActive && newCardState.isActive && oldCardState.cardId != newCardState.cardId)
                StartCoroutine(OnCardEvolvedRoutine(i, isFirstPlayer));

            if (oldCardState.isActive && !newCardState.isActive)
                StartCoroutine(OnCardDiedRoutine(i, isFirstPlayer));

            prevFieldArray[i] = newCardState;
        }
    }

    private void SyncSimpleArray(NetworkArray<int> networkArray, int[] prevArray)
    {
        for (int i = 0; i < networkArray.Length; i++)
            prevArray[i] = networkArray[i];
    }

    private bool IsLocalPlayersCard(bool isFirstPlayer)
    {
        if (ConnectionConfig.IsSpectator) return false;
        return (isFirstPlayer && IsFirstPlayer) || (!isFirstPlayer && IsSecondPlayer);
    }

    private BattleCardController GetControllerAtFieldSlot(bool isFirstPlayer, int index)
    {
        bool isBottom = (ConnectionConfig.IsSpectator || IsFirstPlayer) ? isFirstPlayer : !isFirstPlayer;
        return isBottom ? gameManager.CurrentGame.PlayerFieldControllers[index] : gameManager.CurrentGame.EnemyFieldControllers[index];
    }

    private CardSlot GetVisualSlot(bool isFirstPlayer, int slotIndex, string zone)
    {
        bool isBottom = (ConnectionConfig.IsSpectator || IsFirstPlayer) ? isFirstPlayer : !isFirstPlayer;
        if (zone == "hand") return isBottom ? gameManager.PlayerHandManager.HandSlots[slotIndex] : gameManager.EnemyHandManager.HandSlots[slotIndex];
        if (zone == "field") return isBottom ? gameManager.PlayerFieldManager.FieldSlots[slotIndex] : gameManager.EnemyFieldManager.FieldSlots[slotIndex];
        if (zone == "reset") return isBottom ? gameManager.PlayerResetSlots[slotIndex] : gameManager.EnemyResetSlots[slotIndex];
        return null;
    }

    private IEnumerator OnCardPlayedRoutine(int handSlotIndex, int fieldSlotIndex, bool isFirstPlayer)
    {
        CardSlot handSlot = GetVisualSlot(isFirstPlayer, handSlotIndex, "hand");
        if (handSlot == null || handSlot.CurrentCard == null) yield break;
        Transform cardTransform = handSlot.CurrentCard.transform;

        if (cardTransform.TryGetComponent<CardMovemantScript>(out var card) &&
            cardTransform.TryGetComponent<CardControllerLink>(out var link))
        {
            CardSlot fieldSlot = GetVisualSlot(isFirstPlayer, fieldSlotIndex, "field");
            yield return card.MoveCardTransformToField(fieldSlot);
            handSlot.Clear();

            UpdateGameControllerArray(null, isFirstPlayer, handSlotIndex, "hand");
            UpdateGameControllerArray(link.Controller, isFirstPlayer, fieldSlotIndex, "field");

            link.Controller.MarkAsPlayedInThisTurn(IsLocalPlayersCard(isFirstPlayer) ? CardOwner.Player : CardOwner.Enemy);
        }
    }

    private IEnumerator OnCardAttackedRoutine(int attackerSlotIndex, int attackerAbilityIndex, int defenderSlotIndex, bool isFirstPlayerAttacking)
    {
        CardSlot attackerSlot = GetVisualSlot(isFirstPlayerAttacking, attackerSlotIndex, "field");
        CardSlot defenderSlot = GetVisualSlot(!isFirstPlayerAttacking, defenderSlotIndex, "field");
        if (attackerSlot.CurrentCard == null || defenderSlot.CurrentCard == null) yield break;

        Transform cardTransform = attackerSlot.CurrentCard.transform;
        Transform targetTransform = defenderSlot.CurrentCard.transform;

        BattleCardController attacker = cardTransform.GetComponent<CardControllerLink>().Controller;
        BattleCardController defender = targetTransform.GetComponent<CardControllerLink>().Controller;

        attacker.SetSelectedAbility(attackerAbilityIndex);

        cardsCurrentlyBeingAttacked.Add(defender);

        if (attacker != null && defender != null)
        {
            CardAttackExecutor attackExecutor = attacker.BattleCardView.CardRoot.GetComponent<CardAttackExecutor>();
            attackExecutor.ExecuteNetworkAttack(attacker, defender);
        }

        cardsCurrentlyBeingAttacked.Remove(defender);
    }

    private IEnumerator OnCardEvolvedRoutine(int attackerSlotIndex, bool isFirstPlayer)
    {
        CardSlot attackerSlot = GetVisualSlot(isFirstPlayer, attackerSlotIndex, "field");

        Transform cardTransform = attackerSlot.CurrentCard.transform;
        BattleCardController attackerOldCard = cardTransform.GetComponent<CardControllerLink>().Controller;

        CardAttackExecutor cardAttackExecutor = attackerOldCard.BattleCardView.CardRoot.GetComponent<CardAttackExecutor>();
        yield return cardAttackExecutor.EvolutionRoutine(attackerOldCard, IsLocalPlayersCard(isFirstPlayer));
    }

    private IEnumerator OnCardDiedRoutine(int fieldSlotIndex, bool isFirstPlayer)
    {
        CardSlot fieldSlot = GetVisualSlot(isFirstPlayer, fieldSlotIndex, "field");
        BattleCardController deadCard = fieldSlot.CurrentCard.GetComponent<CardControllerLink>().Controller;

        yield return new WaitUntil(() => !cardsCurrentlyBeingAttacked.Contains(deadCard));

        deadCard.BattleState.CurrentHP = 0;
        deadCard.BattleCardView.ResetBattleStyle();

        for (int i = 0; i < FirstPlayerReset.Length; i++)
        {
            CardSlot resetSlot = GetVisualSlot(isFirstPlayer, i, "reset");
            if (resetSlot.IsEmpty)
            {
                deadCard.BattleCardView.CardRoot.GetComponent<BoxCollider>().enabled = false;
                StartCoroutine(resetSlot.PlaceCardWithMoveAndFlip(deadCard));
                break;
            }
        }

        fieldSlot.Clear();
        UpdateGameControllerArray(null, isFirstPlayer, fieldSlotIndex, "field");
    }

    private void UpdateGameControllerArray(BattleCardController controller, bool isFirstPlayer, int index, string zone)
    {
        bool isBottom = (ConnectionConfig.IsSpectator || IsFirstPlayer) ? isFirstPlayer : !isFirstPlayer;
        if (zone == "hand")
        {
            if (isBottom) gameManager.CurrentGame.PlayerHandControllers[index] = controller;
            else gameManager.CurrentGame.EnemyHandControllers[index] = controller;
        }
        if (zone == "field")
        {
            if (isBottom) gameManager.CurrentGame.PlayerFieldControllers[index] = controller;
            else gameManager.CurrentGame.EnemyFieldControllers[index] = controller;
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RpcSyncInitialHand(PlayerRef player, int[] myHandIds)
    {
        NetworkArray<int> myNetworkHand = (player == FirstPlayerRef) ? FirstPlayerHand : SecondPlayerHand;
        for (int i = 0; i < myNetworkHand.Length; i++)
            myNetworkHand.Set(i, myHandIds[i]);
    }

    public void RequestPlayCard(int handSlotIndex, int fieldSlotIndex, int cardId)
    {
        RpcPlayCard(handSlotIndex, fieldSlotIndex, cardId, Runner.LocalPlayer);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RpcPlayCard(int handSlotIndex, int fieldSlotIndex, int cardId, PlayerRef initiator)
    {
        NetworkCardData newCard = new()
        {
            isActive = true,
            cardId = cardId,
            health = 100,
            attackCounter = 0,
            originalHandSlot = handSlotIndex
        };

        if (initiator == FirstPlayerRef)
        {
            FirstPlayerField.Set(fieldSlotIndex, newCard);
            FirstPlayerHand.Set(handSlotIndex, 0);
        }
        else
        {
            SecondPlayerField.Set(fieldSlotIndex, newCard);
            SecondPlayerHand.Set(handSlotIndex, 0);
        }
    }

    public void RequestAttackAnimation(int attackerSlotIndex, int abilityIndex, int defenderSlotIndex)
    {
        RpcAttackAnimation(attackerSlotIndex, abilityIndex, defenderSlotIndex, Runner.LocalPlayer);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RpcAttackAnimation(int attackerSlotIndex, int attackerAbilityIndex, int defenderSlotIndex, PlayerRef initiator)
    {
        bool isFirst = initiator == FirstPlayerRef;

        NetworkArray<NetworkCardData> attackerField = isFirst ? FirstPlayerField : SecondPlayerField;
        NetworkCardData attackerData = attackerField[attackerSlotIndex];

        attackerData.attackCounter++;
        attackerData.lastTargetSlotIndex = defenderSlotIndex;
        attackerData.lastUsedAbilityIndex = attackerAbilityIndex;
        attackerField.Set(attackerSlotIndex, attackerData);
    }

    public void RequestApplyDamage(int attackerSlotIndex, int abilityIndex, int defenderSlotIndex, int recoilDamage = 0)
    {
        RpcApplyDamage(attackerSlotIndex, abilityIndex, defenderSlotIndex, recoilDamage, Runner.LocalPlayer);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RpcApplyDamage(int attackerSlotIndex, int attackerAbilityIndex, int defenderSlotIndex, int recoilDamage, PlayerRef initiator)
    {
        bool isFirst = initiator == FirstPlayerRef;

        NetworkArray<NetworkCardData> attackerField = isFirst ? FirstPlayerField : SecondPlayerField;
        NetworkArray<NetworkCardData> defenderField = isFirst ? SecondPlayerField : FirstPlayerField;

        NetworkCardData attackerData = attackerField[attackerSlotIndex];
        NetworkCardData defenderData = defenderField[defenderSlotIndex];

        int damage = attackManager.CalculateDamageInNetworkGame(attackerData, attackerAbilityIndex, defenderData);

        defenderData.health -= damage;

        if (defenderData.health <= 0)
        {
            defenderData.health = 0;
            defenderData.isDead = true;

            attackerData.readyForEvolution = true;
            attackerField.Set(attackerSlotIndex, attackerData);
        }

        defenderField.Set(defenderSlotIndex, defenderData);

        if (recoilDamage > 0)
        {
            attackerData.health -= recoilDamage;
            if (attackerData.health <= 0) attackerData.health = 1;
            attackerField.Set(attackerSlotIndex, attackerData);
        }
    }

    public void RequestClearDeadCards() => RpcClearDeadCards();

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RpcClearDeadCards()
    {
        for (int i = 0; i < FirstPlayerField.Length; i++)
        {
            if (FirstPlayerField[i].isActive && FirstPlayerField[i].isDead)
            {
                MoveCardToReset(FirstPlayerReset, FirstPlayerField[i].cardId);
                FirstPlayerField.Set(i, new NetworkCardData());
            }

            if (SecondPlayerField[i].isActive && SecondPlayerField[i].isDead)
            {
                MoveCardToReset(SecondPlayerReset, SecondPlayerField[i].cardId);
                SecondPlayerField.Set(i, new NetworkCardData());
            }
        }
    }

    private void MoveCardToReset(NetworkArray<int> resetArray, int cardId)
    {
        for (int i = 0; i < resetArray.Length; i++)
        {
            if (resetArray[i] == 0)
            {
                resetArray.Set(i, cardId);
                break;
            }
        }
    }

    public void RequestEvolutionCard(int attackerSlotIndex, int newCardId)
    {
        RpcEvolutionCard(attackerSlotIndex, newCardId, Runner.LocalPlayer);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RpcEvolutionCard(int attackerSlotIndex, int newCardId, PlayerRef initiator)
    {
        bool isFirst = initiator == FirstPlayerRef;

        NetworkArray<NetworkCardData> attackerField = isFirst ? FirstPlayerField : SecondPlayerField;
        NetworkCardData attackerData = attackerField[attackerSlotIndex];

        if (attackerData.isActive && attackerData.readyForEvolution)
        {
            NetworkCardData newCard = new()
            {
                isActive = true,
                cardId = newCardId,
                health = 100,
                originalHandSlot = attackerData.originalHandSlot
            };

            attackerField.Set(attackerSlotIndex, newCard);
        }
    }

    public IEnumerator RestoreBoardRoutine()
    {
        yield return new WaitUntil(() => gameManager != null && gameManager.PlayerFieldManager != null && gameManager.EnemyFieldManager != null);

        for (int i = 0; i < FirstPlayerHand.Length; i++)
        {
            if (FirstPlayerHand[i] != 0) SpawnSimpleCard(FirstPlayerHand[i], GetVisualSlot(true, i, "hand"));
            if (SecondPlayerHand[i] != 0) SpawnSimpleCard(SecondPlayerHand[i], GetVisualSlot(false, i, "hand"));
        }

        for (int i = 0; i < FirstPlayerField.Length; i++)
        {
            if (FirstPlayerField[i].isActive) SpawnFieldCard(FirstPlayerField[i], GetVisualSlot(true, i, "field"));
            if (SecondPlayerField[i].isActive) SpawnFieldCard(SecondPlayerField[i], GetVisualSlot(false, i, "field"));
        }

        for (int i = 0; i < FirstPlayerReset.Length; i++)
        {
            if (FirstPlayerReset[i] != 0) SpawnSimpleCard(FirstPlayerReset[i], GetVisualSlot(true, i, "reset"));
            if (SecondPlayerReset[i] != 0) SpawnSimpleCard(SecondPlayerReset[i], GetVisualSlot(false, i, "reset"));
        }

        SyncPrevState();

        Debug.Log("Стол полностью восстановлен.");
    }

    private void SpawnSimpleCard(int cardId, CardSlot slot)
    {
        BattleCardController controller = CardRepository.Instance.GetBattleCardControllerById(cardId);

        if (slot.type == FieldSlotType.SelfHandSlot || slot.type == FieldSlotType.EnemyHandSlot)
            controller.BattleState.SetHP(100);
        else if (slot.type == FieldSlotType.SelfResetStack || slot.type == FieldSlotType.EnemyResetStack)
            controller.BattleState.SetHP(0);

        slot.AddCardInStartGame(controller);
        RegisterInManager(controller, slot);
    }

    private void SpawnFieldCard(NetworkCardData cardData, CardSlot slot)
    {
        BattleCardController controller = CardRepository.Instance.GetBattleCardControllerById(cardData.cardId);

        controller.BattleState.SetHP(cardData.health);

        slot.AddCardInStartGame(controller);
        RegisterInManager(controller, slot);
    }

    private void RegisterInManager(BattleCardController controller, CardSlot slot)
    {
        int i = (int)slot.indexSlotInField;
        if (slot.transform.IsChildOf(gameManager.PlayerFieldManager.transform)) gameManager.CurrentGame.PlayerFieldControllers[i] = controller;
        else if (slot.transform.IsChildOf(gameManager.EnemyFieldManager.transform)) gameManager.CurrentGame.EnemyFieldControllers[i] = controller;
        else if (slot.transform.IsChildOf(gameManager.PlayerHandManager.transform)) gameManager.CurrentGame.PlayerHandControllers[i] = controller;
        else if (slot.transform.IsChildOf(gameManager.EnemyHandManager.transform)) gameManager.CurrentGame.EnemyHandControllers[i] = controller;
    }

    public void AssignPlayerRole(PlayerRef player)
    {
        if (FirstPlayerRef == default) FirstPlayerRef = player;
        else if (SecondPlayerRef == default) SecondPlayerRef = player;
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RpcRequestAssignRole(PlayerRef playerClient, RpcInfo info = default)
    {
        AssignPlayerRole(playerClient);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RpcSubmitName(string name, RpcInfo info = default) => SecondPlayerName = name;

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RpcSubmitDeck(string csv, RpcInfo info = default)
    {
        SecondPlayerDeckCsv = csv;
        SecondPlayerReady = true;
    }

    public void SetPlayerLocalGameReady()
    {
        if (IsFirstPlayer) RpcSetGameReadyStatus(true, false);
        else if (IsSecondPlayer) RpcSetGameReadyStatus(false, true);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RpcSetGameReadyStatus(bool isFirstPlayer, bool isSecondPlayer)
    {
        if (isFirstPlayer) IsFirstPlayerGameFullLoaded = true;
        if (isSecondPlayer) IsSecondPlayerGameFullLoaded = true;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RpcKickForStartedGame(int targetPlayerId)
    {
        if (Runner.LocalPlayer.PlayerId == targetPlayerId)
        {
            NotificationManager.ShowNotification("Игра уже началась! Вы можете зайти только как зритель.", NotificationType.Error);
            StartCoroutine(ExitToMenuRoutine());
        }
    }

    private IEnumerator ExitToMenuRoutine()
    {
        yield return new WaitForSeconds(2f);
        Task disconnectTask = NetworkRunnerController.Instance.DisconnectAndRejoinLobby();
        yield return new WaitUntil(() => disconnectTask.IsCompleted);
        SceneManager.LoadScene("RoomManagerScene");
    }

    public List<int> GetPlayerDeckIds()
    {
        return IsFirstPlayer
            ? SelectedDeckManager.ParseCsv(FirstPlayerDeckCsv.Value)
            : SelectedDeckManager.ParseCsv(SecondPlayerDeckCsv.Value);
    }

    public List<int> GetEnemyDeckIds()
    {
        return IsFirstPlayer
            ? SelectedDeckManager.ParseCsv(SecondPlayerDeckCsv.Value)
            : SelectedDeckManager.ParseCsv(FirstPlayerDeckCsv.Value);
    }
}