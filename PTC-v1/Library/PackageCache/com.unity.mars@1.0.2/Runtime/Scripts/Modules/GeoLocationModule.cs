﻿using System;
using Unity.MARS.Data;
using Unity.XRTools.ModuleLoader;
using Unity.XRTools.Utils;
using UnityEngine;

namespace Unity.MARS
{
    [ScriptableSettingsPath(MARSCore.UserSettingsFolder)]
    [ModuleOrder(ModuleOrders.GeoLocationLoadOrder)]
    public class GeoLocationModule : ScriptableSettings<GeoLocationModule>,
        IModuleDependency<MARSDatabase>, IModuleDependency<SlowTaskModule>
    {
        public const float MaxLatitude = 90f;
        public const float MaxLongitude = 180f;

        // default simulated coordinates is San Francisco, USA
        const float k_DefaultLatitude = 37.7749f;
        const float k_DefaultLongitude = -122.4194f;

        [SerializeField]
        GeographicCoordinate m_CurrentLocation;

        // it's ok if these members aren't used in the Editor, so we disable that particular warning.
#pragma warning disable 0414
        [SerializeField]
        [Tooltip("Desired accuracy of the location service, in meters")]
        float m_DesiredAccuracy = 100f;

        [SerializeField]
        [Tooltip("Distance before a location updated event occurs")]
        float m_UpdateDistance = 25f;

        [SerializeField]
        [Tooltip("When enabled, the location service will continue to poll for updates after acquiring initial location")]
        bool m_ContinuousUpdates;

        [SerializeField] [Tooltip("How often to check for updates to geo location (in seconds)")]
        int m_UpdateInterval = 15;

        [SerializeField] [Tooltip("How long to wait for the location service to initialize before giving up")]
        int m_InitializationTimeout = 20;

        int m_InitializationTimeCounter;

        MARSDatabase m_Database;
        SlowTaskModule m_SlowTaskModule;

        public event Action<GeographicCoordinate> GeoLocationChanged;

#pragma warning restore

        /// <summary>
        /// When enabled, the location service will continue to poll for updates after acquiring initial location
        /// </summary>
        public bool continuousUpdates
        {
            get { return m_ContinuousUpdates; }
            set { m_ContinuousUpdates = value; }
        }

        public void ConnectDependency(MARSDatabase dependency)
        {
            m_Database = dependency;
        }

        public void ConnectDependency(SlowTaskModule dependency)
        {
            m_SlowTaskModule = dependency;
        }

        public void LoadModule()
        {
            // setup our static functionality injection for IUsesGeoLocation implementors
            IUsesGeoLocationMethods.TryGetGeoLocationAction = TryGetGeoLocation;
            IUsesGeoLocationMethods.TryStartServiceFunction = TryStartService;
            IUsesGeoLocationMethods.SubscribeGeoLocationChangedAction = SubscribeGeoLocationChanged;
            IUsesGeoLocationMethods.UnsubscribeGeoLocationChangedAction = UnsubscribeGeoLocationChanged;

#if !UNITY_EDITOR
            if(!Input.location.isEnabledByUser)
                Debug.LogWarning("Location service not enabled by user - Geolocation conditions will not work!");
#endif

            if (m_CurrentLocation.latitude == default && m_CurrentLocation.longitude == default)
                m_CurrentLocation = new Vector2(k_DefaultLatitude, k_DefaultLongitude);

            // since this is the only environment trait provider so far,  we add this environment semantic tag
            // to the reserved data ID here.  In the future it may / should live in a more central place.
            m_Database.GetTraitProvider(out MARSTraitDataProvider<bool> tagTraitProvider);
            tagTraitProvider?.AddOrUpdateTrait((int)ReservedDataIDs.ImmediateEnvironment, TraitNames.Environment, true);
            TryStartService();
        }

        void SubscribeGeoLocationChanged(Action<GeographicCoordinate> action)
        {
            GeoLocationChanged += action;
        }

        void UnsubscribeGeoLocationChanged(Action<GeographicCoordinate> action)
        {
            GeoLocationChanged -= action;
        }

        internal void AddOrUpdateLocationTrait()
        {
            m_Database.GetTraitProvider(out MARSTraitDataProvider<Vector2> vector2TraitProvider);
            vector2TraitProvider?.AddOrUpdateTrait((int)ReservedDataIDs.ImmediateEnvironment, TraitNames.Geolocation, m_CurrentLocation);
            GeoLocationChanged?.Invoke(m_CurrentLocation);
        }

        /// <summary>
        /// Given a GeographicCoordinate, immediately update the GeoLocationModule and MARS database with it
        /// </summary>
        public void SetCurrentLocationAndUpdateTrait(GeographicCoordinate location)
        {
            m_CurrentLocation = location;
            AddOrUpdateLocationTrait();
        }

        public void UnloadModule()
        {
            m_InitializationTimeCounter = 0;

            switch (Input.location.status)
            {
                case LocationServiceStatus.Initializing:
                    m_SlowTaskModule.RemoveSlowTask(ServiceStartTask);
                    break;
                case LocationServiceStatus.Running:
                    if (m_ContinuousUpdates)
                        m_SlowTaskModule.RemoveSlowTask(UpdateLocationTask);
                    break;
            }
        }

        void UpdateLocationTask()
        {
            if (Input.location.status != LocationServiceStatus.Running)
                return;

            var location = Input.location.lastData;
            if (location.latitude != m_CurrentLocation.latitude ||
                location.longitude != m_CurrentLocation.longitude)
            {
                m_CurrentLocation = location;
                AddOrUpdateLocationTrait();
            }
        }

        void ServiceStartTask()
        {
            var service = Input.location;
            switch(service.status)
            {
                case LocationServiceStatus.Failed:
                {
                    if (MARSDebugSettings.geoLocationModuleLogging)
                    {
                        if(!service.isEnabledByUser)
                            Debug.LogWarning("Location service start failed - user has location disabled");
                        else
                            Debug.LogWarning("Location service start failed - unknown reason");
                    }
                    m_SlowTaskModule.RemoveSlowTask(ServiceStartTask);
                    break;
                }
                case LocationServiceStatus.Initializing:
                {
                    if (m_InitializationTimeCounter > 0)
                    {
                        m_InitializationTimeCounter--;
                    }
                    else
                    {
                        if (MARSDebugSettings.geoLocationModuleLogging)
                            Debug.LogWarningFormat("Location service start timed out after {0} seconds", m_InitializationTimeout);

                        m_SlowTaskModule.RemoveSlowTask(ServiceStartTask);
                    }
                    break;
                }
                case LocationServiceStatus.Running:
                {
                    m_SlowTaskModule.RemoveSlowTask(ServiceStartTask);
                    if(m_ContinuousUpdates)
                        m_SlowTaskModule.AddSlowTask(UpdateLocationTask, m_UpdateInterval);

                    m_CurrentLocation = Input.location.lastData;
                    AddOrUpdateLocationTrait();

                    // since GPS has significant power overhead, unless we've explicitly asked for continuous updates,
                    // stop the service once we've ascertained location.  It can be restarted later if needed.
                    if (!m_ContinuousUpdates)
                        Input.location.Stop();

                    break;
                }
            }
        }

        public bool TryStartService()
        {
            m_InitializationTimeCounter = m_InitializationTimeout;

            var status = Input.location.status;
            if (status == LocationServiceStatus.Running || status == LocationServiceStatus.Initializing)
                return false;

#if !UNITY_EDITOR || true
            Input.location.Start(m_DesiredAccuracy, m_UpdateDistance);
            m_SlowTaskModule.AddSlowTask(ServiceStartTask, 1f);
            AddOrUpdateLocationTrait();
#endif
            return true;
        }

        public bool TryGetGeoLocation(out GeographicCoordinate coordinate)
        {
#if UNITY_EDITOR
            coordinate = m_CurrentLocation;
            return true;
#else
            if (Input.location.status == LocationServiceStatus.Running)
            {
                coordinate = Input.location.lastData;
                m_CurrentLocation = coordinate;
                return true;
            }

            coordinate = GeographicCoordinate.Invalid;
            return false;
#endif
        }
    }
}
