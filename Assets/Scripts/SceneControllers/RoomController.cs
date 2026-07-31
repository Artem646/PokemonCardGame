using System;
using UnityEngine;
using UnityEngine.UIElements;

public class RoomController : MonoBehaviour
{
    [SerializeField] private VisualTreeAsset roomTemplate;

    private VisualElement roomRoot;
    private Label roomNameLabel;
    private VisualElement checkmark;

    public VisualElement InstantiateRoomElement(string roomName, Action<VisualElement> onClick)
    {
        InitializeUI();

        roomRoot.style.width = new StyleLength(new Length(2080, LengthUnit.Pixel));
        roomRoot.style.height = new StyleLength(new Length(154, LengthUnit.Pixel));

        roomNameLabel.text = roomName;
        checkmark.style.backgroundImage = null;

        roomRoot.userData = roomName;

        roomRoot.RegisterCallback<ClickEvent>(evt =>
        {
            onClick?.Invoke(roomRoot);
        });

        return roomRoot;
    }

    private void InitializeUI()
    {
        roomRoot = roomTemplate.Instantiate();
        roomNameLabel = roomRoot.Q<Label>("roomNameLabel");
        checkmark = roomRoot.Q<VisualElement>("checkmark");
    }

    public void ToggleCheckmark(bool isChecked)
    {
        if (isChecked)
            checkmark.style.backgroundImage = new StyleBackground(Resources.Load<Sprite>($"Sprites/checkmark"));
        else
            checkmark.style.backgroundImage = null;
    }
}
