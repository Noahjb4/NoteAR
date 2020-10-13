using System;
using Unity.MARS;
using UnityEditor;
using UnityEngine;

namespace Unity.MARS.MARSEditor
{
    [CustomEditor(typeof(MARSEnvironmentManager))]
    public class MarsEnvironmentManagerEditor : Editor
    {
        MarsEnvironmentManagerDrawer m_MarsEnvironmentManagerDrawer;

        void OnEnable()
        {
            m_MarsEnvironmentManagerDrawer = new MarsEnvironmentManagerDrawer(serializedObject);
        }

        public override void OnInspectorGUI()
        {
            m_MarsEnvironmentManagerDrawer.InspectorGUI(serializedObject);
        }
    }

    class MarsEnvironmentManagerDrawer
    {
        SerializedProperty m_SimulationEnvironmentModeSettingsProperty;

        public MarsEnvironmentManagerDrawer(SerializedObject serializedObject)
        {
            m_SimulationEnvironmentModeSettingsProperty = serializedObject.FindProperty("m_SimulationEnvironmentModeSettings");
        }

        internal void InspectorGUI(SerializedObject serializedObject)
        {
            serializedObject.Update();
            EditorGUIUtility.labelWidth = MARSEditorGUI.SettingsLabelWidth;

            using (var change = new EditorGUI.ChangeCheckScope())
            {
                EditorGUILayout.PropertyField(m_SimulationEnvironmentModeSettingsProperty);

                if (change.changed)
                {
                    serializedObject.ApplyModifiedProperties();

                    var envManager = serializedObject.targetObject as MARSEnvironmentManager;
                    if (envManager != null)
                        envManager.UpdateSimulatedEnvironmentCandidates();
                }
            }
        }
    }
}
