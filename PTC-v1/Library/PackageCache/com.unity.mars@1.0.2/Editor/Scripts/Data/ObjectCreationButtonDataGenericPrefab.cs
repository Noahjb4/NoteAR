using UnityEngine;
using UnityEditor;

namespace Unity.MARS
{
    public class ObjectCreationButtonDataGenericPrefab : ObjectCreationButtonData
    {
#pragma warning disable 649
        [SerializeField]
        GameObject m_Prefab;
#pragma warning restore 649

        public override bool CreateGameObject()
        {
            if (m_Prefab == null)
                return false;

            MARSSession.EnsureRuntimeState();

            var objName = GameObjectUtility.GetUniqueNameForSibling(null, m_ButtonName);
            var go = Instantiate(m_Prefab);
            MARSWorldScaleModule.ScaleChildren(go.transform);

            foreach (var colorComponent in go.GetComponentsInChildren<IHasEditorColor>())
            {
                colorComponent.SetNewColor(true, true);
            }

            go.name = objName;
            Undo.RegisterCreatedObjectUndo(go, $"Create {objName}");
            Selection.activeGameObject = go;
            return true;
        }
    }
}
