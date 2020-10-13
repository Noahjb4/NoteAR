using System;
using UnityEngine;

namespace Unity.MARS
{
    [ExecuteAlways]
    [AddComponentMenu("")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public class CompositeCameraRenderer : MonoBehaviour
    {
        public event Action PreCullCamera;
        public event Action PostRenderCamera;

        void OnPreCull()
        {
            PreCullCamera?.Invoke();
        }

        void OnPostRender()
        {
            PostRenderCamera?.Invoke();
        }
    }
}
