using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class TopButtonPanel : VisualElement
{
    public TopButtonPanel()
    {
        VisualTreeAsset topPanel = Resources.Load<VisualTreeAsset>("TopButtonPanelUXML");
        topPanel.CloneTree(this);
        RegisterCallback<AttachToPanelEvent>(RegisterCallbacks);
    }

    private void RegisterCallbacks(AttachToPanelEvent evt)
    {
        if (Application.isPlaying) { UserProfileView.Instance.SetUIDocument(this); }

        this.Q<Button>("playButton")?.RegisterCallback<ClickEvent>(e => SceneSwitcher.SwitchScene("StartPlayScene", parent));
        this.Q<Button>("decksButton")?.RegisterCallback<ClickEvent>(e => SceneSwitcher.SwitchScene("DecksScene", parent));
        this.Q<Button>("collectionButton")?.RegisterCallback<ClickEvent>(e => SceneSwitcher.SwitchScene("CollectionScene", parent));
        this.Q<Button>("friendsButton")?.RegisterCallback<ClickEvent>(e => SceneSwitcher.SwitchScene("FriendsScene", parent));
        this.Q<Button>("optionsButton")?.RegisterCallback<ClickEvent>(evt => SceneSwitcher.SwitchScene("OptionsScene", parent));
        this.Q<VisualElement>("profileField")?.RegisterCallback<ClickEvent>(e =>
        {
            SettingsController settings = Object.FindAnyObjectByType<SettingsController>();
            settings.OpenSettings();
        });
    }
}