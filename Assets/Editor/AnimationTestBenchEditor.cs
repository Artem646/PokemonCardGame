using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AnimationTestBench))]
public class AnimationTestBenchEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        AnimationTestBench script = (AnimationTestBench)target;

        GUILayout.Space(10);
        GUILayout.Label("Динамический выбор способностей", EditorStyles.boldLabel);

        if (GUILayout.Button("Загрузить список способностей карты"))
            FetchAbilities(script);

        if (script.availableAbilities != null && script.availableAbilities.Length > 0 && !string.IsNullOrEmpty(script.availableAbilities[0]))
            script.selectedAbilityIndex = EditorGUILayout.Popup("Выбрать способность:", script.selectedAbilityIndex, script.availableAbilities);
        else
            EditorGUILayout.HelpBox("Нажмите кнопку выше, чтобы подтянуть способности по ID карты.", MessageType.Info);

        GUILayout.Space(10);

        if (Application.isPlaying)
        {
            if (GUILayout.Button("ЗАПУСТИТЬ АНИМАЦИЮ", GUILayout.Height(40)))
                script.RunAnimation();

            GUILayout.Space(10);

            if (GUILayout.Button("ОБНОВИТЬ КАРТЫ НА СТОЛЕ", GUILayout.Height(40)))
                script.SpawnCards();
        }
        else
            EditorGUILayout.HelpBox("Запуск теста доступен только в режиме Play.", MessageType.Warning);
    }

    private void FetchAbilities(AnimationTestBench script)
    {
        CardModel model = CardRepository.Instance.GetGameCardModelById(int.Parse(script.attackerCardId));
        if (model != null)
        {
            script.availableAbilities[0] = model.abilities.firstAbility.name;
            script.availableAbilities[1] = model.abilities.secondAbility.name;
            script.availableAbilities[2] = model.abilities.thirdAbility.name;
            script.availableAbilities[3] = model.abilities.fourthAbility.name;

            EditorUtility.SetDirty(script);
            Debug.Log($"Способности для карты {model.titleKey} успешно загружены.");
        }
        else
            Debug.LogError("Не удалось найти карту. Убедитесь, что ID верный и проект запущен (или CardRepository инициализирован).");
    }
}