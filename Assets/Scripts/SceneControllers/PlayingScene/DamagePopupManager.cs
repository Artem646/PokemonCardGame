using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class DamagePopupManager : MonoBehaviour
{
    public static DamagePopupManager Instance { get; private set; }

    [SerializeField] private GameObject damagePanelPrefab;

    private RectTransform canvasRectTransform;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        canvasRectTransform = GetComponent<RectTransform>();
    }

    public void ShowDamage(Vector3 worldPosition, int amount)
    {
        GameObject damagePanel = Instantiate(damagePanelPrefab, transform);
        RectTransform rectTransform = damagePanel.GetComponent<RectTransform>();

        Vector2 screenPosition = Camera.main.WorldToScreenPoint(worldPosition + Vector3.up * 1.5f);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, screenPosition,
            Camera.main, out Vector2 localPoint);

        rectTransform.localPosition = localPoint;

        TextMeshProUGUI damageText = damagePanel.GetComponentInChildren<TextMeshProUGUI>();
        Image damagePanelFrameImage = damagePanel.GetComponent<Image>();
        Image damagePanelImage = damagePanel.transform.Find("DamagePanelContainer").GetComponent<Image>();

        damageText.text = $"-{amount}";
        rectTransform.localScale = Vector3.zero;

        Sequence sequence = DOTween.Sequence();

        sequence.Append(rectTransform.DOScale(1.2f, 0.2f).SetEase(Ease.OutBack));
        sequence.Join(rectTransform.DOAnchorPosY(rectTransform.anchoredPosition.y + 130f, 0.7f).SetEase(Ease.OutCubic));
        sequence.Insert(0.5f, damageText.DOFade(0, 0.5f));
        sequence.Join(damagePanelFrameImage.DOFade(0, 0.5f));
        sequence.Join(damagePanelImage.DOFade(0, 0.5f));

        sequence.OnComplete(() => Destroy(damagePanel));
    }
}