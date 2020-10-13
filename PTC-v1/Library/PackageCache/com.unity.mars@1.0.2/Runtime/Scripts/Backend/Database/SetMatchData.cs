using System.Collections.Generic;
using Unity.MARS.Query;

namespace Unity.MARS.Data
{
    /// <summary>
    /// Contains information about which data is used for each child in a set match.
    /// </summary>
    public class SetMatchData
    {
        public Dictionary<IMRObject, int> dataAssignments;
        public Dictionary<IMRObject, Exclusivity> exclusivities;
    }
}
