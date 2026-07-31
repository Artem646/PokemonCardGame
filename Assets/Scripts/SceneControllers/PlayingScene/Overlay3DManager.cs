using UnityEngine;
using UnityEngine.EventSystems;

public class Overlay3DManager : MonoBehaviour
{
    public static Overlay3DManager Instance { get; private set; }

    [SerializeField] private Canvas overlayCanvas;
    [SerializeField] private GameObject overlayBackground;

    private void Awake() => Instance = this;

    private void Start()
    {
        if (overlayBackground.TryGetComponent<EventTrigger>(out var trigger))
        {
            EventTrigger.Entry entry = new() { eventID = EventTriggerType.PointerClick };
            entry.callback.AddListener((data) => { BattleCardScaleAnimator.HideCard(overlayBackground); });
            trigger.triggers.Add(entry);
        }
    }

    public void ShowCard(GameObject cloneCard)
    {
        BattleCardScaleAnimator.ShowCard(cloneCard, overlayBackground, overlayCanvas);
    }
}
