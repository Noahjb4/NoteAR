using Unity.MARS.Forces.EditorExtensions;
using UnityEngine;

namespace Unity.MARS.Forces
{
    [ComponentEditor(typeof(ProxyForces))]
    public class ProxyForcesInspector : ComponentInspector
    {

        public override void OnSceneGUI()
        {
            base.OnSceneGUI();
            var forces = (target as ProxyForces);
            if (forces)
            {
                ProxyForcesHandles.main.CustomForcesHandles(forces);
            }
        }
    }
}
