using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public static class SceneSwitcher
{
    public static void SwitchScene(string targetSceneName, VisualElement currentRoot)
    {
        if (!SceneManager.GetSceneByName(targetSceneName).isLoaded)
        {
            SceneManager.LoadSceneAsync(targetSceneName, LoadSceneMode.Additive).completed += op =>
                ActivateScene(targetSceneName, currentRoot);
        }
        else
            ActivateScene(targetSceneName, currentRoot);
    }

    private static void ActivateScene(string targetSceneName, VisualElement currentRoot)
    {
        currentRoot.style.display = DisplayStyle.None;
        Scene targetScene = SceneManager.GetSceneByName(targetSceneName);
        SceneManager.SetActiveScene(targetScene);
        foreach (var gameObject in targetScene.GetRootGameObjects())
        {
            if (gameObject.name == "UIDocument")
            {
                UIDocument document = gameObject.GetComponent<UIDocument>();
                document.rootVisualElement.style.display = DisplayStyle.Flex;
                break;
            }
        }
    }

    public static void ReturnToPreviousDescriptionScene()
    {
        Scene previousDescriptionScene = SceneManager.GetSceneByName(SceneContext.PreviousDescriptionSceneName);

        foreach (var gameObject in previousDescriptionScene.GetRootGameObjects())
        {
            if (gameObject.name == "UIDocument")
            {
                UIDocument document = gameObject.GetComponent<UIDocument>();
                document.rootVisualElement.style.display = DisplayStyle.Flex;
                break;
            }
        }

        SceneManager.UnloadSceneAsync("DescriptionScene");
    }
}
