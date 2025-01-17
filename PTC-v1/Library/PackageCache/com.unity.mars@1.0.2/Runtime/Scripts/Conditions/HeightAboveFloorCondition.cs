using UnityEngine;

namespace Unity.MARS
{
    /// <summary>
    /// Represents a situation where a given plane must be within a certain height off the floor
    /// </summary>
    [DisallowMultipleComponent]
    [ComponentTooltip("Requires the object be raised above the floor.")]
    [MonoBehaviourComponentMenu(typeof(HeightAboveFloorCondition), "Condition/Height Above Floor")]
    [CreateFromDataOptions(0, false)]
    public class HeightAboveFloorCondition : Condition<float>, ICreateFromData
    {
        static readonly TraitRequirement[] k_RequiredTraits = { TraitDefinitions.HeightAboveFloor };

        [SerializeField]
        float m_IdealHeight = 1.5f;

        [SerializeField]
        float m_RangeFromIdealHeight = 2.0f;

        [SerializeField]
        bool m_RequireInRange = false;

        public override TraitRequirement[] GetRequiredTraits() { return k_RequiredTraits; }

        public override float RateDataMatch(ref float height)
        {
            var diff = height - m_IdealHeight;
            var absHeightDiff = diff >= 0 ? diff : -diff; // Abs
            var unitDist = absHeightDiff / m_RangeFromIdealHeight;

            if (m_RequireInRange)
                return ((unitDist > 1.0f) ? 0f : (1.0f - unitDist));

            if (unitDist > 1.0f)
                unitDist = 1.0f;
            // shift distance from 0~1 to 0.0~0.5 range to ensure it succeeds
            unitDist *= 0.5f;
            return 1f - unitDist;
        }

        public void OptimizeForData(TraitDataSnapshot data)
        {
            if (data.TryGetTrait(TraitNames.HeightAboveFloor, out float height))
            {
                m_IdealHeight = height;
            }
        }

        public void IncludeData(TraitDataSnapshot data)
        {
            if (data.TryGetTrait(TraitNames.HeightAboveFloor, out float height))
            {
                var dist = Mathf.Abs(m_IdealHeight - height) * (2.0f);
                if (dist > m_RangeFromIdealHeight)
                {
                    m_RangeFromIdealHeight = dist;
                }
            }
        }

        public string FormatDataString(TraitDataSnapshot data)
        {
            if (!data.TryGetTrait(TraitNames.HeightAboveFloor, out float height))
            {
                height = m_IdealHeight;
            }

            return ((!m_RequireInRange) ? $"~{height:0.00}m" : $"{height:0.00}m +/- {m_RangeFromIdealHeight:0.00}m");
        }

        public float GetConditionRatingForData(TraitDataSnapshot data)
        {
            if (data.TryGetTrait(TraitNames.HeightAboveFloor, out float height))
            {
                var rating = this.RateDataMatch(ref height);
                return rating;
            }
            else
            {
                return 0.0f;
            }
        }
    }
}
