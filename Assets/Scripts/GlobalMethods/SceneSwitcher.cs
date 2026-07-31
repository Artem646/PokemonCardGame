using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public interface IRefreshableScene
{
    void RefreshSceneContent();
}

public static class SceneSwitcher
{
    public static bool IsBusy { get; private set; }

    public static void SwitchScene(string targetSceneName, VisualElement currentRoot)
    {
        if (IsBusy) return;
        IsBusy = true;

        bool isAlreadyLoaded = SceneManager.GetSceneByName(targetSceneName).isLoaded;
        if (!isAlreadyLoaded)
        {
            SceneManager.LoadSceneAsync(targetSceneName, LoadSceneMode.Additive).completed += op =>
                ActivateScene(targetSceneName, currentRoot, isAlreadyLoaded);
        }
        else
        {
            if (targetSceneName == "DescriptionScene")
            {
                CardRepository.Instance.AddCardsToIntermediateCardContainer();
                RefreshScene(targetSceneName);
                IsBusy = false;
            }
            else ActivateScene(targetSceneName, currentRoot, isAlreadyLoaded);
        }
    }

    private static void ActivateScene(string targetSceneName, VisualElement currentRoot, bool isAlreadyLoaded)
    {
        if (targetSceneName != "DescriptionScene") currentRoot.style.display = DisplayStyle.None;

        if (SceneManager.GetActiveScene().name == "CollectionScene" || SceneManager.GetActiveScene().name == "BestiaryScene")
            CardRepository.Instance.AddCardsToIntermediateCardContainer();

        Scene targetScene = SceneManager.GetSceneByName(targetSceneName);
        SceneManager.SetActiveScene(targetScene);

        if (isAlreadyLoaded) RefreshScene(targetSceneName);

        foreach (GameObject gameObject in targetScene.GetRootGameObjects())
        {
            if (gameObject.name == "UIDocument")
            {
                UIDocument document = gameObject.GetComponent<UIDocument>();
                document.rootVisualElement.style.display = DisplayStyle.Flex;
                break;
            }
        }

        IsBusy = false;
    }

    private static void RefreshScene(string targetSceneName)
    {
        Scene targetScene = SceneManager.GetSceneByName(targetSceneName);
        foreach (GameObject gameObject in targetScene.GetRootGameObjects())
        {
            if (gameObject.TryGetComponent<IRefreshableScene>(out var refreshableSceneController))
            {
                refreshableSceneController.RefreshSceneContent();
                break;
            }
        }
    }

    public static void ReturnToPreviousDescriptionScene(VisualElement descriptionRoot)
    {
        if (IsBusy) return;
        IsBusy = true;

        descriptionRoot.style.display = DisplayStyle.None;

        CardRepository.Instance.AddCardsToIntermediateCardContainer();

        Scene previousDescriptionScene = SceneManager.GetSceneByName(SceneContext.PreviousDescriptionSceneName);
        SceneManager.SetActiveScene(previousDescriptionScene);

        RefreshScene(SceneContext.PreviousDescriptionSceneName);

        foreach (GameObject gameObject in previousDescriptionScene.GetRootGameObjects())
        {
            if (gameObject.name == "UIDocument")
            {
                UIDocument document = gameObject.GetComponent<UIDocument>();
                document.rootVisualElement.style.display = DisplayStyle.Flex;
                break;
            }
        }

        SceneManager.UnloadSceneAsync("DescriptionScene").completed += op =>
            IsBusy = false;
    }
}