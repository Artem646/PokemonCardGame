using UnityEngine;

public class AnimationTestBench : MonoBehaviour
{
    [SerializeField] private CardSlot attackerSlot;
    [SerializeField] private CardSlot defenderSlot;
    [SerializeField] private GameObject cardPrefab3D;
    [SerializeField] private AttackManager attackManager;

    public string attackerCardId;
    public string defenderCardId;

    public int selectedAbilityIndex;
    [HideInInspector] public string[] availableAbilities = new string[4];

    private BattleCardController attackerController;
    private BattleCardController defenderController;

    private async void Start()
    {
        await CardRepository.Instance.LoadGameCards();
        CardControllerFactory.Init(prefab3D: cardPrefab3D);
        SpawnCards();
    }

    public void SpawnCards()
    {
        attackerController?.RemoveFromContainer();
        defenderController?.RemoveFromContainer();

        CardModel attackerModel = CardRepository.Instance.GetGameCardModelById(int.Parse(attackerCardId));
        CardModel defenderModel = CardRepository.Instance.GetGameCardModelById(int.Parse(defenderCardId));

        if (attackerModel == null || defenderModel == null) return;

        attackerController = CardControllerFactory.Create<BattleCardController>(attackerModel);
        defenderController = CardControllerFactory.Create<BattleCardController>(defenderModel);

        attackerSlot.AddCardInStartGame(attackerController);
        defenderSlot.AddCardInStartGame(defenderController);
    }

    public void RunAnimation()
    {
        if (attackerController == null || defenderController == null) return;

        attackerController.SelectAbilityByIndex(selectedAbilityIndex);
        string abilityName = availableAbilities[selectedAbilityIndex];
        BaseAttackAnimation animation = AttackAnimationRegistry.GetAnimationForAbility(abilityName);

        StartCoroutine(animation.PlayAnimation(attackerController, defenderController,
            attackManager, (pos, dmg) => { })
        );
    }
}