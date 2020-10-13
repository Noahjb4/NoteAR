using Unity.MARS.Query;

namespace Unity.MARS.Data
{
    /// <summary>
    /// Container for all arguments required to find the best match for a set of Conditions
    /// </summary>
    public class TryBestMatchArguments
    {
        /// <summary>
        /// The set of Conditions to find data matches for
        /// </summary>
        public Conditions conditions;

        /// <summary>
        /// The query's data exclusivity setting
        /// </summary>
        public Exclusivity exclusivity;

        /// <summary>
        /// Pre-allocated collections used during the calculation of match ratings
        /// </summary>
        internal ConditionRatingsData ratings;

        public TryBestMatchArguments(Conditions conditions, Exclusivity exclusivity)
        {
            this.conditions = conditions;
            this.exclusivity = exclusivity;
            ratings = new ConditionRatingsData(conditions);
        }

        /// <summary>
        /// Use this overload to get arguments for the next instance of a replicated Proxy
        /// </summary>
        /// <param name="original">The original / previous spawn's arguments</param>
        public TryBestMatchArguments(TryBestMatchArguments original)
        {
            conditions = original.conditions;
            exclusivity = original.exclusivity;
            // we can re-use the internal ratings structure for all instances of a spawn
            ratings = original.ratings;
        }
    }
}
