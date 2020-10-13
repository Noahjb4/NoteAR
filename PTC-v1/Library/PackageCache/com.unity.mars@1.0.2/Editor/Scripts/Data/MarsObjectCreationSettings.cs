using System;
using Unity.XRTools.Utils;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace Unity.MARS
{
    public class MarsObjectCreationSettings : EditorScriptableSettings<MarsObjectCreationSettings>
    {
        [Serializable]
        internal class ObjectCreationButtonSection
        {
            [SerializeField]
            string m_Name;

            [SerializeField]
            ObjectCreationButtonData[] m_Buttons;

            [SerializeField]
            bool m_Expanded;
            
            public string Name => m_Name;

            public ObjectCreationButtonData[] Buttons => m_Buttons;

            public bool Expanded
            {
                get => m_Expanded;
                set => m_Expanded = value;
            }

            public ObjectCreationButtonSection()
            {
                m_Name = "New Group";
                m_Expanded = true;
            }

            public ObjectCreationButtonSection(string name, bool expanded, ObjectCreationButtonData[] buttons)
            {
                m_Name = name;
                m_Buttons = buttons;
                m_Expanded = expanded;
            }
        }
        
        [SerializeField]
        ObjectCreationButtonSection[] m_ObjectCreationButtonSections;

        internal ObjectCreationButtonSection[] ObjectCreationButtonSections => m_ObjectCreationButtonSections;

        protected override void OnLoaded()
        {
            base.OnLoaded();
            if (m_ObjectCreationButtonSections == null || m_ObjectCreationButtonSections.Length < 1)
            {
                ResetToDefault();
            }
        }

        void Reset()
        {
            ResetToDefault();

            s_Instance = this;
        }

        void ResetToDefault()
        {
            m_ObjectCreationButtonSections = new[]
            {
                new ObjectCreationButtonSection("Templates", true, new[]
                {
                    MarsObjectCreationResources.instance.HorizontalPlanePreset,
                    MarsObjectCreationResources.instance.VerticalPlanePreset,
                    MarsObjectCreationResources.instance.ImageMarkerPreset,
                    MarsObjectCreationResources.instance.FaceMaskPreset
                }),
                new ObjectCreationButtonSection("Primitives", true, new[]
                {
                    MarsObjectCreationResources.instance.ProxyObjectPreset,
                    MarsObjectCreationResources.instance.ProxyGroupPreset,
                    MarsObjectCreationResources.instance.ReplicatorPreset,
                    MarsObjectCreationResources.instance.SyntheticPreset
                }),
                new ObjectCreationButtonSection("Visualizers", true, new[]
                {
                    MarsObjectCreationResources.instance.PlaneVisualsPreset,
                    MarsObjectCreationResources.instance.PointCloudVisualsPreset,
                    MarsObjectCreationResources.instance.FaceLandmarkVisualsPreset
                }),
                new ObjectCreationButtonSection("Simulated", true, new[]
                {
                    MarsObjectCreationResources.instance.SyntheticImageMarkerPreset
                })
            };
        }

        void OnValidate()
        {
            if (m_ObjectCreationButtonSections == null || m_ObjectCreationButtonSections.Length < 1)
            {
                ResetToDefault();
            }
        }
    }
}
