using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class CardOverlayManager
{
    private static CardOverlayManager _instance;
    public static CardOverlayManager Instance => _instance ??= new CardOverlayManager();

    private readonly Dictionary<string, VisualElement> overlaysVisualElement = new();
    private readonly Dictionary<string, GameObject> overlaysGameObject = new();

    private RectTransform lastCardRectTransform;

    private CardOverlayManager() { }

    public void RegisterOverlayVisualElement(string sceneName, VisualElement overlay)
    {
        overlaysVisualElement[sceneName] = overlay;
        overlay.RegisterCallback<ClickEvent>(evt => CollectionCardScaleAnimator.HideCard(overlay));
    }

    public void RegisterOverlayGameObject(string sceneName, GameObject overlay)
    {
        overlaysGameObject[sceneName] = overlay;
        if (!overlay.TryGetComponent<EventTrigger>(out var trigger))
            trigger = overlay.AddComponent<EventTrigger>();
        var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
        entry.callback.AddListener((data) => BattleCardScaleAnimator.HideCard(overlay));
        trigger.triggers.Add(entry);
    }

    public void ShowCollectionCard(ICollectionCardView originalCardView, ICollectionCardView cloneCardView, ClickEvent evt)
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (overlaysVisualElement.TryGetValue(sceneName, out var overlay))
        {
            CollectionCardScaleAnimator.ShowCard(
                originalCardView.CardRoot.Q<VisualElement>("fullCard"),
                cloneCardView.CardRoot,
                overlay,
                evt.position
            );
        }
        else
        {
            Debug.LogWarning($"[OverlayManager] OverlayVE для сцены {sceneName} не найден!");
        }
    }

    public void ShowBattleCard(IBattleCardView originalCardView, IBattleCardView cloneCardView)
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (overlaysGameObject.TryGetValue(sceneName, out var overlay))
        {
            RectTransform originalRectTransform = originalCardView.CardRoot.GetComponent<RectTransform>();
            lastCardRectTransform = cloneCardView.CardRoot.GetComponent<RectTransform>();
            Canvas canvas = overlay.GetComponentInParent<Canvas>();
            BattleCardScaleAnimator.ShowCard(originalRectTransform, lastCardRectTransform, overlay, canvas);
        }
        else
        {
            Debug.LogWarning($"[OverlayManager] OverlayGO для сцены {sceneName} не найден!");
        }
    }
}
