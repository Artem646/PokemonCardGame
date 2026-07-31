using TMPro;
using UnityEngine;

public enum TweakerLanguageMode
{
    English,
    RussianAndBelarusian
}

[ExecuteInEditMode]
public class CardVisualTweaker : MonoBehaviour
{
    [SerializeField] private Transform pokemonImageTransform;
    [SerializeField] private TextMeshPro title;

    public CardVisualDataSO visualData;
    public TweakerLanguageMode currentLanguageMode;

    [ContextMenu("Load From SO")]
    public void LoadFromSO()
    {
        if (visualData == null || pokemonImageTransform == null) return;
        pokemonImageTransform.localPosition = visualData.position;
        pokemonImageTransform.localEulerAngles = visualData.rotation;
        pokemonImageTransform.localScale = visualData.scale;

        if (currentLanguageMode == TweakerLanguageMode.English)
        {
            title.fontSize = visualData.fontSizeEN;
            title.characterSpacing = visualData.characterSpacingEN;
        }
        else
        {
            title.fontSize = visualData.fontSizeRU;
            title.characterSpacing = visualData.characterSpacingRU;
        }

        Debug.Log($"[Tweaker] Данные загружены. Режим текста: {currentLanguageMode}");
    }

    [ContextMenu("Save To SO")]
    public void SaveToSO()
    {
        if (visualData == null || pokemonImageTransform == null) return;
        SyncToSO();
        Debug.Log($"[Tweaker] Данные для {visualData.name} сохранены принудительно.");
    }

    private void SyncToSO()
    {
        visualData.position = pokemonImageTransform.localPosition;
        visualData.rotation = pokemonImageTransform.localEulerAngles;
        visualData.scale = pokemonImageTransform.localScale;

        if (currentLanguageMode == TweakerLanguageMode.English)
        {
            visualData.fontSizeEN = title.fontSize;
            visualData.characterSpacingEN = title.characterSpacing;
        }
        else if (currentLanguageMode == TweakerLanguageMode.RussianAndBelarusian)
        {
            visualData.fontSizeRU = title.fontSize;
            visualData.characterSpacingRU = title.characterSpacing;
        }

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(visualData);
#endif
    }
}