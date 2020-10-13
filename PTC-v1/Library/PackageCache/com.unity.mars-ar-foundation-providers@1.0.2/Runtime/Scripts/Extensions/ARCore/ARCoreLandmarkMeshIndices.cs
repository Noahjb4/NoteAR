using Unity.XRTools.Utils;
using UnityEngine;

namespace Unity.MARS.Providers
{
    public class ARCoreLandmarkMeshIndices : ScriptableSettings<ARCoreLandmarkMeshIndices>
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
