using UnityEngine;
using UnityEditor;

public class TypeChartWindow : EditorWindow
{
    private TypeChart chart;
    private Vector2 scrollPos;

    [MenuItem("Tools/Type Chart Viewer")]
    public static void ShowWindow()
    {
        GetWindow<TypeChartWindow>("Type Chart");
    }

    private void OnGUI()
    {
        chart = (TypeChart)EditorGUILayout.ObjectField("Type Chart", chart, typeof(TypeChart), false);

        if (chart == null || chart.pairs == null || chart.pairs.Count == 0)
        {
            EditorGUILayout.HelpBox("Выбери ScriptableObject TypeChart с заполненными парами.", MessageType.Info);
            return;
        }

        var types = System.Enum.GetValues(typeof(PokemonElement));
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        // Заголовки
        GUILayout.BeginHorizontal();
        GUILayout.Space(120);
        foreach (PokemonElement def in types)
        {
            GUILayout.Label(def.ToString(), GetHeaderStyle(), GUILayout.Width(80));
        }
        GUILayout.EndHorizontal();

        // Строки
        foreach (PokemonElement atk in types)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(atk.ToString(), GetHeaderStyle(), GUILayout.Width(120));

            foreach (PokemonElement def in types)
            {
                float value = chart.GetMultiplier(atk, def);
                GUILayout.Label(value.ToString("0.#"), GetStyle(value), GUILayout.Width(80));
            }

            GUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
    }

    private GUIStyle GetHeaderStyle()
    {
        var style = new GUIStyle(EditorStyles.boldLabel);
        style.fontSize = 14;
        style.alignment = TextAnchor.MiddleCenter;
        return style;
    }

    private GUIStyle GetStyle(float value)
    {
        var style = new GUIStyle(EditorStyles.label)
        {
            fontSize = 16,
            alignment = TextAnchor.MiddleCenter
        };

        if (Mathf.Approximately(value, 2f)) style.normal.textColor = Color.green;
        else if (Mathf.Approximately(value, 0.5f)) style.normal.textColor = Color.red;
        else if (Mathf.Approximately(value, 0f)) style.normal.textColor = Color.black;
        else style.normal.textColor = Color.gray;

        return style;
    }
}
