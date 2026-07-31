using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public class CardDissolveScript : MonoBehaviour
{
    [SerializeField] private Camera snapshotCamera;
    [SerializeField] private MeshRenderer cardMesh;
    [SerializeField] private GameObject frontUIContainer;
    [SerializeField] private Material shaderCardFrontMaterial;
    [SerializeField] private Material shaderCardEdgeMaterial;
    [SerializeField] private Material shaderCardBackMaterial;

    private const int PHOTO_WIDTH = 712;
    private const int PHOTO_HEIGHT = 930;

    private const float DISSOLVE_DURATION = 1f;
    private const int EDGE_MATERIAL_INDEX = 0;
    private const int BACK_MATERIAL_INDEX = 1;
    private const int FRONT_MATERIAL_INDEX = 2;

    private string dissolvePropertyName = "_Dissolve_Value";
    private string mainTexturePropertyName = "_Texture";

    private GameManager gameManager;

    public IEnumerator DissolveCardForEvolutionRoutine(BattleCardController oldCardController, int newCardId, bool isMyCard, Action onComplete)
    {
        if (gameManager == null) gameManager = FindAnyObjectByType<GameManager>();

        CardRepository.Instance.BuildNewBattleCardController(newCardId);
        BattleCardController newCardController = CardRepository.Instance.GetBattleCardControllerById(newCardId);

        Transform newCardTransform = newCardController.BattleCardView.CardRoot.transform;
        Transform oldCardTransform = oldCardController.BattleCardView.CardRoot.transform;
        Vector3 newCardPosition = newCardTransform.position;
        Transform newCardParent = newCardTransform.parent;
        CardSlot cardSlot = oldCardTransform.parent.GetComponent<CardSlot>();

        Transform evolutionCardsPoints = cardSlot.transform.parent.parent.parent.Find("EvolutionCardsPoints");
        Transform oldCardPoint = evolutionCardsPoints.Find("OldCardPoint");
        Transform newCardPoint = evolutionCardsPoints.Find("NewCardPoint");

        Camera newCardSnapshotCamera = newCardTransform.Find("SnapshotCamera").GetComponent<Camera>();
        MeshRenderer newMeshCard = newCardTransform.GetComponent<MeshRenderer>();
        GameObject newCardFrontUIContainer = newCardTransform.Find("FrontUIContainer").gameObject;

        oldCardController.BattleCardView.ResetBattleStyle();

        Texture2D oldCardFrontTexture = TakeSnapshot(snapshotCamera);
        Texture2D newCardFrontTexture = TakeSnapshot(newCardSnapshotCamera);

        Material[] oldCardOriginalMaterials = cardMesh.materials;
        Material[] newCardOriginalMaterials = newMeshCard.materials;

        ChangeMaterialsForDissolve(cardMesh, oldCardFrontTexture);
        ChangeMaterialsForDissolve(newMeshCard, newCardFrontTexture);

        frontUIContainer.SetActive(false);
        newCardFrontUIContainer.SetActive(false);

        yield return AnimateDissolve(cardMesh.materials, newMeshCard.materials, 0f, 1f);

        Sequence swapSequence = DOTween.Sequence();

        swapSequence.Append(oldCardTransform.DOMove(oldCardPoint.position, 0.01f).SetEase(Ease.OutQuad));
        swapSequence.Append(newCardTransform.DOMove(newCardPoint.position, 0.01f).SetEase(Ease.OutQuad));

        swapSequence.Append(oldCardTransform.DOMove(oldCardPoint.position + Vector3.up * 0.5f, 0.01f)).SetEase(Ease.OutQuad);
        swapSequence.Append(oldCardTransform.DOMove(newCardPoint.position + Vector3.up * 0.5f, 0.01f)).SetEase(Ease.OutQuad);
        swapSequence.Join(newCardTransform.DOMove(oldCardPoint.position, 0.01f)).SetEase(Ease.OutQuad);
        swapSequence.Append(oldCardTransform.DOMove(newCardPoint.position, 0.01f)).SetEase(Ease.OutQuad);

        yield return swapSequence.WaitForCompletion();

        oldCardTransform.DOMove(newCardPosition, 0.01f).SetEase(Ease.OutQuad);
        oldCardTransform.SetParent(newCardParent);

        cardSlot.AddCardInStartGame(newCardController);

        int index = (int)cardSlot.indexSlotInField;
        BattleCardController[] fieldControllers = isMyCard ? gameManager.CurrentGame.PlayerFieldControllers : gameManager.CurrentGame.EnemyFieldControllers;
        fieldControllers[index] = newCardController;

        yield return AnimateDissolve(cardMesh.materials, newMeshCard.materials, 1f, 0f);

        newCardFrontUIContainer.SetActive(true);
        newMeshCard.SetMaterials(newCardOriginalMaterials.ToList());

        newCardController.MarkAsPlayedInThisTurn(isMyCard ? CardOwner.Player : CardOwner.Enemy);
        if (isMyCard) newCardController.BattleCardView.ApplyBattleStyle(newCardController.BattleState);

        Destroy(oldCardFrontTexture);
        Destroy(newCardFrontTexture);
        Destroy(oldCardController.BattleCardView.CardRoot.transform.gameObject);

        onComplete?.Invoke();
    }

    private Texture2D TakeSnapshot(Camera snapshotCamera)
    {
        RenderTexture renderTexture = RenderTexture.GetTemporary(PHOTO_WIDTH, PHOTO_HEIGHT, 24);
        snapshotCamera.targetTexture = renderTexture;

        snapshotCamera.gameObject.SetActive(true);
        snapshotCamera.Render();
        snapshotCamera.gameObject.SetActive(false);

        RenderTexture.active = renderTexture;
        Texture2D snapshot = new(PHOTO_WIDTH, PHOTO_HEIGHT, TextureFormat.ARGB32, false);
        snapshot.ReadPixels(new Rect(0, 0, PHOTO_WIDTH, PHOTO_HEIGHT), 0, 0);
        snapshot.Apply();

        snapshotCamera.targetTexture = null;
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(renderTexture);

        return snapshot;
    }

    private void ChangeMaterialsForDissolve(MeshRenderer targetMesh, Texture2D targetPhoto)
    {
        Material newFrontMaterial = Instantiate(shaderCardFrontMaterial);
        Material newEdgeMaterial = Instantiate(shaderCardEdgeMaterial);
        Material newBackMaterial = Instantiate(shaderCardBackMaterial);

        newFrontMaterial.SetTexture(mainTexturePropertyName, targetPhoto);

        List<Material> cloneMaterials = new(targetMesh.materials)
        {
            [FRONT_MATERIAL_INDEX] = newFrontMaterial,
            [EDGE_MATERIAL_INDEX] = newEdgeMaterial,
            [BACK_MATERIAL_INDEX] = newBackMaterial
        };

        targetMesh.SetMaterials(cloneMaterials);
    }

    private IEnumerator AnimateDissolve(Material[] oldCardMaterial, Material[] newCardMaterial, float startValue, float endValue)
    {
        float time = 0;
        while (time < DISSOLVE_DURATION)
        {
            time += Time.deltaTime;
            float dissolveValue = Mathf.Lerp(startValue, endValue, time / DISSOLVE_DURATION);

            foreach (Material material in oldCardMaterial)
                material.SetFloat(dissolvePropertyName, dissolveValue);

            foreach (Material material in newCardMaterial)
                material.SetFloat(dissolvePropertyName, dissolveValue);

            yield return null;
        }

        foreach (Material material in oldCardMaterial)
            material.SetFloat(dissolvePropertyName, endValue);

        foreach (Material material in newCardMaterial)
            material.SetFloat(dissolvePropertyName, endValue);
    }
}