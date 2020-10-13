using Unity.MARS.Query;

namespace Unity.MARS
{
    /// <summary>
    /// Allows a component on a Set to receive callbacks for when a query match has not been found in time
    /// </summary>
    public interface ISetMatchTimeoutHandler : IAction, ISimulatable
    {
        /// <summary>
        /// Called when no query match has been found in time
        /// </summary>
        /// <param name="queryArgs">The original query associated with this object</param>
        void OnSetMatchTimeout(SetQueryArgs queryArgs);
    }
}
