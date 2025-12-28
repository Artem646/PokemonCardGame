using System;
using UnityEngine;
using UnityEngine.UIElements;

public class DeckEditorCardController : BaseCardController
{
    public IDeckCardView DeckCardView { get; set; }
    private bool isSelected;
    public event Action<int, bool> OnCardSelectionChanged;

    public DeckEditorCardController(CardModel model, IDeckCardView view, bool selected)
        : base(model, view)
    {
        DeckCardView = view;
        SetSelected(selected);
        RegisterEvents();
    }

    public override void RegisterEvents()
    {
        DeckCardView.RegisterClickHandlers(OnCardElementClicked);
    }

    public override void UnregisterEvents()
    {
        DeckCardView.UnregisterClickHandlers(OnCardElementClicked);
    }

    private void OnCardElementClicked(ClickEvent evt)
    {
        SetSelected(!isSelected);
        OnCardSelectionChanged?.Invoke(CardModel.id, isSelected);
    }

    public override void AddToContainer(object container)
    {
        if (container is VisualElement visualElement)
            visualElement.Add(DeckCardView.CardRoot);
    }

    public override void RemoveFromContainer()
    {
        DeckCardView.CardRoot.RemoveFromHierarchy();
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        DeckCardView.SetSelected(selected);
    }
}
