using Unity.XRTools.Utils;
using UnityEngine;

namespace Unity.MARS
{
    [AddComponentMenu("")]
    public class GeneratedPlanesRoot : MonoBehaviour, ISerializationCallbackReceiver
    {
        public const string PlanesRootName = "Generated Planes";
        public bool anyObjectsModified => false;
        public void OnBeforeSerialize() { }
        public void OnAfterDeserialize() { }

#if UNITY_EDITOR
        public void DestroyExceptModifiedObjects(UndoBlock undoBlock) { }
#endif
    }
}
