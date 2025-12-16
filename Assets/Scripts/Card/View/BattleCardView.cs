using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleCardView : CardViewBase, IBattleCardView, IUGUICardView
{
    public GameObject CardRoot { get; }
    public GameObject CardPrefab { get; }
    private TextMeshProUGUI titleLabel;
    private GameObject backCover;
    private bool isFaceDown;

    public BattleCardView(CardModel model, GameObject prefab, Transform parent, bool faceDown)
        : base(model)
    {
        CardPrefab = prefab;
        CardRoot = Object.Instantiate(CardPrefab, parent);
        CardRoot.name = faceDown ? $"{model.title}_Back" : model.title;
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
        titleLabel = CardRoot.transform.Find("Body/Title").GetComponent<TextMeshProUGUI>();
        backCover = CardRoot.transform.Find("BackCover").gameObject;
    }

    public override void BindData()
    {
        CardRoot.GetComponent<Image>().color = CardModel.colors.cardColor;
        titleLabel.text = CardModel.title;
        CardViewHelper.UpdateBodyUGUI(CardRoot, CardModel);
        CardViewHelper.SetImagesUGUI(CardRoot, CardModel);
    }

    public void ApplyFaceDownState(bool faceDown)
    {
        isFaceDown = faceDown;
        backCover.SetActive(isFaceDown);
    }
}
