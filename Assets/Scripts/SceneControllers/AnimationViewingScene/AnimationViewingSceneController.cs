using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AnimationViewingSceneController : MonoBehaviour
{
    [SerializeField] private AnimationFieldManager playerFieldManager, enemyFieldManager;
    [SerializeField] private Button exitButton;

    public AnimationFieldManager PlayerFieldManager => playerFieldManager;
    public AnimationFieldManager EnemyFieldManager => enemyFieldManager;

    const float DELAY_BETWEEN_CARDS = 0.2f;

    private void Start()
    {
        StartCoroutine(DealHandsRoutine());
        RegisterCallbacks();
    }

    private IEnumerator DealHandsRoutine()
    {
        int enemyCardId = 1;
        yield return DealCardsToHandsRoutine(SelectedCardModelStorage.SelectedCardModel.id, enemyCardId);
    }

    private IEnumerator DealCardsToHandsRoutine(int playerCardId, int enemyCardId)
    {
        BattleCardController playerCardController = CardRepository.Instance.GetBattleCardControllerById(playerCardId);
        Transform playerCardTransform = playerCardController.BattleCardView.CardRoot.transform;

        playerCardTransform.GetComponent<CardMovemantScript>().enabled = false;
        playerCardTransform.GetComponent<CardClickScript>().enabled = false;
        playerCardTransform.GetComponent<CardBattleState>().enabled = false;

        playerCardTransform.Find("FrontUIContainer/Footer/Abilities/FirstAbility/FirstAbilityPlateMesh").GetComponent<BoxCollider>().enabled = false;
        playerCardTransform.Find("FrontUIContainer/Footer/Abilities/SecondAbility/SecondAbilityPlateMesh").GetComponent<BoxCollider>().enabled = false;
        playerCardTransform.Find("FrontUIContainer/Footer/Abilities/ThirdAbility/ThirdAbilityPlateMesh").GetComponent<BoxCollider>().enabled = false;
        playerCardTransform.Find("FrontUIContainer/Footer/Abilities/FourthAbility/FourthAbilityPlateMesh").GetComponent<BoxCollider>().enabled = false;

        playerCardTransform.GetComponent<AnimationCardMovemantScript>().enabled = true;
        playerCardTransform.GetComponent<AnimationCardClickScript>().enabled = true;

        if (playerCardTransform.TryGetComponent<AnimationCardClickScript>(out var clickOnPlayerCardScript))
            clickOnPlayerCardScript.OnCardClicked += playerCardController.OnAnimationCardClicked;

        playerFieldManager.TryAddCardInStart(playerCardController);
        yield return new WaitForSeconds(DELAY_BETWEEN_CARDS);

        BattleCardController enemyCardController = CardRepository.Instance.GetBattleCardControllerById(enemyCardId);
        Transform enemyCardTransform = enemyCardController.BattleCardView.CardRoot.transform;

        enemyCardTransform.GetComponent<CardMovemantScript>().enabled = false;
        enemyCardTransform.GetComponent<CardClickScript>().enabled = false;
        enemyCardTransform.GetComponent<CardBattleState>().enabled = false;

        enemyCardTransform.Find("FrontUIContainer/Footer/Abilities/FirstAbility/FirstAbilityPlateMesh").GetComponent<BoxCollider>().enabled = false;
        enemyCardTransform.Find("FrontUIContainer/Footer/Abilities/SecondAbility/SecondAbilityPlateMesh").GetComponent<BoxCollider>().enabled = false;
        enemyCardTransform.Find("FrontUIContainer/Footer/Abilities/ThirdAbility/ThirdAbilityPlateMesh").GetComponent<BoxCollider>().enabled = false;
        enemyCardTransform.Find("FrontUIContainer/Footer/Abilities/FourthAbility/FourthAbilityPlateMesh").GetComponent<BoxCollider>().enabled = false;

        enemyCardTransform.GetComponent<AnimationCardMovemantScript>().enabled = true;
        enemyCardTransform.GetComponent<AnimationCardClickScript>().enabled = true;

        if (enemyCardTransform.TryGetComponent<AnimationCardClickScript>(out var clickOnEnemyCardScript))
            clickOnEnemyCardScript.OnCardClicked += enemyCardController.OnAnimationCardClicked;

        enemyFieldManager.TryAddCardInStart(enemyCardController);
        yield return new WaitForSeconds(DELAY_BETWEEN_CARDS);
    }

    private void RegisterCallbacks()
    {
        exitButton.onClick.AddListener(() => { SceneManager.LoadScene($"{SceneContext.PreviousDescriptionSceneName}"); });
    }
}