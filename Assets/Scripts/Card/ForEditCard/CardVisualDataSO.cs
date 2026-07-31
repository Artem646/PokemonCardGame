using UnityEngine;

[CreateAssetMenu(fileName = "NewVisualData", menuName = "Cards/Visual Data")]
public class CardVisualDataSO : ScriptableObject
{
    public int cardId;

    [Header("Pokemon Image Texture Settings")]
    public Vector3 position = new(0.00056f, -1.8e-05f, 0.01053f);
    public Vector3 rotation = new(0f, -90f, -90f);
    public Vector3 scale = new(0.03016113f, 0.03035458f, 0.025537f);

    [Header("Title Text Settings")]
    public float fontSizeEN = 65f;
    public float fontSizeRU = 55f;
    public float characterSpacingEN = 4.92f;
    public float characterSpacingRU = 4.92f;
}