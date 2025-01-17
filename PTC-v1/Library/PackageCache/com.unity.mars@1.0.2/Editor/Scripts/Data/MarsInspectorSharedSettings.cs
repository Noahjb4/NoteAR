﻿using UnityEngine;

namespace Unity.MARS
{
    /// <summary>
    /// A general settings object for values which should persist domain reload, be project-specific,
    /// and don't need to be saved to disk.
    /// </summary>
    public class MarsInspectorSharedSettings : ScriptableObject
    {
        static MarsInspectorSharedSettings m_Instance;

        [SerializeField]
        int m_ComponentTabSelection;

        public static MarsInspectorSharedSettings Instance
        {
            get
            {
                if (!m_Instance)
                {
                    var globalSettings = Resources.FindObjectsOfTypeAll<MarsInspectorSharedSettings>();
                    if (globalSettings.Length > 0)
                        m_Instance = globalSettings[0];
                }

                if (!m_Instance)
                    m_Instance = CreateInstance<MarsInspectorSharedSettings>();

                return m_Instance;
            }
        }

        /// <summary>
        /// The selected tab (Conditions / Actions / Fallbacks / Settings) on the Proxy editor
        /// </summary>
        public int ComponentTabSelection
        {
            get => m_ComponentTabSelection;
            set => m_ComponentTabSelection = value;
        }
    }
}
