using UnityEngine;

[CreateAssetMenu(fileName = "NewCardUIToolkitVisualData", menuName = "Cards/UIToolkit Visual Data")]
public class CardUIToolkitVisualDataSO : ScriptableObject
{
    public int cardId;

    [Header("Pokemon Image Position (%)")]
    [Range(-100f, 100f)] public float topPercent = 0;
    [Range(-100f, 100f)] public float leftPercent = 0;

    [Header("Pokemon Image Transform")]
    public float rotationAngle = 0;
    public float scaleX = 1;
    public float scaleY = 1;

    [Header("Title Text Settings")]
    public float fontSizeEN = 38f;
    public float fontSizeRU = 32f;
    public float letterSpacingEN = 12f;
    public float letterSpacingRU = 12f;
}