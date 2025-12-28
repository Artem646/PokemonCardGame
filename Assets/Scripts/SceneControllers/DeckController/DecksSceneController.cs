using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class DecksSceneController : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;

    private VisualElement root;

    // private async void Start()
    private void Start()
    {
        root = uiDocument.rootVisualElement;

        // root.Q<VisualElement>("loadingOverlay").style.display = DisplayStyle.Flex;

        UserProfileView.Instance.SetUIDocument(uiDocument);
        UserProfileView.Instance.UpdateView(UserProfileView.Instance.GetCachedProfile());

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
    }
}
