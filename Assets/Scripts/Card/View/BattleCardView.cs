using UnityEngine;
using UnityEngine.UI;

public class BattleCardView : CardViewBase, IBattleCardView
{
    public GameObject CardRoot { get; }
    public GameObject CardPrefab { get; }
    private GameObject backCover;
    private bool isFaceDown;

    public BattleCardView(CardModel model, GameObject prefab, Transform parent, bool faceDown)
        : base(model)
    {
        CardPrefab = prefab;
        CardRoot = Object.Instantiate(CardPrefab, parent);
        CardRoot.name = faceDown ? $"{model.titleKey}_Back" : model.titleKey;
        isFaceDown = faceDown;

        InitializeElements();
        BindData();
        ApplyFaceDownState(isFaceDown);

        if (CardRoot.TryGetComponent<CardFlipScript>(out var flipScript))
        {
            flipScript.OnFlipStateChanged += (faceDown) =>
            {
                ApplyFaceDownState(faceDown);
            };
        }
    }

    private void InitializeElements()
    {
        backCover = CardRoot.transform.Find("BackCover").gameObject;
    }

    public override void BindData()
    {
        CardRoot.GetComponent<Image>().color = CardModel.colors.cardColor;
        Localizer.LocalizeGameObjectElement(CardRoot, "Body/Title", CardModel.titleKey, "PokemonTitles");
        CardViewHelper.UpdateBodyUGUI(CardRoot, CardModel);
        CardViewHelper.SetImagesUGUI(CardRoot, CardModel);
        CardViewHelper.BindHPWithBattleState(CardRoot, CardModel);
        // CardViewHelper.UpdateStatsUGUI(CardRoot, CardModel);
    }

    public void ApplyFaceDownState(bool faceDown)
    {
        isFaceDown = faceDown;
        backCover.SetActive(isFaceDown);
    }

    public void ApplyBattleStyle(CardBattleState battleState)
    {
        if (battleState.IsFresh)
        {
            CardRoot.transform.Find("GrayFilter").gameObject.SetActive(true);
            CardRoot.GetComponent<Outline>().enabled = false;
        }
        else
        {
            CardRoot.transform.Find("GrayFilter").gameObject.SetActive(false);

            if (!battleState.HasAttacked)
                CardRoot.GetComponent<Outline>().effectColor = new Color32(247, 234, 117, 255);
            else
                CardRoot.GetComponent<Outline>().effectColor = Color.softRed;

            CardRoot.GetComponent<Outline>().enabled = true;
        }
    }

    public void ResetBattleStyle()
    {
        CardRoot.transform.Find("GrayFilter").gameObject.SetActive(false);
        CardRoot.GetComponent<Outline>().enabled = false;
        CardRoot.transform.Find("Highlighted").gameObject.SetActive(false);
    }

    public void SetHPOnClone(int HP) => CardViewHelper.BindCloneHPWithOriginal(CardRoot, HP);
}
