using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class CardOverlayManager
{
    private static CardOverlayManager _instance;
    public static CardOverlayManager Instance => _instance ??= new CardOverlayManager();

    private readonly Dictionary<string, VisualElement> overlaysVisualElement = new();

    private const float COLLECTION_TARGET_SCALE = 2.1f;
    private const float DECK_TARGET_SCALE = 1.55f;
    private const float DURATION = 0.4f;

    private CardOverlayManager() { }

    public void RegisterCardOverlay(string sceneName, VisualElement overlay)
    {
        overlay.RegisterCallback<ClickEvent>(evt => CardScaleAnimatorUIToolkit.HideCard(overlay));
        overlaysVisualElement[sceneName] = overlay;
    }

    public void ShowCollectionCard(ICollectionCardView originalCardView, ICollectionCardView cloneCardView)
    {
        string sceneName = SceneManager.GetActiveScene().name;

        if (overlaysVisualElement.TryGetValue(sceneName, out var overlay))
        {
            CardScaleAnimatorUIToolkit.ShowCard(
               originalCardView.CardRoot,
               cloneCardView.CardRoot,
               overlay,
               COLLECTION_TARGET_SCALE, DURATION
           );
        }
        else
        {
            Debug.LogWarning($"[OverlayManager] OverlayVE для сцены {sceneName} не найден!");
        }
    }

    public void ShowDeckCard(IDeckCardView originalCardView, IDeckCardView cloneCardView)
    {
        string sceneName = SceneManager.GetActiveScene().name;

        if (overlaysVisualElement.TryGetValue(sceneName, out var overlay))
        {
            CardScaleAnimatorUIToolkit.ShowCard(
                originalCardView.CardRoot,
                cloneCardView.CardRoot,
                overlay,
                DECK_TARGET_SCALE, DURATION
            );
        }
        else
        {
            Debug.LogWarning($"[OverlayManager] OverlayVE для сцены {sceneName} не найден!");
        }
    }

    public void HideOverlay()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        if (overlaysVisualElement.TryGetValue(sceneName, out var overlay))
        {
            overlay.schedule.Execute(() =>
            {
                overlay.Clear();
                overlay.RemoveFromClassList("overlay-active");
                overlay.AddToClassList("overlay-none");
            }).StartingIn(100);
        }
    }
}
