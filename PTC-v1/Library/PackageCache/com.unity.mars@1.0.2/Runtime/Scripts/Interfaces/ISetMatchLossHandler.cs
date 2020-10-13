using Unity.MARS.Query;

namespace Unity.MARS
{
    /// <summary>
    /// Allows a component on a Set to receive callbacks for when a query match is lost
    /// </summary>
    public interface ISetMatchLossHandler : IAction, ISimulatable
    {
        /// <summary>
        /// Called when a query match has been lost
        /// </summary>
        /// <param name="queryResult">Data associated with this event</param>
        void OnSetMatchLoss(SetQueryResult queryResult);
    }
}
