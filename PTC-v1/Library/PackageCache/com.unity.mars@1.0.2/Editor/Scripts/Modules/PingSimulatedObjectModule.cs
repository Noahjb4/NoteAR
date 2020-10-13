using Unity.XRTools.ModuleLoader;
using UnityEditor;

namespace Unity.MARS
{
    public class PingSimulatedObjectModule : IModule
    {
        public void LoadModule()
        {
            Selection.selectionChanged += OnSelectionChanged;
        }

        public void UnloadModule()
        {
            Selection.selectionChanged -= OnSelectionChanged;
            HierarchyTreeView.UnsetPing();
        }

        static void OnSelectionChanged()
        {
            HierarchyTreeView.PingObjects(Selection.transforms);
        }
    }
}
