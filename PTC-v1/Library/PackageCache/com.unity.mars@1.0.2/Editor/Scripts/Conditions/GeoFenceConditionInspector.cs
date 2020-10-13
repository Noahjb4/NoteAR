using Unity.MARS.Providers;
using Unity.XRTools.ModuleLoader;
using UnityEditor;
using UnityEngine;

namespace Unity.MARS
{
    [ComponentEditor(typeof(GeoFenceCondition))]
    public class GeoFenceConditionInspector : ComponentInspector
    {
        SerializedProperty m_GeoBoundsProperty;
        SerializedProperty m_RuleProperty;
        SerializedProperty m_Latitude;
        SerializedProperty m_Longitude;
        bool m_ShortcutButtonsFoldout = true;

        public override void OnEnable()
        {
            base.OnEnable();
            m_RuleProperty = serializedObject.FindProperty("m_Rule");
            m_GeoBoundsProperty = serializedObject.FindProperty("m_BoundingBox");
            var center = m_GeoBoundsProperty.FindPropertyRelative("center");
            m_Latitude = center.FindPropertyRelative("latitude");
            m_Longitude = center.FindPropertyRelative("longitude");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();
            using (var changeCheck = new EditorGUI.ChangeCheckScope())
            {
                EditorGUILayout.PropertyField(m_RuleProperty);
                EditorGUILayout.PropertyField(m_GeoBoundsProperty);
                if (changeCheck.changed)
                {
                    PropagateGeoLocationChanges();
                }
            }

            m_ShortcutButtonsFoldout = GeoLocationShortcutButtons.DrawShortcutButtons("Geolocation shortcuts", (latitude, longitude) =>
            {
                m_Latitude.doubleValue = latitude;
                m_Longitude.doubleValue = longitude;
                PropagateGeoLocationChanges();
            }, true, m_ShortcutButtonsFoldout);

            var showHint = MarsHints.ShowSimulatedGeoLocationHint;
            if (showHint)
            {
                showHint = MARSUtils.HintBox(showHint, "You can set the simulated geolocation in the GeoLocationModule.", "SetSimLocationInGeoModule");
                if (GUILayout.Button("Go to GeoLocationModule"))
                {
                    Selection.activeObject = ModuleLoaderCore.instance.GetModule<GeoLocationModule>();
                }

                if (!showHint)
                    MarsHints.ShowSimulatedGeoLocationHint = showHint;
            }

            serializedObject.ApplyModifiedProperties();
        }

        void PropagateGeoLocationChanges()
        {
            var foundReasoningApi = ReasoningModule.instance.FindReasoningAPI<GeoDuplicationReasoningAPI>();
            if (foundReasoningApi)
            {
                foundReasoningApi.ProcessScene();
            }
        }
    }
}
