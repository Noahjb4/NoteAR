using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Unity.MARS
{
    /// <summary>
    /// Utility methods used by the Composite Renderer in editor
    /// </summary>
    static class CompositeRenderEditorUtils
    {
        static FieldInfo s_SceneTargetTextureFieldInfo;

        static FieldInfo SceneTargetTextureFieldInfo
        {
            get
            {
                if (s_SceneTargetTextureFieldInfo == null)
                {
                    s_SceneTargetTextureFieldInfo = typeof(SceneView).GetField("m_SceneTargetTexture",
                        BindingFlags.Instance | BindingFlags.NonPublic);
                }

                return s_SceneTargetTextureFieldInfo;
            }
        }

        /// <summary>
        /// Gets the render target texture from a scene view
        /// </summary>
        /// <param name="sceneView">The scene view to get the target render texture from.</param>
        /// <returns></returns>
        internal static RenderTexture GetSceneTargetTexture(this SceneView sceneView)
        {
            return SceneTargetTextureFieldInfo.GetValue(sceneView) as RenderTexture;
        }
    }
}
