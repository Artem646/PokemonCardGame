using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class DecksSceneController : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private SettingsController settingsController;

    private VisualElement root;
    private VisualElement profileField;

    private async void Start()
    {
        root = uiDocument.rootVisualElement;
        profileField = root.Q<VisualElement>("profileField");

        // root.Q<VisualElement>("loadingOverlay").style.display = DisplayStyle.Flex;

        UserProfileView.Instance.SetUIDocument(uiDocument, settingsController);
        await UserProfileView.Instance.LoadUserData();

        // await WaitUntilCardsLoaded(cardsContainer, CardRepository.Instance.GetUserCards().cards.Count);

        // root.Q<VisualElement>("loadingOverlay").style.display = DisplayStyle.None;

        RegisterCallbacks();
    }

    // private async Task WaitUntilCardsLoaded(ScrollView cardsContainer, int expectedCount)
    // {
    //     while (cardsContainer.childCount < expectedCount)
    //     {
    //         await Task.Yield();
    //     }
    // }

    private void RegisterCallbacks()
    {
        root.Q<Button>("playButton").RegisterCallback<ClickEvent>(evt =>
        {
            SceneSwitcher.SwitchScene("StartPlayScene", root);
        });

        root.Q<Button>("bestiaryButton").RegisterCallback<ClickEvent>(evt =>
        {
            SceneContext.PreviousSceneName = SceneManager.GetActiveScene().name;
            SceneSwitcher.SwitchScene("BestiaryScene", root);
        });

        root.Q<Button>("collectionButton").RegisterCallback<ClickEvent>(evt =>
        {
            SceneSwitcher.SwitchScene("CollectionScene", root);
        });

        profileField.RegisterCallback<ClickEvent>(evt =>
        {
            settingsController.OpenSettings();
        });
    }
}
