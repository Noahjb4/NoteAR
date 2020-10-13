using System.Collections.Generic;
using Unity.XRTools.ModuleLoader;
using UnityEditor;
using UnityEngine;

namespace Unity.MARS
{
    [CustomEditor(typeof(DataVisualsModuleOptions))]
    public class DataVisualsModuleOptionsEditor : Editor
    {
        DataVisualsModuleOptionsDrawer m_DataVisualsModuleOptionsDrawer;

        void OnEnable()
        {
            m_DataVisualsModuleOptionsDrawer = new DataVisualsModuleOptionsDrawer(serializedObject);
        }

        public override void OnInspectorGUI()
        {
            m_DataVisualsModuleOptionsDrawer.InspectorGUI(serializedObject);
        }
    }

    public class DataVisualsModuleOptionsDrawer
    {
        static readonly string[] k_PreferencePropertyNames = {
            "m_ShowDataVisualsInHierarchy", "m_DisableSimulationDataVisuals", "m_RatingGradient"
        };

        List<SerializedProperty> m_ExposedProperties = new List<SerializedProperty>() ;

        public DataVisualsModuleOptionsDrawer(SerializedObject serializedObject)
        {
            m_ExposedProperties.Clear();
            foreach (var name in k_PreferencePropertyNames)
                m_ExposedProperties.Add(serializedObject.FindProperty(name));
        }

        public void InspectorGUI(SerializedObject serializedObject)
        {
            EditorGUIUtility.labelWidth = MARSEditorGUI.SettingsLabelWidth;

            using (var check = new EditorGUI.ChangeCheckScope())
            {
                serializedObject.Update();
                foreach (var property in m_ExposedProperties)
                    EditorGUILayout.PropertyField(property);

                serializedObject.ApplyModifiedProperties();

                if (check.changed)
                    ModuleLoaderCore.instance.ReloadModules(); // Reload modules is needed for changes to take effect.
            }

            if (GUILayout.Button("Reset Defaults"))
            {
                var options = serializedObject.targetObject as DataVisualsModuleOptions;
                if (options != null)
                    options.ResetDefaultPreferences();

                AssetDatabase.SaveAssets(); // Save assets so that default values are applied to the asset and update immediately in all drawers
            }
        }
    }
}
