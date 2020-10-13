namespace Unity.MARS
{
    /// <summary>
    /// Provides access to callbacks from Replicator
    /// </summary>
    public interface ISpawnable : IMatchAcquireHandler, IMatchUpdateHandler, IMatchLossHandler
    {
    }
}
