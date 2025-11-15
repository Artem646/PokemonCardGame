using UnityEngine.UIElements;

public class CardOverlayManager
{
    private static CardOverlayManager _instance;
    public static CardOverlayManager Instance => _instance ??= new CardOverlayManager();

    private VisualElement overlay;

    private CardOverlayManager() { }

    public void Init(VisualElement overlay)
    {
        this.overlay = overlay;
        overlay?.RegisterCallback<ClickEvent>(evt => { CardScaleAnimator.AnimateCardBack(overlay); });
    }

    public void ShowCard(ICollectionCardView originalCardView, ICollectionCardView cloneCardView, ClickEvent evt)
    {
        CardScaleAnimator.AnimateCardFromListToOverlay(
            originalCardView.CardRoot.Q<VisualElement>("fullCard"), cloneCardView.CardRoot, overlay, evt.position, 1.6f);
    }
}
