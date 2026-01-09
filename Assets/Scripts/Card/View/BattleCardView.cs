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

        if (CardRoot.TryGetComponent(out CardFlipScript flipScript))
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
    }

    public void ApplyFaceDownState(bool faceDown)
    {
        isFaceDown = faceDown;
        backCover.SetActive(isFaceDown);
    }
}
