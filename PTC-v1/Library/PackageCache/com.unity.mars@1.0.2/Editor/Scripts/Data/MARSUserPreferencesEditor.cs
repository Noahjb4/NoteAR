using UnityEditor;
using UnityEngine;

namespace Unity.MARS
{
    [CustomEditor(typeof(MARSUserPreferences))]
    public class MARSUserPreferencesEditor : Editor
    {
        MARSUserPreferencesDrawer m_PreferencesDrawer;

        void OnEnable()
        {
            m_PreferencesDrawer = new MARSUserPreferencesDrawer(serializedObject);
        }

        public override void OnInspectorGUI()
        {
            var prefs = serializedObject.targetObject as MARSUserPreferences;

            m_PreferencesDrawer.InspectorGUI(prefs, serializedObject);
        }
    }

    public class MARSUserPreferencesDrawer
    {
        SerializedProperty m_HighlightedSimulatedObjectColorProperty;
        SerializedProperty m_ConditionFailTextColorProperty;

        SerializedProperty m_ExtractPlanesOnSaveProperty;
        SerializedProperty m_ScaleEntityPositionsProperty;
        SerializedProperty m_ScaleEntityChildrenProperty;
        SerializedProperty m_ScaleSceneAudioProperty;
        SerializedProperty m_ScaleSceneLightingProperty;
        SerializedProperty m_RestrictCameraToEnvironmentBoundsProperty;
        SerializedProperty m_TintImageMarkers;
        SerializedProperty m_ElectiveExtensionsReportsErrors;

        public MARSUserPreferencesDrawer(SerializedObject serializedObject)
        {
            m_HighlightedSimulatedObjectColorProperty = serializedObject.FindProperty("m_HighlightedSimulatedObjectColor");
            m_ConditionFailTextColorProperty = serializedObject.FindProperty("m_ConditionFailTextColor");
            m_ExtractPlanesOnSaveProperty = serializedObject.FindProperty("m_ExtractPlanesOnSave");
            m_ScaleEntityPositionsProperty = serializedObject.FindProperty("m_ScaleEntityPositions");
            m_ScaleEntityChildrenProperty = serializedObject.FindProperty("m_ScaleEntityChildren");
            m_ScaleSceneAudioProperty = serializedObject.FindProperty("m_ScaleSceneAudio");
            m_ScaleSceneLightingProperty = serializedObject.FindProperty("m_ScaleSceneLighting");
            m_RestrictCameraToEnvironmentBoundsProperty = serializedObject.FindProperty("m_RestrictCameraToEnvironmentBounds");
            m_TintImageMarkers = serializedObject.FindProperty("m_TintImageMarkers");
            m_ElectiveExtensionsReportsErrors = serializedObject.FindProperty("m_ElectiveExtensionsReportsErrors");

        }

        public void InspectorGUI(MARSUserPreferences prefs, SerializedObject serializedObject)
        {
            serializedObject.Update();
            EditorGUIUtility.labelWidth = MARSEditorGUI.SettingsLabelWidth;

            EditorGUI.BeginChangeCheck();

            if (GUILayout.Button("Reset All Hints", GUILayout.Width(120)))
                MarsHints.ResetHints();

            EditorGUILayout.PropertyField(m_HighlightedSimulatedObjectColorProperty);
            EditorGUILayout.PropertyField(m_ConditionFailTextColorProperty);

            using (new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Use Default Colors"))
                {
                    prefs.ResetColors();
                    EditorUtility.SetDirty(prefs);
                    serializedObject.UpdateIfRequiredOrScript();
                }
                GUILayout.FlexibleSpace();
            }

            if (EditorGUI.EndChangeCheck())
            {
                foreach (var window in Resources.FindObjectsOfTypeAll<EditorWindow>())
                {
                    window.Repaint();
                }
            }

            GUILayout.Space(5);

            EditorGUILayout.PropertyField(m_ExtractPlanesOnSaveProperty);
            EditorGUILayout.PropertyField(m_ScaleEntityPositionsProperty);
            EditorGUILayout.PropertyField(m_ScaleEntityChildrenProperty);
            EditorGUILayout.PropertyField(m_ScaleSceneAudioProperty);
            EditorGUILayout.PropertyField(m_ScaleSceneLightingProperty);

            EditorGUILayout.PropertyField(m_RestrictCameraToEnvironmentBoundsProperty);

            EditorGUILayout.PropertyField(m_TintImageMarkers);

            EditorGUILayout.PropertyField(m_ElectiveExtensionsReportsErrors);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
