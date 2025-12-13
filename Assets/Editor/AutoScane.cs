using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public static class AutoStartScene
{
    private const string loadingScenePath = "Assets/Scenes/LoadingScene.unity";

    static AutoStartScene()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            if (!string.IsNullOrEmpty(loadingScenePath))
            {
                EditorSceneManager.OpenScene(loadingScenePath);
            }
        }
    }
}
