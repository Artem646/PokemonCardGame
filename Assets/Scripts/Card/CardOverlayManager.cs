using UnityEngine.UIElements;

public class CardOverlayManager
{
    public static CardOverlayManager Instance { get; } = new CardOverlayManager();

    private VisualElement overlay;

    public void Init(VisualElement overlay)
    {
        this.overlay = overlay;
        overlay?.RegisterCallback<ClickEvent>(evt => { CardScaleAnimator.AnimateCardBack(overlay); });
    }

    public void ShowCard(CardView originalCardView, CardView cloneCardView, ClickEvent evt)
    {
        CardScaleAnimator.AnimateCardFromListToOverlay(
            originalCardView.CardRoot.Q<VisualElement>("fullCard"), cloneCardView.CardRoot, overlay, evt.position, 1.6f);
    }
}
