using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public static class SceneSwitcher
{
    public static void SwitchScene(string sceneName, VisualElement currentRoot)
    {
        if (!SceneManager.GetSceneByName(sceneName).isLoaded)
        {
            SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive).completed += op =>
            {
                ActivateScene(sceneName, currentRoot);
            };
        }
        else
        {
            ActivateScene(sceneName, currentRoot);
        }
    }

    private static void ActivateScene(string sceneName, VisualElement currentRoot)
    {
        currentRoot.style.display = DisplayStyle.None;
        var scene = SceneManager.GetSceneByName(sceneName);
        SceneManager.SetActiveScene(scene);

        foreach (var gameObject in scene.GetRootGameObjects())
        {
            if (gameObject.name == "UIDocument")
            {
                UIDocument doc = gameObject.GetComponent<UIDocument>();
                doc.rootVisualElement.style.display = DisplayStyle.Flex;
            }
        }
    }
}
