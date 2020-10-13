using Unity.XRTools.Utils;
using UnityEngine;

namespace Unity.MARS
{
    internal class MarsObjectCreationResources : EditorScriptableSettings<MarsObjectCreationResources>
    {
#pragma warning disable 649
        [Header("Primitives")]
        [SerializeField]
        ObjectCreationButtonData m_ProxyObjectPreset;

        [SerializeField]
        ObjectCreationButtonData m_ProxyGroupPreset;

        [SerializeField]
        ObjectCreationButtonData m_ReplicatorPreset;

        [SerializeField]
        ObjectCreationButtonData m_SyntheticPreset;

        [Header("Presets")]
        [SerializeField]
        ObjectCreationButtonData m_HorizontalPlanePreset;

        [SerializeField]
        ObjectCreationButtonData m_VerticalPlanePreset;

        [SerializeField]
        ObjectCreationButtonData m_ImageMarkerPreset;

        [SerializeField]
        ObjectCreationButtonData m_FaceMaskPreset;

        [Header("Visualizers")]
        [SerializeField]
        ObjectCreationButtonData m_PlaneVisualsPreset;

        [SerializeField]
        ObjectCreationButtonData m_PointCloudVisualsPreset;

        [SerializeField]
        ObjectCreationButtonData m_FaceLandmarkVisualsPreset;

        [Header("Simulated")]
        [SerializeField]
        ObjectCreationButtonData m_SyntheticImageMarkerPreset;
#pragma warning restore 649

        public ObjectCreationButtonData ProxyObjectPreset => m_ProxyObjectPreset;
        public ObjectCreationButtonData ProxyGroupPreset => m_ProxyGroupPreset;
        public ObjectCreationButtonData ReplicatorPreset => m_ReplicatorPreset;
        public ObjectCreationButtonData SyntheticPreset => m_SyntheticPreset;

        public ObjectCreationButtonData HorizontalPlanePreset => m_HorizontalPlanePreset;
        public ObjectCreationButtonData VerticalPlanePreset => m_VerticalPlanePreset;
        public ObjectCreationButtonData ImageMarkerPreset => m_ImageMarkerPreset;
        public ObjectCreationButtonData FaceMaskPreset => m_FaceMaskPreset;

        public ObjectCreationButtonData PlaneVisualsPreset => m_PlaneVisualsPreset;
        public ObjectCreationButtonData PointCloudVisualsPreset => m_PointCloudVisualsPreset;
        public ObjectCreationButtonData FaceLandmarkVisualsPreset => m_FaceLandmarkVisualsPreset;

        public ObjectCreationButtonData SyntheticImageMarkerPreset => m_SyntheticImageMarkerPreset;
    }
}
