﻿#if UNITY_EDITOR
using UnityEngine;

namespace Unity.MARS.Data.Tests
{
    class PlaneSizeRatingPerformance : TimedConditionPerformanceTest<PlaneSizeCondition, Vector2>
    {
        public void Start()
        {
            m_Condition.minimumSize = new Vector2(1f, 1f);
            m_Condition.maximumSize = new Vector2(2f, 2f);
            
            for (int i = 0; i < s_DataCount; i++)
            {
                m_DataToCompare[i] = Random.insideUnitCircle * 5f;
            }
        }

        protected override void Update()
        {
            m_Condition.maximumSize += Random.insideUnitCircle * 0.01f;
            m_Condition.minimumSize += Random.insideUnitCircle * 0.01f;
            RunTestIteration();
        }
    }
}
#endif
