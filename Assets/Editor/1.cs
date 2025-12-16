using UnityEngine;
using UnityEditor;
using System.IO;
using ExcelDataReader;
using System.Collections.Generic;

public class TypeChartExcelMatrixImporter : EditorWindow
{
    private TypeChart chart;
    private string excelPath = "Assets/TypeChart.xlsx";

    [MenuItem("Tools/Type Chart Excel Matrix Importer")]
    public static void ShowWindow()
    {
        GetWindow<TypeChartExcelMatrixImporter>("Type Chart Matrix Importer");
    }

    private void OnGUI()
    {
        chart = (TypeChart)EditorGUILayout.ObjectField("Type Chart", chart, typeof(TypeChart), false);
        excelPath = EditorGUILayout.TextField("Excel Path", excelPath);

        if (GUILayout.Button("Обновить из Excel (матрица)"))
        {
            ImportExcelMatrix();
        }
    }

    private void ImportExcelMatrix()
    {
        if (chart == null)
        {
            Debug.LogError("Не выбран TypeChart!");
            return;
        }

        if (!File.Exists(excelPath))
        {
            Debug.LogError("Файл Excel не найден: " + excelPath);
            return;
        }

        // 🔹 ОБЯЗАТЕЛЬНО перед чтением Excel
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

        using (var stream = File.Open(excelPath, FileMode.Open, FileAccess.Read))
        using (var reader = ExcelReaderFactory.CreateReader(stream))
        {
            var result = reader.AsDataSet();
            var table = result.Tables[0]; // первая таблица

            var newPairs = new List<TypeChart.TypePair>();

            // первая строка — заголовки (Defender types)
            int colCount = table.Columns.Count;
            int rowCount = table.Rows.Count;

            PokemonElement[] defenders = new PokemonElement[colCount - 1];
            for (int c = 1; c < colCount; c++)
            {
                string defStr = table.Rows[0][c].ToString();
                if (System.Enum.TryParse(defStr, out PokemonElement def))
                    defenders[c - 1] = def;
            }

            // строки (Attacker + значения)
            for (int r = 1; r < rowCount; r++)
            {
                string atkStr = table.Rows[r][0].ToString();
                if (!System.Enum.TryParse(atkStr, out PokemonElement attacker))
                    continue;

                for (int c = 1; c < colCount; c++)
                {
                    string valStr = table.Rows[r][c].ToString();
                    if (float.TryParse(valStr, out float multiplier))
                    {
                        newPairs.Add(new TypeChart.TypePair
                        {
                            attacker = attacker,
                            defender = defenders[c - 1],
                            multiplier = multiplier
                        });
                    }
                }
            }

            chart.pairs = newPairs;
            EditorUtility.SetDirty(chart);
            Debug.Log("TypeChart обновлён из Excel-матрицы!");
        }
    }
}
