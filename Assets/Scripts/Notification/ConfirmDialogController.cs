using System;
using UnityEngine;
using UnityEngine.UIElements;

public class ConfirmDialogController : MonoBehaviour
{
    public static ConfirmDialogController Instance { get; private set; }
    [SerializeField] private UIDocument uiDocument;

    private VisualElement dialogRoot;
    private Label dialogMessage;
    private Button yesButton;
    private Button noButton;

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
        yesButton = root.Q<Button>("yesButton");
        noButton = root.Q<Button>("noButton");

        dialogRoot.style.display = DisplayStyle.None;
    }

    public static void ShowDialog(string message, Action onYes, Action onNo)
    {
        Instance.Show(message, onYes, onNo);
    }

    private void Show(string message, Action onYes, Action onNo)
    {
        dialogMessage.text = message;
        dialogRoot.style.display = DisplayStyle.Flex;

        yesButton.clicked += () =>
        {
            dialogRoot.style.display = DisplayStyle.None;
            onYes?.Invoke();
        };

        noButton.clicked += () =>
        {
            dialogRoot.style.display = DisplayStyle.None;
            onNo?.Invoke();
        };
    }
}
