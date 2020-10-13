using Unity.XRTools.Utils;
using UnityEditor;

namespace Unity.MARS.Providers
{
    [CustomEditor(typeof(GeoLocationModule))]
    public class GeoLocationModuleEditor : Editor
    {
        SerializedProperty m_LatitudeProperty;
        SerializedProperty m_LongitudeProperty;
        SerializedProperty m_DesiredAccuracyProperty;
        SerializedProperty m_UpdateDistanceProperty;
        SerializedProperty m_ContinuousUpdatesProperty;
        SerializedProperty m_UpdateIntervalProperty;
        bool m_ShortcutButtonsFoldout = true;

        public void OnEnable()
        {
            m_LatitudeProperty = serializedObject.FindProperty("m_CurrentLocation.latitude");
            m_LongitudeProperty = serializedObject.FindProperty("m_CurrentLocation.longitude");
            m_DesiredAccuracyProperty = serializedObject.FindProperty("m_DesiredAccuracy");
            m_ContinuousUpdatesProperty = serializedObject.FindProperty("m_ContinuousUpdates");
            m_UpdateDistanceProperty = serializedObject.FindProperty("m_UpdateDistance");
            m_UpdateIntervalProperty = serializedObject.FindProperty("m_UpdateInterval");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_DesiredAccuracyProperty);
            EditorGUILayout.PropertyField(m_ContinuousUpdatesProperty);
            using (new EditorGUI.DisabledScope(!m_ContinuousUpdatesProperty.boolValue))
            {
                EditorGUILayout.PropertyField(m_UpdateDistanceProperty);
                EditorGUILayout.PropertyField(m_UpdateIntervalProperty);
            }
            EditorGUILayout.LabelField("Simulated Geolocation", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Change lat/long here to simulate different testing locations.\nValues here have no effect in builds.", MessageType.Info);
            using (var changeCheck = new EditorGUI.ChangeCheckScope())
            {
                EditorGUILayout.PropertyField(m_LatitudeProperty);
                EditorGUILayout.PropertyField(m_LongitudeProperty);
                if (changeCheck.changed)
                {
                    m_LatitudeProperty.doubleValue = MathUtility.Clamp(m_LatitudeProperty.doubleValue, -GeoLocationModule.MaxLatitude, GeoLocationModule.MaxLatitude);
                    m_LongitudeProperty.doubleValue = MathUtility.Clamp(m_LongitudeProperty.doubleValue, -GeoLocationModule.MaxLongitude, GeoLocationModule.MaxLongitude);
                    PropagateGeoLocationChanges();
                }
            }

            m_ShortcutButtonsFoldout = GeoLocationShortcutButtons.DrawShortcutButtons("Debug Geolocation Shortcuts", (latitude, longitude) =>
            {
                m_LatitudeProperty.doubleValue = latitude;
                m_LongitudeProperty.doubleValue = longitude;
                PropagateGeoLocationChanges();
            }, true, m_ShortcutButtonsFoldout);

            serializedObject.ApplyModifiedProperties();
        }

        void PropagateGeoLocationChanges()
        {
            serializedObject.ApplyModifiedProperties();
            GeoLocationModule.instance.AddOrUpdateLocationTrait();
            var foundReasoningApi = ReasoningModule.instance.FindReasoningAPI<GeoDuplicationReasoningAPI>();
            if (foundReasoningApi)
            {
                foundReasoningApi.ProcessScene();
            }
            QuerySimulationModule.instance.RestartSimulationIfNeeded();
        }
    }
}
