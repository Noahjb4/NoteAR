using UnityEngine;

namespace Unity.MARS.Recording
{
    /// <summary>
    /// Event for facial expression recordings
    /// </summary>
    public struct FaceExpressionDataEvent
    {
        public double Time;
        public MRFaceExpression Expression;
        public float Coefficient;
        public bool Engaged;
    }
}
