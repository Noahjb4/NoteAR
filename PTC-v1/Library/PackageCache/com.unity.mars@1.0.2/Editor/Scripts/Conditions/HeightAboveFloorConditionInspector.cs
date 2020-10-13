using System;
using UnityEngine;

namespace Unity.MARS
{
    [ComponentEditor(typeof(HeightAboveFloorCondition))]
    public class HeightAboveFloorConditionInspector : ComponentInspector
    {
        public override void OnInspectorGUI()
        {
            if (isDirty)
                CleanUp();

            serializedObject.Update();

            DrawDefaultInspector();

            MarsCompareTool.DrawComponentControls(target as HeightAboveFloorCondition);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
