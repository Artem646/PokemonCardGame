using UnityEngine;

public class BattleCardView : CardViewBase, IBattleCardView
{
    public GameObject CardRoot { get; }
    public GameObject CardPrefab { get; }

    public BattleCardView(CardModel model, GameObject prefab3D)
        : base(model)
    {
        model.visualData = CardVisualRegistrySO.Instance.GetVisualData(model.id);

        CardPrefab = prefab3D;
        CardRoot = Object.Instantiate(prefab3D);
        CardRoot.name = model.titleKey;

        if (CardRoot.TryGetComponent<CardVisualTweaker>(out var tweaker))
            tweaker.visualData = model.visualData;

        BindData();
    }

    public override void BindData()
    {
        MeshRenderer meshRendererCardPlate = CardRoot.transform.Find("FrontUIContainer/CardPlate").GetComponent<MeshRenderer>();
        meshRendererCardPlate.materials[2].color = CardModel.colors.cardColor;
        Localizer.LocalizeCard3DTitleGameObject(CardRoot, "FrontUIContainer/Body/Title/TitleText", CardModel, "PokemonTitles");
        CardViewHelper.UpdateBody3D(CardRoot, CardModel);
        CardViewHelper.SetImages3D(CardRoot, CardModel);
        CardViewHelper.UpdateStats3D(CardRoot, CardModel);
        CardViewHelper.UpdateAbilities3D(CardRoot, CardModel);
        CardViewHelper.UpdateAbilityDamage3D(CardRoot, CardModel);
        CardViewHelper.BindHPWithBattleState3D(CardRoot, CardModel);
    }

    public void ApplyFaceDownState(bool faceDown) { }

    public void ApplyBattleStyle(CardBattleState battleState)
    {
        if (battleState.IsFresh)
        {
            CardRoot.transform.Find("GrayFilter").gameObject.SetActive(true);
            CardRoot.transform.Find("CardFrame").gameObject.SetActive(false);
        }
        else
        {
            CardRoot.transform.Find("GrayFilter").gameObject.SetActive(false);

            if (!battleState.HasAttacked)
                CardRoot.transform.Find("CardFrame").GetComponent<MeshRenderer>().material.color = new Color32(247, 234, 117, 255);
            else
                CardRoot.transform.Find("CardFrame").GetComponent<MeshRenderer>().material.color = new Color32(165, 165, 165, 255);

            CardRoot.transform.Find("CardFrame").gameObject.SetActive(true);
        }
    }

    public void ResetBattleStyle()
    {
        CardRoot.transform.Find("GrayFilter").gameObject.SetActive(false);
        CardRoot.transform.Find("CardFrame").gameObject.SetActive(false);
    }

    public void SetHPOnClone(int HP) { }
}
