﻿using System.Collections.Generic;
using Unity.MARS.Data;
using UnityEngine;
using UnityEngine.XR;

namespace Unity.MARS
{
    /// <summary>
    /// Attaches the level of immersion a user has to their entry in the MARS Database.
    /// 0 is flat device, such as a phone or tablet, while 1 includes immersive displays like HMDs
    /// </summary>
    [CreateAssetMenu(menuName = "MARS/Immersion LevelReasoningAPI")]
    public class ImmersionReasoningAPI : ScriptableObject, IReasoningAPI, IProvidesTraits<bool>, IRequiresTraits
    {
        const float k_ProcessSceneTime = 5f;
        static readonly TraitRequirement[] k_RequiredTraits = { TraitDefinitions.User };
        static readonly TraitDefinition[] k_ProvidedTraits = { TraitDefinitions.DisplayFlat, TraitDefinitions.DisplaySpatial };

        [SerializeField]
        [Tooltip("If the simulated user should be flat or spatial")]
        bool m_SimulateSpatialDevice = false;

        bool m_LastDeviceSpatial = true;

        bool m_ImmersionSet = false;

        public float processSceneInterval => k_ProcessSceneTime;

        public TraitDefinition[] GetProvidedTraits() { return k_ProvidedTraits; }

        public TraitRequirement[] GetRequiredTraits() { return k_RequiredTraits; }

        public void Setup()
        {
            m_ImmersionSet = false;
            UpdateImmersionLevel();
        }

        public void TearDown()
        {
            this.RemoveTrait((int)ReservedDataIDs.LocalUser, TraitNames.DisplayFlat);
            this.RemoveTrait((int)ReservedDataIDs.LocalUser, TraitNames.DisplaySpatial);
            m_ImmersionSet = false;
        }

        public void ProcessScene()
        {
            // A user's immersion-level won't change at runtime - we only perform this check periodically in the editor
#if UNITY_EDITOR
            UpdateImmersionLevel();
#endif
        }

        public void UpdateData() { }

        void UpdateImmersionLevel()
        {
            // Editor path - just check if the user has changed the simulated mode
            if (Application.isEditor)
            {
                if (!m_ImmersionSet || m_SimulateSpatialDevice != m_LastDeviceSpatial)
                {
                    this.RemoveTrait((int)ReservedDataIDs.LocalUser, m_SimulateSpatialDevice ? TraitNames.DisplayFlat : TraitNames.DisplaySpatial);
                    this.AddOrUpdateTrait((int)ReservedDataIDs.LocalUser, m_SimulateSpatialDevice ? TraitNames.DisplaySpatial : TraitNames.DisplayFlat, true);
                    m_LastDeviceSpatial = m_SimulateSpatialDevice;
                    m_ImmersionSet = true;
                }
                return;
            }

            // Application path - look up platform specific data and then dive deeper into SDK specific data if needed
            if (m_ImmersionSet)
                return;

            var targetDeviceSpatial = false;

            var stereoRendererList = new List<XRDisplaySubsystem>();
            SubsystemManager.GetInstances(stereoRendererList);
            if (stereoRendererList.Count > 0)
                targetDeviceSpatial = true;

            this.RemoveTrait((int)ReservedDataIDs.LocalUser, m_SimulateSpatialDevice ? TraitNames.DisplayFlat : TraitNames.DisplaySpatial);
            this.AddOrUpdateTrait((int)ReservedDataIDs.LocalUser, m_SimulateSpatialDevice ? TraitNames.DisplaySpatial : TraitNames.DisplayFlat, true);
            m_LastDeviceSpatial = targetDeviceSpatial;
            m_ImmersionSet = true;
        }
    }
}
