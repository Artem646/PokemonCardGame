using UnityEngine;
using UnityEngine.UI;

public class BattleCardController : BaseCardController
{
    private readonly IBattleCardView battleCardView;
    public bool CanAttack { get; private set; } = false;

    public BattleCardController(CardModel model, IBattleCardView view)
        : base(model, view)
    {
        battleCardView = view;

        if (view.CardRootGameObject.TryGetComponent(out CardClickScript clickScript))
            clickScript.OnCardClicked += OnCardElementClicked;
    }

    public override void RegisterEvents() { }

    public override void UnregisterEvents() { }

    private void OnCardElementClicked(CardClickScript cardClick)
    {
        BattleCardController cloneController = CardControllerFactory.Create<BattleCardController>(CardModel, faceDown: false);
        IBattleCardView cloneView = cloneController?.battleCardView;
        if (cloneView != null)
            CardOverlayManager.Instance?.ShowBattleCard(battleCardView, cloneView);
    }

    public void ChangeAttackState(bool canAttack)
    {
        CanAttack = canAttack;
    }

    public void HighlightCard()
    {
        GameObject highlighting = battleCardView.CardRootGameObject.transform.Find("Highlighted").gameObject;
        highlighting.GetComponent<Image>().color = Color.green;
        battleCardView.CardRootGameObject.transform.Find("Highlighted").gameObject.SetActive(true);
    }

    public void DeHighlightCard()
    {
        battleCardView.CardRootGameObject.transform.Find("Highlighted").gameObject.SetActive(false);
    }

    public void ShowAttackTargets(TypeChart chart, PokemonElement attackerElement)
    {
        float multiplier = chart.GetMultiplier(attackerElement, CardModel.mainElement);
        GameObject highlighting = battleCardView.CardRootGameObject.transform.Find("Highlighted").gameObject;
        if (multiplier == 1f)
        {
            highlighting.GetComponent<Image>().color = Color.yellow;
        }
        else if (multiplier == 2f || multiplier == 0.5f)
        {
            highlighting.GetComponent<Image>().color = Color.red;
        }
        highlighting.SetActive(true);
    }

    public void ClearAttackTargets()
    {
        battleCardView.CardRootGameObject.transform.Find("Highlighted").gameObject.SetActive(false);
    }
}
