using UnityEngine;
using UnityEngine.UIElements;

public class PlayModeCardVisualTweaker : MonoBehaviour
{
    public int targetCardId;
    public CardUIToolkitVisualDataSO visualData;
    public TweakerLanguageMode currentLanguageMode;

    private VisualElement targetPokemonImage;
    private Label targetPokemonLabel;
    private int lastProcessedCardId = -1;

    void Update()
    {
        if (!Application.isPlaying || visualData == null) return;

        if (targetCardId != lastProcessedCardId) FindCard();

        ApplyCurrentSettings();
    }

    private void FindCard()
    {
        CollectionCardController controller = CardRepository.Instance.GetCollectionCardControllerById(targetCardId);
        if (controller != null)
        {
            VisualElement root = controller.CollectionCardView.CardRoot;
            targetPokemonImage = root.Q<VisualElement>("pokemonImage");
            targetPokemonLabel = root.Q<Label>("title");
            lastProcessedCardId = targetCardId;
            Debug.Log($"[Tweaker] Настроена связь с ID: {targetCardId}");
        }
    }

    private void ApplyCurrentSettings()
    {
        targetPokemonImage.style.top = Length.Percent(visualData.topPercent);
        targetPokemonImage.style.left = Length.Percent(visualData.leftPercent);
        targetPokemonImage.style.rotate = new Rotate(Angle.Degrees(visualData.rotationAngle));
        targetPokemonImage.style.scale = new StyleScale(new Vector2(visualData.scaleX, visualData.scaleY));

        if (currentLanguageMode == TweakerLanguageMode.English)
        {
            targetPokemonLabel.style.fontSize = visualData.fontSizeEN;
            targetPokemonLabel.style.letterSpacing = visualData.letterSpacingEN;
        }
        else if (currentLanguageMode == TweakerLanguageMode.RussianAndBelarusian)
        {
            targetPokemonLabel.style.fontSize = visualData.fontSizeRU;
            targetPokemonLabel.style.letterSpacing = visualData.letterSpacingRU;
        }
    }

    [ContextMenu("Save To SO")]
    public void SaveToSO()
    {
        if (visualData == null) return;

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(visualData);
        UnityEditor.AssetDatabase.SaveAssets();
#endif
        Debug.Log($"Данные для {visualData.name} (ID: {targetCardId}) сохранены!");
    }
}