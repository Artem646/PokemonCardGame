using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class CardOverlayManager
{
    private static CardOverlayManager _instance;
    public static CardOverlayManager Instance => _instance ??= new CardOverlayManager();

    private VisualElement overlayVE;
    private GameObject overlayGO;

    private RectTransform lastCardRectTransform;

    private CardOverlayManager() { }

    public void Init(object overlay)
    {
        if (overlay is VisualElement)
        {
            overlayVE = overlay as VisualElement;
            overlayVE?.RegisterCallback<ClickEvent>(evt => CollectionCardScaleAnimator.HideCard(overlayVE));
        }
        else if (overlay is GameObject)
        {
            overlayGO = overlay as GameObject;

            if (!overlayGO.TryGetComponent<EventTrigger>(out var trigger))
                trigger = overlayGO.AddComponent<EventTrigger>();

            var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
            entry.callback.AddListener((data) => BattleCardScaleAnimator.HideCard(overlayGO));
            trigger.triggers.Add(entry);
        }
    }

    public void ShowCollectionCard(ICollectionCardView originalCardView, ICollectionCardView cloneCardView, ClickEvent evt)
    {
        CollectionCardScaleAnimator.ShowCard(originalCardView.CardRoot.Q<VisualElement>("fullCard"), cloneCardView.CardRoot, overlayVE, evt.position);
    }

    public void ShowBattleCard(IBattleCardView originalCardView, IBattleCardView cloneCardView)
    {
        RectTransform originalRectTransform = originalCardView.CardRootGameObject.GetComponent<RectTransform>();
        lastCardRectTransform = cloneCardView.CardRootGameObject.GetComponent<RectTransform>();
        Canvas canvas = overlayGO.GetComponentInParent<Canvas>();
        BattleCardScaleAnimator.ShowCard(originalRectTransform, lastCardRectTransform, overlayGO, canvas);
    }
}
