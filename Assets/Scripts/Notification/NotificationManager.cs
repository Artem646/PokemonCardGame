using UnityEngine;
using UnityEngine.UIElements;

public class NotificationManager : MonoBehaviour
{
    public static NotificationManager Instance { get; private set; }
    [SerializeField] private UIDocument uiDocument;
    private VisualElement notificationContainer;
    private Label notificationText;

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
        notificationContainer = root.Q<VisualElement>("notification-container");
        notificationText = root.Q<Label>("notification-text");

        notificationContainer.style.display = DisplayStyle.None;
    }

    public static void ShowNotification(string message, bool isError = false)
    {
        Instance.Show(message, isError);
    }

    private void Show(string message, bool isError = false)
    {
        if (notificationContainer == null || notificationText == null)
        {
            Debug.LogWarning("[P][Notification] Контейнер не найден!");
            return;
        }

        notificationText.text = message;

        if (isError)
        {
            notificationContainer.RemoveFromClassList("success-notification");
            notificationContainer.AddToClassList("error-notification");
        }
        else
        {
            notificationContainer.RemoveFromClassList("error-notification");
            notificationContainer.AddToClassList("success-notification");
        }

        notificationContainer.style.display = DisplayStyle.Flex;

        CancelInvoke(nameof(HideNotification));
        Invoke(nameof(HideNotification), 3f);
    }

    private void HideNotification()
    {
        notificationContainer.style.display = DisplayStyle.None;
    }
}