#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BattleLaneInputRouter))]
public class BattleLaneInputRouterEditor : Editor
{
    static readonly string[] LaneTitles =
    {
        "Lane 1 Attack",
        "Lane 2 Attack",
        "Lane 3 Attack",
        "Lane 4 Defence",
        "Lane 5 Defence",
        "Lane 6 Defence"
    };

    SerializedProperty battleManager;
    SerializedProperty visualFeedback;
    SerializedProperty laneInputs;
    SerializedProperty inputGlyphMode;
    SerializedProperty preferDynamicBindingSprites;
    SerializedProperty keyboardIconBindings;
    SerializedProperty gamepadIconBindings;
    SerializedProperty triggerDownThreshold;
    SerializedProperty triggerUpThreshold;
    SerializedProperty duplicatePressLockSeconds;
    SerializedProperty loadSavedRuntimeBindings;

    void OnEnable()
    {
        battleManager = serializedObject.FindProperty("battleManager");
        visualFeedback = serializedObject.FindProperty("visualFeedback");
        laneInputs = serializedObject.FindProperty("laneInputs");
        inputGlyphMode = serializedObject.FindProperty("inputGlyphMode");
        preferDynamicBindingSprites = serializedObject.FindProperty("preferDynamicBindingSprites");
        keyboardIconBindings = serializedObject.FindProperty("keyboardIconBindings");
        gamepadIconBindings = serializedObject.FindProperty("gamepadIconBindings");
        triggerDownThreshold = serializedObject.FindProperty("triggerDownThreshold");
        triggerUpThreshold = serializedObject.FindProperty("triggerUpThreshold");
        duplicatePressLockSeconds = serializedObject.FindProperty("duplicatePressLockSeconds");
        loadSavedRuntimeBindings = serializedObject.FindProperty("loadSavedRuntimeBindings");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(battleManager);
        EditorGUILayout.PropertyField(visualFeedback);

        EditorGUILayout.Space(10f);
        EditorGUILayout.LabelField(
            "DESIGNER INPUT - DROPDOWN PER LANE",
            EditorStyles.boldLabel);

        EditorGUILayout.HelpBox(
            "I designer possono cambiare tastiera e gamepad direttamente qui. " +
            "Non serve aprire script o asset .inputactions.",
            MessageType.Info);

        DrawLaneInputs();

        EditorGUILayout.Space(10f);
        EditorGUILayout.LabelField(
            "Dynamic Input Sprites",
            EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(inputGlyphMode);
        EditorGUILayout.PropertyField(preferDynamicBindingSprites);

        EditorGUILayout.HelpBox(
            "Quando un binding cambia, menu e note gia' spawnate aggiornano subito " +
            "la sprite. Se una sprite non esiste viene mantenuto il fallback testuale.",
            MessageType.Info);

        EditorGUILayout.PropertyField(
            keyboardIconBindings,
            new GUIContent("Keyboard Sprite Library"),
            true);

        EditorGUILayout.PropertyField(
            gamepadIconBindings,
            new GUIContent("Gamepad Sprite Library"),
            true);

        EditorGUILayout.Space(10f);
        EditorGUILayout.LabelField(
            "Trigger / Single Press",
            EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(triggerDownThreshold);
        EditorGUILayout.PropertyField(triggerUpThreshold);
        EditorGUILayout.PropertyField(duplicatePressLockSeconds);

        EditorGUILayout.Space(10f);
        EditorGUILayout.LabelField(
            "Runtime Remapping",
            EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(loadSavedRuntimeBindings);

        EditorGUILayout.HelpBox(
            "Durante il gioco Esc e Start/Menu aprono la schermata di rimappatura. " +
            "Questi due controlli sono riservati al menu e non vanno assegnati a una lane.",
            MessageType.Info);

        if (triggerUpThreshold.floatValue >= triggerDownThreshold.floatValue)
        {
            EditorGUILayout.HelpBox(
                "La soglia di rilascio deve essere inferiore alla soglia di pressione.",
                MessageType.Warning);
        }

        serializedObject.ApplyModifiedProperties();
    }

    void DrawLaneInputs()
    {
        int visibleCount = Mathf.Min(6, laneInputs.arraySize);

        for (int laneIndex = 0; laneIndex < visibleCount; laneIndex++)
        {
            SerializedProperty lane = laneInputs.GetArrayElementAtIndex(laneIndex);
            SerializedProperty keyboardKey = lane.FindPropertyRelative("keyboardKey");
            SerializedProperty gamepadControl = lane.FindPropertyRelative("gamepadControl");
            SerializedProperty displayLabel = lane.FindPropertyRelative("displayLabel");
            SerializedProperty displayIcon = lane.FindPropertyRelative("displayIcon");
            SerializedProperty displayColor = lane.FindPropertyRelative("displayColor");

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField(LaneTitles[laneIndex], EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(keyboardKey, new GUIContent("Keyboard Key"));
            EditorGUILayout.PropertyField(gamepadControl, new GUIContent("Gamepad Control"));
            EditorGUILayout.PropertyField(
                displayLabel,
                new GUIContent("Display Label", "Lascia vuoto per generarla automaticamente."));
            EditorGUILayout.PropertyField(
                displayIcon,
                new GUIContent(
                    "Display Icon Fallback",
                    "Fallback della lane. La sprite dinamica del binding ha priorita' quando disponibile."));
            EditorGUILayout.PropertyField(displayColor, new GUIContent("Display Color"));
            EditorGUILayout.EndVertical();
        }
    }
}
#endif
