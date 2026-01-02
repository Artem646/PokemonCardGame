using System;
using UnityEngine;
using UnityEngine.UIElements;

public class ConfirmDialogController : MonoBehaviour
{
    public static ConfirmDialogController Instance { get; private set; }
    [SerializeField] private UIDocument uiDocument;

    private VisualElement root;
    private VisualElement overlay;
    private Label dialogMessage;
    private Button retryButton;
    private Button cancelButton;
    private Button bestiaryButton;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        root = uiDocument.rootVisualElement;
        overlay = root.Q<VisualElement>("overlay");
        dialogMessage = root.Q<Label>("dialogMessage");
        retryButton = root.Q<Button>("retryButton");
        cancelButton = root.Q<Button>("cancelButton");
        bestiaryButton = root.Q<Button>("bestiaryButton");

        overlay.style.display = DisplayStyle.None;
    }

    public static void ShowDialog(string message, Action onRetry, Action onCancel, Action onBestiaty)
    {
        Instance.Show(message, onRetry, onCancel, onBestiaty);
    }

    private void Show(string message, Action onRetry, Action onCancel, Action onBestiary)
    {
        dialogMessage.text = message;
        overlay.style.display = DisplayStyle.Flex;

        retryButton.clicked -= RetryClicked;
        cancelButton.clicked -= CancelClicked;
        bestiaryButton.clicked -= BestiaryClicked;

        void RetryClicked()
        {
            overlay.style.display = DisplayStyle.None;
            onRetry?.Invoke();
        }

        void CancelClicked()
        {
            overlay.style.display = DisplayStyle.None;
            onCancel?.Invoke();
        }

        void BestiaryClicked()
        {
            overlay.style.display = DisplayStyle.None;
            onBestiary?.Invoke();
        }

        retryButton.clicked += RetryClicked;
        cancelButton.clicked += CancelClicked;
        bestiaryButton.clicked += BestiaryClicked;
    }

    public static void CloseDialog()
    {
        if (Instance != null && Instance.overlay != null)
            Instance.overlay.style.display = DisplayStyle.None;
    }
}
