using UnityEngine;

namespace Unity.MARS.Recording
{
    /// <summary>
    /// Interface for a recorded MR data provider that updates its data each time the Timeline PlayableGraph is evaluated
    /// </summary>
    public interface ISteppableRecordedDataProvider : IRecordedDataProvider
    {
        /// <summary>
        /// Update provided data. This is called after the Timeline PlayableGraph is evaluated, due to either a
        /// Mars Update or a manual change in time.
        /// </summary>
        void StepRecordedData();
    }
}
