using UnityEditor.EditorTools;

namespace Unity.MARS
{
    /// <summary>
    /// Base class for blank semantic tag condition inspectors
    /// </summary>
    public abstract class FixedTagConditionInspector : ComponentInspector
    {
        public override bool HasDisplayProperties()
        {
            return EditorTools.activeToolType == typeof(MarsCompareTool);
        }

        public override void OnInspectorGUI()
        {
            if (isDirty)
                CleanUp();

            serializedObject.Update();

            MarsCompareTool.DrawComponentControls(target as SimpleTagCondition);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
