using System.Collections.Generic;
using Unity.XRTools.ModuleLoader;
using Unity.XRTools.Utils;
using UnityEditor;
using UnityEngine;

namespace Unity.MARS
{
    public class PlaneGenerationModule : IModule, IUsesPlaneFinding
    {
        const string k_SavePlanesDialogTitle = "Saving Planes";
        const string k_NoPlanesDialogMessage = "No planes have been discovered in simulation. Cannot save planes to environment.";
        const string k_NoPlanesDialogOk = "OK";
        const string k_ConfirmDestroyDialogMessage = "This action will destroy all previously generated planes. If you want " +
            "to preserve any planes you can move them out of the parent game object that has the component {0}. Do you want to continue?";

        const string k_ConfirmDestroyDialogOk = "Yes";
        const string k_ConfirmDestroyDialogCancel = "No";

        internal static bool DestroyedPlanes;

        public IProvidesPlaneFinding provider { get; set; }

        // Local method use only -- created here to reduce garbage collection. Collections must be cleared before use
        // Reference type collections must also be cleared after use
        static readonly List<MRPlane> k_Planes = new List<MRPlane>();
        static readonly List<GeneratedPlanesRoot> k_PlanesRoots = new List<GeneratedPlanesRoot>();

        public void LoadModule() { }

        public void UnloadModule() { }

        public static void TryDestroyPreviousPlanes(GameObject environmentRoot, string confirmDestroyDialogTitle, UndoBlock undoBlock)
        {
            // k_PlanesRoots is cleared by GetComponentsInChildren
            environmentRoot.GetComponentsInChildren(k_PlanesRoots);

            var anyChildrenToDestroy = false;
            foreach (var planesRoot in k_PlanesRoots)
            {
                if (planesRoot.transform.childCount > 0)
                {
                    anyChildrenToDestroy = true;
                    break;
                }
            }

            DestroyedPlanes = true;
            if (anyChildrenToDestroy)
            {
                DestroyedPlanes = EditorUtility.DisplayDialog(confirmDestroyDialogTitle,
                    string.Format(k_ConfirmDestroyDialogMessage, nameof(GeneratedPlanesRoot)),
                    k_ConfirmDestroyDialogOk, k_ConfirmDestroyDialogCancel);
            }

            if (DestroyedPlanes)
            {
                foreach (var planesRoot in k_PlanesRoots)
                {
                    Undo.DestroyObjectImmediate(planesRoot.gameObject);
                }
            }

            k_PlanesRoots.Clear();
        }

        internal static Transform CreateGeneratedPlanesRoot(Transform parent, UndoBlock undoBlock)
        {
            var planesRoot = new GameObject(GeneratedPlanesRoot.PlanesRootName, typeof(GeneratedPlanesRoot)).transform;
            planesRoot.SetParent(parent);
            undoBlock.RegisterCreatedObject(planesRoot.gameObject);
            return planesRoot;
        }

        public void SavePlanesFromSimulation(GameObject environmentRoot)
        {
            using (var undoBlock = new UndoBlock("Save Planes From Simulation"))
            {
                k_Planes.Clear();
                this.GetPlanes(k_Planes);
                if (k_Planes.Count == 0)
                {
                    EditorUtility.DisplayDialog(k_SavePlanesDialogTitle, k_NoPlanesDialogMessage, k_NoPlanesDialogOk);
                    return;
                }

                TryDestroyPreviousPlanes(environmentRoot, k_SavePlanesDialogTitle, undoBlock);
                if (!DestroyedPlanes)
                    return;

                var newPlanesRoot = CreateGeneratedPlanesRoot(environmentRoot.transform, undoBlock);
                var simPlanePrefab = MARSEditorPrefabs.instance.GeneratedSimulatedPlanePrefab;
                foreach (var plane in k_Planes)
                {
                    var synthPlane = Object.Instantiate(simPlanePrefab, newPlanesRoot);
                    synthPlane.transform.SetWorldPose(plane.pose);
                    synthPlane.SetMRPlaneData(plane.vertices, plane.center, plane.extents);
                }

                newPlanesRoot.gameObject.SetLayerRecursively(SimulationConstants.SimulatedEnvironmentLayerIndex);
            }
        }
    }
}
