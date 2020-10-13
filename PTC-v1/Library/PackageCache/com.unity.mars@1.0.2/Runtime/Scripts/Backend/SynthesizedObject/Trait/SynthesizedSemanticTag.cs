using System;
using UnityEngine;

namespace Unity.MARS.Data
{
    /// <summary>
    /// Creates data for a semantic tag trait
    /// When added to a synthesized object, adds a semantic tag to its representation in the database
    /// </summary>
    public class SynthesizedSemanticTag : SynthesizedTrait<bool>
    {
        [SerializeField]
        [Tooltip("The semantic tag to apply to the Synthesized Object")]
        string m_SemanticTag;

        void OnValidate()
        {
            if (m_SemanticTag != null)
            {
                m_SemanticTag = m_SemanticTag.ToLower();
            }
        }

        public override string TraitName { get { return m_SemanticTag; } }

        public override bool UpdateWithTransform { get { return false; } }

        public override bool GetTraitData() { return true; }
    }
}
