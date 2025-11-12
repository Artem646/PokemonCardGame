using System;
using UnityEngine;
using UnityEngine.UIElements;

public class ConfirmDialogController : MonoBehaviour
{
    public static ConfirmDialogController Instance { get; private set; }
    [SerializeField] private UIDocument uiDocument;

    private VisualElement dialogRoot;
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

        VisualElement root = uiDocument.rootVisualElement;
        dialogRoot = root.Q<VisualElement>("dialogRoot");
        dialogMessage = root.Q<Label>("dialogMessage");
        retryButton = root.Q<Button>("retryButton");
        cancelButton = root.Q<Button>("cancelButton");
        bestiaryButton = root.Q<Button>("bestiaryButton");

        dialogRoot.style.display = DisplayStyle.None;
    }

    public static void ShowDialog(string message, Action onRetry, Action onCancel, Action onBestiaty)
    {
        Instance.Show(message, onRetry, onCancel, onBestiaty);
    }

    private void Show(string message, Action onRetry, Action onCancel, Action onBestiary)
    {
        dialogMessage.text = message;
        dialogRoot.style.display = DisplayStyle.Flex;

        retryButton.clicked -= RetryClicked;
        cancelButton.clicked -= CancelClicked;
        bestiaryButton.clicked -= BestiaryClicked;

        void RetryClicked()
        {
            dialogRoot.style.display = DisplayStyle.None;
            onRetry?.Invoke();
        }

        void CancelClicked()
        {
            dialogRoot.style.display = DisplayStyle.None;
            onCancel?.Invoke();
        }

        void BestiaryClicked()
        {
            dialogRoot.style.display = DisplayStyle.None;
            onBestiary?.Invoke();
        }

        retryButton.clicked += RetryClicked;
        cancelButton.clicked += CancelClicked;
        bestiaryButton.clicked += BestiaryClicked;
    }

    public static void CloseDialog()
    {
        if (Instance != null && Instance.dialogRoot != null)
            Instance.dialogRoot.style.display = DisplayStyle.None;
    }
}
