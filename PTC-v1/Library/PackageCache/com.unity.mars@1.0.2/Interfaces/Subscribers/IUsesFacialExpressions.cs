using System;
using Unity.XRTools.ModuleLoader;

namespace Unity.MARS
{
    /// <summary>
    /// Defines the API for a facial expression consumer
    /// </summary>
    public interface IUsesFacialExpressions : IFunctionalitySubscriber<IProvidesFacialExpressions>, IFaceFeature
    {
    }

    public static class IUsesFacialExpressionsMethods
    {
        /// <summary>
        /// Subscribe to the facial expression
        /// </summary>
        public static void SubscribeToExpression(this IUsesFacialExpressions obj, MRFaceExpression expression, Action<float> onEngage, Action<float> onDisengage = null)
        {
            obj.provider.SubscribeToExpression(expression, onEngage, onDisengage);
        }

        /// <summary>
        /// Unsubscribe from the facial expression
        /// </summary>
        public static void UnsubscribeToExpression(this IUsesFacialExpressions obj, MRFaceExpression expression, Action<float> onEngage, Action<float> onDisengage = null)
        {
            obj.provider.UnsubscribeToExpression(expression, onEngage, onDisengage);
        }
    }
}
