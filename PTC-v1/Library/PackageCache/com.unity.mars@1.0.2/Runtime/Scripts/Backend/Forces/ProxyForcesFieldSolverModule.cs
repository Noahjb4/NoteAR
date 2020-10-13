using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.MARS.Forces.ForceFieldSolving;
using Unity.XRTools.ModuleLoader;
using Unity.XRTools.Utils;

namespace Unity.MARS.Forces
{
    [ModuleOrder(ModuleOrders.ForcesFieldSolverLoadOrder)]
    [ScriptableSettingsPath(MARSCore.SettingsFolder)]
    class ProxyForcesFieldSolverModule : ScriptableSettings<ProxyForcesFieldSolverModule>, IModuleBehaviorCallbacks
    {
        ProxyForcesFieldSolver m_MainFieldSolver = new ProxyForcesFieldSolver();

#pragma warning disable 649
        [SerializeField]
        bool m_ShowForcesInSceneView;
#pragma warning restore 649

        internal ProxyForcesFieldSolver mainFieldSolver => m_MainFieldSolver;

        public void ResetMainGroup()
        {
            mainFieldSolver.Clear();
        }

        public static ProxyForcesFieldSolverModule EnsureInstance(GameObject from, ref ProxyForcesFieldSolverModule ptr)
        {
            if (ptr)
            {
                return ptr;
            }
            ptr = ProxyForcesFieldSolverModule.instance;
            return ptr;
        }

        public void OnBehaviorAwake()
        {
            ResetMainGroup();
        }

        public void OnBehaviorEnable()
        {
        }

        public void OnBehaviorStart()
        {
        }

        public void OnBehaviorUpdate()
        {
            mainFieldSolver.ShowForcesAsLines = this.m_ShowForcesInSceneView;
        }

        public void OnBehaviorDisable()
        {
        }

        public void OnBehaviorDestroy()
        {
            mainFieldSolver.Dispose();
        }

        public void LoadModule()
        {
            ResetMainGroup();
        }

        public void UnloadModule()
        {
            ResetMainGroup();
            mainFieldSolver.Dispose();
        }
    }
}
