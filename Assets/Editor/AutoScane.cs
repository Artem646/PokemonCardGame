using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class AutoStartScene
{
    private const string menuItem = "Tools/Set Start Scene";
    private const string prefsKey = "AutoStartScenePath";

    static AutoStartScene()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    [MenuItem(menuItem)]
    private static void SetStartScene()
    {
        // берём текущую открытую сцену как стартовую
        string path = EditorSceneManager.GetActiveScene().path;
        if (!string.IsNullOrEmpty(path))
        {
            EditorPrefs.SetString(prefsKey, path);
            Debug.Log($"Стартовая сцена установлена: {path}");
        }
        else
        {
            Debug.LogWarning("Нет активной сцены для установки!");
        }
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            string path = EditorPrefs.GetString(prefsKey, "");
            if (!string.IsNullOrEmpty(path))
            {
                EditorSceneManager.OpenScene(path);
            }
        }
    }
}
