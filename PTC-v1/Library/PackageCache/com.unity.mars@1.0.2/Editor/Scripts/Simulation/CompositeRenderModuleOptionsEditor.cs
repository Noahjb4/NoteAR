using Unity.XRTools.ModuleLoader;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Unity.MARS
{
    [CustomEditor(typeof(CompositeRenderModuleOptions))]
    class CompositeRenderModuleOptionsEditor : Editor
    {
        CompositeRenderModuleOptionsDrawer m_CompositeRenderModuleOptionsDrawer;

        void OnEnable()
        {
            m_CompositeRenderModuleOptionsDrawer = new CompositeRenderModuleOptionsDrawer(serializedObject);
        }

        public override void OnInspectorGUI()
        {
            m_CompositeRenderModuleOptionsDrawer.InspectorGUI(serializedObject);
        }
    }

    class CompositeRenderModuleOptionsDrawer
    {
        SerializedProperty m_UseFallbackCompositeRenderingProperty;
        SerializedProperty m_UseCameraStackInFallbackProperty;

        public CompositeRenderModuleOptionsDrawer(SerializedObject serializedObject)
        {
            m_UseFallbackCompositeRenderingProperty = serializedObject.FindProperty("m_UseFallbackCompositeRendering");
            m_UseCameraStackInFallbackProperty = serializedObject.FindProperty("m_UseCameraStackInFallback");
        }
        internal void InspectorGUI(SerializedObject serializedObject)
        {
            serializedObject.Update();
            EditorGUIUtility.labelWidth = MARSEditorGUI.SettingsLabelWidth;

            using (new EditorGUI.DisabledScope(Application.isPlaying))
            {
                using (var changed = new EditorGUI.ChangeCheckScope())
                {
                    using (new EditorGUI.DisabledScope(RenderPipelineManager.currentPipeline != null))
                    {
                        EditorGUILayout.PropertyField(m_UseFallbackCompositeRenderingProperty);
                    }

                    // TODO: Bring this back when we've fixed this use case
                    //using (new EditorGUI.DisabledScope(!CompositeRenderModuleOptions.instance.UseFallbackCompositeRendering))
                    //{
                    //    EditorGUI.indentLevel++;
                    //    EditorGUILayout.PropertyField(m_UseCameraStackInFallbackProperty);
                    //    EditorGUI.indentLevel--;
                    //}

                    if (changed.changed)
                    {
                        serializedObject.ApplyModifiedProperties();

                        if (ModuleLoaderCore.instance != null)
                            ModuleLoaderCore.instance.ReloadModules();
                    }
                }
            }
        }
    }
}
