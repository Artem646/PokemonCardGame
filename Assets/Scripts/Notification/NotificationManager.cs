using UnityEngine;
using UnityEngine.UIElements;
using DG.Tweening;
using System.Collections.Generic;

public enum NotificationType
{
    Success,
    Error,
    Info
}

public class NotificationManager : MonoBehaviour
{
    public static NotificationManager Instance { get; private set; }

    [SerializeField] private UIDocument uiDocument;

    private VisualElement notificationContainer;
    private Label notificationText;

    private Tweener showTween;
    private Tweener hideTween;
    private Tween delayedHideTween;

    private Dictionary<NotificationType, string> notificationMap =
        new()
        {
            { NotificationType.Success, "success-notification" },
            { NotificationType.Error, "error-notification" },
            { NotificationType.Info, "info-notification" }
        };

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        VisualElement root = uiDocument.rootVisualElement;
        notificationContainer = root.Q<VisualElement>("notificationContainer");
        notificationText = root.Q<Label>("notificationText");

        notificationContainer.style.display = DisplayStyle.None;
        notificationContainer.style.scale = new Scale(new Vector2(0f, 1f));
    }

    public static void ShowNotification(string message, NotificationType type, float durationSeconds = 3f)
    {
        Instance.Show(message, type, durationSeconds);
    }

    private void Show(string message, NotificationType type, float durationSeconds)
    {
        notificationText.text = message;

        foreach (string cssClass in notificationMap.Values)
            notificationContainer.RemoveFromClassList(cssClass);

        if (notificationMap.TryGetValue(type, out string cssClassName))
            notificationContainer.AddToClassList(cssClassName);

        notificationContainer.style.display = DisplayStyle.Flex;

        showTween?.Kill();
        hideTween?.Kill();
        delayedHideTween?.Kill();

        notificationContainer.style.scale = new Scale(new Vector2(0f, 1f));

        float start = 0f;
        float duration = 0.3f;

        showTween = DOTween.To(
            () => start,
            x =>
            {
                start = x;
                notificationContainer.style.scale = new Scale(new Vector2(x, 1f));
            },
            1f,
            duration
        ).SetEase(Ease.InCubic);

        delayedHideTween = DOVirtual.DelayedCall(durationSeconds, HideNotification);
    }

    private void HideNotification()
    {
        showTween?.Kill();
        hideTween?.Kill();
        delayedHideTween?.Kill();

        float currentX = notificationContainer.resolvedStyle.scale.value.x;
        float duration = 0.3f;

        hideTween = DOTween.To(
            () => currentX,
            x =>
            {
                currentX = x;
                notificationContainer.style.scale = new Scale(new Vector2(x, 1f));
            },
            0f,
            duration
        ).SetEase(Ease.InBack)
         .OnComplete(() =>
         {
             notificationContainer.style.display = DisplayStyle.None;
         });
    }
}
