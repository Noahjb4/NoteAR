using Unity.XRTools.Utils;
using UnityEditor;
using UnityEngine;

namespace Unity.MARS
{
    [ScriptableSettingsPath(MARSCore.UserSettingsFolder)]
    public class MARSUserPreferences : ScriptableSettings<MARSUserPreferences>
    {
        const string k_ResetColorsEvent = "Reset Colors";

        static readonly Color k_DefaultHighlightedSimulatedObjectColor = Color.yellow;
        static readonly Color k_DefaultConditionFailTextColor = Color.red;

        [Header("Colors")]

        [SerializeField]
        [Tooltip("Text color used to highlight the simulated versions of selected content in the MARS Hierarchy")]
        Color m_HighlightedSimulatedObjectColor = k_DefaultHighlightedSimulatedObjectColor;

        [SerializeField]
        [Tooltip("Text color used by the MARS Compare Tool for data that fails to pass a Condition")]
        Color m_ConditionFailTextColor = k_DefaultConditionFailTextColor;

        [Header("Planes Extraction")]
#pragma warning disable 649
        [SerializeField]
        bool m_ExtractPlanesOnSave;
#pragma warning restore 649

        [Header("World Scale")]

        [SerializeField]
        bool m_ScaleEntityPositions = true;

#pragma warning disable 649
        [SerializeField]
        bool m_ScaleEntityChildren;

        [SerializeField]
        bool m_ScaleSceneAudio;

        [SerializeField]
        bool m_ScaleSceneLighting;
#pragma warning restore 649

        [SerializeField]
        bool m_ScaleClippingPlanes = true;

        [Header("MARS Views")]

        [SerializeField]
        bool m_RestrictCameraToEnvironmentBounds = true;

        [Header("MARS Image Markers")]
        [SerializeField]
        bool m_TintImageMarkers = false;

        [Header("MARS ElectiveExtensions")]
        [SerializeField]
        bool m_ElectiveExtensionsReportsErrors = false;


        public Color highlightedSimulatedObjectColor { get { return m_HighlightedSimulatedObjectColor; } }
        public Color conditionFailTextColor { get { return m_ConditionFailTextColor; } }

        public bool extractPlanesOnSave { get { return m_ExtractPlanesOnSave; } }

        public bool scaleEntityPositions { get { return m_ScaleEntityPositions; } }
        public bool scaleEntityChildren { get { return m_ScaleEntityChildren; } }
        public bool scaleSceneAudio { get { return m_ScaleSceneAudio; } }
        public bool scaleSceneLighting { get { return m_ScaleSceneLighting; } }
        public bool scaleClippingPlanes { get { return m_ScaleClippingPlanes; } }

        public bool RestrictCameraToEnvironmentBounds { get { return m_RestrictCameraToEnvironmentBounds; } }

        public bool TintImageMarkers { get { return m_TintImageMarkers; } }

        public bool ElectiveExtensionsReportsErrors { get { return m_ElectiveExtensionsReportsErrors; } }

        public void ResetColors()
        {
            m_HighlightedSimulatedObjectColor = k_DefaultHighlightedSimulatedObjectColor;
            m_ConditionFailTextColor = k_DefaultConditionFailTextColor;

            EditorEvents.UiComponentUsed.Send(new UiComponentArgs { label = k_ResetColorsEvent, active = true });

            SceneView.RepaintAll();
        }
    }
}
