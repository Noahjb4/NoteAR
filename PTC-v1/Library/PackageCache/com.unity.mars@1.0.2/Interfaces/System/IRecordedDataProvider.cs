using UnityEngine;

namespace Unity.MARS.Recording
{
    /// <summary>
    /// Interface for a recorded MR data provider
    /// </summary>
    public interface IRecordedDataProvider
    {
        /// <summary>
        /// Remove all provided data. This is called when a looping recording reaches its duration.
        /// </summary>
        void ClearData();
    }
}
