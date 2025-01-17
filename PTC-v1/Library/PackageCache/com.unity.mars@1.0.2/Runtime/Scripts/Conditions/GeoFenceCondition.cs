﻿using Unity.MARS.Data;
using Unity.XRTools.Utils;
using UnityEngine;

namespace Unity.MARS
{
    /// <summary>
    /// Represents a situation that depends on the user's geolocation
    /// </summary>
    [ComponentTooltip("Requires the object to be inside or outside of the specified geolocation.")]
    [MonoBehaviourComponentMenu(typeof(GeoFenceCondition), "Condition/GeoFence")]
    public class GeoFenceCondition : Condition<Vector2>
    {
        enum GeoFenceRule
        {
            Inside,
            Outside
        }

        static readonly TraitRequirement[] k_RequiredTraits = { TraitDefinitions.GeoCoordinate };

#pragma warning disable 649
        [SerializeField]
        GeographicBoundingBox m_BoundingBox;

        [SerializeField]
        [Tooltip("Whether the condition relies on the user being inside or outside the fence")]
        GeoFenceRule m_Rule;
#pragma warning restore 649

#if UNITY_EDITOR
        public override void OnValidate()
        {
            base.OnValidate();
            m_BoundingBox.center.latitude = MathUtility.Clamp(m_BoundingBox.center.latitude, -GeoLocationModule.MaxLatitude, GeoLocationModule.MaxLatitude);
            m_BoundingBox.center.longitude = MathUtility.Clamp(m_BoundingBox.center.longitude, -GeoLocationModule.MaxLongitude, GeoLocationModule.MaxLongitude);

            // the 5th decimal place is worth ~1.1m in length at the equator, and trends toward 0m closer to a pole.
            // This was chosen as the minimum extents due to better resolution with phone / headset GPS being unlikely.
            // https://en.wikipedia.org/wiki/Decimal_degrees
            const double minimumExtents = 0.00001;
            m_BoundingBox.latitudeExtents = MathUtility.Clamp(m_BoundingBox.latitudeExtents, minimumExtents, GeoLocationModule.MaxLatitude);
            m_BoundingBox.longitudeExtents = MathUtility.Clamp(m_BoundingBox.longitudeExtents, minimumExtents, GeoLocationModule.MaxLongitude);
        }
#endif

        public override TraitRequirement[] GetRequiredTraits() { return k_RequiredTraits; }

        // unless we change this sort of condition to care about how close you are to some geographic point,
        // a binary pass/fail answer is still suitable, since we're defining a boundary
        public override float RateDataMatch(ref Vector2 data)
        {
            if (m_Rule == GeoFenceRule.Inside)
                return IsInside(data) ? 1f : 0f;

            return IsInside(data) ? 0f : 1f;
        }

        bool IsInside(Vector2 data)
        {
            var latitudeDistance = MathUtility.ShortestAngleDistance(
                m_BoundingBox.center.latitude, data.x, GeoLocationModule.MaxLatitude, 180f);
            var longitudeDistance = MathUtility.ShortestAngleDistance(
                m_BoundingBox.center.longitude, data.y, GeoLocationModule.MaxLongitude, 360f);

            return (latitudeDistance < 0 ? -latitudeDistance : latitudeDistance) < m_BoundingBox.latitudeExtents &&
                (longitudeDistance < 0 ? -longitudeDistance : longitudeDistance) < m_BoundingBox.longitudeExtents;
        }
    }
}
