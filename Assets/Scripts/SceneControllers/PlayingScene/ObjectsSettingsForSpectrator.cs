using UnityEngine;
using System;

[Serializable]
public class ObjectsSettings
{
    public Vector3 fieldsPosition;
    public Vector3 playerHandPosition;
    public Vector3 resetStacksPosition;
    public Vector3[] playerCardSlotsPosition;
}

public class ObjectsSettingsForSpectrator : MonoBehaviour
{
    [SerializeField] private Transform fieldsTransform;
    [SerializeField] private Transform playerHandTransform;
    [SerializeField] private Transform resetStacksTransform;
    [SerializeField] private Transform[] playerHandSlots;
    [SerializeField] private GameObject[] enemyFieldCardSlotsMesh;
    [SerializeField] private ObjectsSettings objectSettings;

    private void Start()
    {
        if (ConnectionConfig.IsSpectator)
        {
            playerHandTransform.position = objectSettings.playerHandPosition;
            fieldsTransform.position = objectSettings.fieldsPosition;
            resetStacksTransform.position = objectSettings.resetStacksPosition;

            for (int i = 0; i < playerHandSlots.Length; i++)
            {
                playerHandSlots[i].Find("SlotPlace").gameObject.SetActive(false);
                playerHandSlots[i].localPosition = objectSettings.playerCardSlotsPosition[i];
            }

            for (int i = 0; i < enemyFieldCardSlotsMesh.Length; i++)
                enemyFieldCardSlotsMesh[i].SetActive(true);
        }
    }
}