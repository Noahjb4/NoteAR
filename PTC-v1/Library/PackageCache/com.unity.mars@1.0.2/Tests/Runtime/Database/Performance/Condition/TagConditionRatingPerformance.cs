#if UNITY_EDITOR
using UnityEngine;

namespace Unity.MARS.Data.Tests
{
    class TagConditionRatingPerformance : TimedConditionPerformanceTest<SemanticTagCondition, bool>
    {
        public virtual SemanticTagMatchRule matchRule { get; set; }
        
        public void Start()
        {
            m_Condition.matchRule = matchRule;
            for (int i = 0; i < s_DataCount; i++)
            {
                m_DataToCompare[i] = Random.Range(0f, 1f) > 0.5f;
            }
        }
    }
    
    class TagConditionInclusiveMatchRatingPerformance : TagConditionRatingPerformance
    {
        public override SemanticTagMatchRule matchRule { get { return SemanticTagMatchRule.Match; } }
    }
    
    class TagConditionExclusiveMatchRatingPerformance : TagConditionRatingPerformance
    {
        public override SemanticTagMatchRule matchRule { get { return SemanticTagMatchRule.Exclude; } }
    }
}
#endif
