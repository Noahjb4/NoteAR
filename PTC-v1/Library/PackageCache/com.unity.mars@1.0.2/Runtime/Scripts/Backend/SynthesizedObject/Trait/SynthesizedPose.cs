using System;
using Unity.XRTools.ModuleLoader;
using UnityEngine;

namespace Unity.MARS.Data
{
    /// <summary>
    /// Create the data for a pose trait
    /// When added to a synthesized object, adds a pose in the form of the GameObject's world position
    /// to its representation in the database
    /// </summary>
    public class SynthesizedPose : SynthesizedTrait<Pose>, IUsesCameraOffset
    {
        public override string TraitName { get { return TraitNames.Pose; } }
        public override bool UpdateWithTransform { get { return true; } }

        IProvidesCameraOffset IFunctionalitySubscriber<IProvidesCameraOffset>.provider { get; set; }

        public override Pose GetTraitData()
        {
            return this.ApplyInverseOffsetToPose(new Pose(transform.position, transform.rotation));
        }
    }
}
