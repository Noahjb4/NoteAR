using Unity.XRTools.ModuleLoader;
using Unity.XRTools.Utils;
using UnityEngine;
using UnityEngine.Rendering;

namespace Unity.MARS
{
    [ScriptableSettingsPath(MARSCore.UserSettingsFolder)]
    public class CompositeRenderModuleOptions : EditorScriptableSettings<CompositeRenderModuleOptions>
    {
        [SerializeField]
        [Tooltip("Use a simplified version of composite rendering with greater compatibility to different rendering setups.")]
        bool m_UseFallbackCompositeRendering;

        [SerializeField]
        [Tooltip("Use a camera stack for composite rendering of game view and other views that support camera stacking in Fallback composite rendering.")]
        bool m_UseCameraStackInFallback;

        /// <summary>
        /// Use a simplified version of composite rendering with greater compatibility to different rendering setups.
        /// </summary>
        public bool UseFallbackCompositeRendering
        {
            get
            {
                if (RenderPipelineManager.currentPipeline != null && !m_UseFallbackCompositeRendering)
                    UseFallbackCompositeRendering = true;

                return m_UseFallbackCompositeRendering;
            }
            set
            {
                if (RenderPipelineManager.currentPipeline != null && m_UseFallbackCompositeRendering)
                    return;

                if (m_UseFallbackCompositeRendering != value)
                {
                    m_UseFallbackCompositeRendering = value;
                    // Do not reload modules if changed when application is playing
                    if (ModuleLoaderCore.instance != null && !Application.isPlaying)
                        ModuleLoaderCore.instance.ReloadModules();
                }
            }
        }

        /// <summary>
        /// Use a camera stack for composite rendering of game view and other views that support camera stacking in Fallback composite rendering.
        /// </summary>
        public bool UseCameraStackInFallback
        {
            get { return m_UseFallbackCompositeRendering && m_UseCameraStackInFallback; }
            set
            {
                if (m_UseCameraStackInFallback != value)
                {
                    m_UseCameraStackInFallback = value;
                    // Do not reload modules if changed when application is playing
                    if (ModuleLoaderCore.instance != null  && !Application.isPlaying)
                        ModuleLoaderCore.instance.ReloadModules();
                }
            }
        }
    }
}
