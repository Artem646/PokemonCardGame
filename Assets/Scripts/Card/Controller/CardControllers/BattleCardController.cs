using UnityEngine;
using UnityEngine.UI;

public class BattleCardController : BaseCardController
{
    public IBattleCardView BattleCardView { get; set; }
    public bool CanAttack { get; private set; } = false;

    public BattleCardController(CardModel model, IBattleCardView view)
        : base(model, view)
    {
        BattleCardView = view;

        if (view.CardRoot.TryGetComponent(out CardClickScript clickScript))
            clickScript.OnCardClicked += OnCardElementClicked;
    }

    private void OnCardElementClicked(CardClickScript cardClick)
    {
        BattleCardController cloneController = CardControllerFactory.Create<BattleCardController>(CardModel, faceDown: false);
        IBattleCardView cloneView = cloneController?.BattleCardView;
        if (cloneView != null)
            CardOverlayManager.Instance?.ShowBattleCard(BattleCardView, cloneView);
    }

    // public void ChangeAttackState(bool canAttack)
    // {
    //     CanAttack = canAttack;
    // }

    // public void HighlightCard()
    // {
    //     GameObject highlighting = BattleCardView.CardRoot.transform.Find("Highlighted").gameObject;
    //     highlighting.GetComponent<Image>().color = Color.green;
    //     BattleCardView.CardRoot.transform.Find("Highlighted").gameObject.SetActive(true);
    // }

    // public void DeHighlightCard()
    // {
    //     BattleCardView.CardRoot.transform.Find("Highlighted").gameObject.SetActive(false);
    // }

    // public void ShowAttackTargets(TypeChart chart, PokemonElement attackerElement)
    // {
    //     float multiplier = chart.GetMultiplier(attackerElement, CardModel.mainElement);
    //     GameObject highlighting = BattleCardView.CardRoot.transform.Find("Highlighted").gameObject;
    //     // if (multiplier == 1f)
    //     // {
    //     //     highlighting.GetComponent<Image>().color = Color.yellow;
    //     // }
    //     // else if (multiplier == 2f || multiplier == 0.5f)
    //     // {
    //     //     highlighting.GetComponent<Image>().color = Color.red;
    //     // }

    //     if (multiplier == 2f)
    //     {
    //         highlighting.GetComponent<Image>().color = Color.cyan;
    //     }
    //     else if (multiplier == 1f || multiplier == 0.5f)
    //     {
    //         highlighting.GetComponent<Image>().color = Color.red;
    //     }

    //     highlighting.SetActive(true);
    // }

    // public void ClearAttackTargets()
    // {
    //     BattleCardView.CardRoot.transform.Find("Highlighted").gameObject.SetActive(false);
    // }

    public override void AddToContainer(object container)
    {
        if (container is Transform handContainer)
            BattleCardView.CardRoot.transform.SetParent(handContainer, false);
    }

    public override void RemoveFromContainer()
    {
        Object.Destroy(BattleCardView.CardRoot);
    }
}
