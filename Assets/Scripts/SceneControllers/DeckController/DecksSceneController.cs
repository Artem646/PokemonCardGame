using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class DecksSceneController : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private DeckEditorController editorController;

    private VisualElement root;
    private VisualElement cardOverlay;
    private Button addDeckButton;

    private void Start()
    {
        InitializeUI();
        CardOverlayManager.Instance.RegisterCardOverlay(SceneManager.GetActiveScene().name, cardOverlay);
        RegisterCallbacks();
    }

    private void InitializeUI()
    {
        root = uiDocument.rootVisualElement;
        cardOverlay = root.Q<VisualElement>("overlay");
        addDeckButton = root.Q<Button>("addDeckButton");
    }

    private void RegisterCallbacks()
    {
        addDeckButton.RegisterCallback<ClickEvent>(evt =>
        {
            Deck newDeck = new() { name = "New deck", cards = new List<int>() };
            editorController.SetDeckEditorAction(DeckEditorAction.SaveDeck);
            editorController.OpenDeckEditor(newDeck);
        });
    }
}
