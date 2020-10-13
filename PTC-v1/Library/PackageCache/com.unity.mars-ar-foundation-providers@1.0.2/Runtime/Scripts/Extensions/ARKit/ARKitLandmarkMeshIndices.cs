using Unity.XRTools.Utils;
using UnityEngine;

namespace Unity.MARS.Providers
{
    public class ARKitLandmarkMeshIndices : ScriptableSettings<ARKitLandmarkMeshIndices>
    {
        [SerializeField]
        int[] m_LandmarkTriangleIndexes;

        public int[] landmarkTriangleIndices
        {
            get { return m_LandmarkTriangleIndexes; }
            set { m_LandmarkTriangleIndexes = value; }
        }
    }
}
