using System;
using Unity.XRTools.ModuleLoader;
using Unity.XRTools.Utils;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
#if INCLUDE_POST_PROCESSING
using UnityEngine.Rendering.PostProcessing;
#endif
using UnityEngine.SceneManagement;

namespace Unity.MARS
{
    /// <summary>
    /// The settings used to render the view for the composite context from the CompositeRenderModule.
    /// </summary>
    public class CompositeRenderContext : IDisposable
    {
        const string k_CompositeShader = "MARS/CompositeBlit";
        const string k_ShaderKeywordDesaturateOverlay = "DESATURATE_OVERLAY";
        const string k_ShaderKeywordDesaturateBase = "DESATURATE_BASE";
        const string k_ShaderKeywordCompositeUnder = "COMPOSITEUNDER";

        static readonly int k_OverlayTexID = Shader.PropertyToID("_CompositeBlitOverlayTex");

        ContextViewType m_ContextViewType;

        Camera m_TargetCamera;
        Camera m_CompositeCamera;
        CompositeCameraRenderer m_CompositeCameraRenderer;

        bool m_ShowImageEffects;
        bool m_BackgroundSceneActive;
        bool m_DesaturateComposited;
        bool m_UseXRay;
        float m_ShadowDistance;

        Material m_CompositeMaterial;
        LayerMask m_CompositeLayerMask;

#if INCLUDE_POST_PROCESSING
        PostProcessLayer m_CompositePostProcess;
#endif

        Scene m_CameraScene;
        CommandBuffer m_CompositeLayerBuffer;
        CameraEvent m_CameraEvent;
        RenderTextureDescriptor m_TargetDescriptor;
        CameraClearFlags m_BackgroundClearFlags;
        Color m_BackgroundColor;
        Scene m_PrefabStageScene;
        RenderTexture m_CompositeCameraTexture;
        int m_TargetCameraCullingMask;

        /// <summary>
        /// The camera that is the target of the compositing in the context
        /// </summary>
        public Camera TargetCamera => m_TargetCamera;

        /// <summary>
        /// Type of view the context is being used with
        /// </summary>
        public ContextViewType ContextViewType => m_ContextViewType;

        /// <summary>
        /// Show image effects in the View
        /// </summary>
        public bool ShowImageEffects
        {
            get => m_ShowImageEffects;
            set => m_ShowImageEffects = value;
        }

        /// <summary>
        /// Use the X-Ray wall cut away in the view.
        /// </summary>
        public bool UseXRay
        {
            get => m_UseXRay;
            set => m_UseXRay = value;
        }

        /// <summary>
        /// Whether to desaturate the composite camera's texture when compositing.
        /// </summary>
        public bool DesaturateComposited
        {
            get => m_DesaturateComposited;
            set => m_DesaturateComposited = value;
        }

        public CameraClearFlags BackgroundClearFlag
        {
            get => m_BackgroundClearFlags;
            set => m_BackgroundClearFlags = value;
        }

        /// <summary>
        /// The LayerMask for the background layer of the composite.
        /// This is also the layer any image effects will use in the composite.
        /// </summary>
        public LayerMask CompositeLayerMask
        {
            get => m_CompositeLayerMask;
            set => m_CompositeLayerMask = value;
        }

        /// <summary>
        /// The composite camera being used in the context
        /// </summary>
        public Camera CompositeCamera => m_CompositeCamera;

        public RenderTexture CompositeCameraTexture => m_CompositeCameraTexture;

        /// <summary>
        /// Is the composite context active
        /// </summary>
        static bool compositeSimulationReady => CompositeRenderModule.GetActiveCompositeRenderModule(out var module)
            && module.SimulationSceneReady;

        public bool IsBaseSceneView => m_ContextViewType == ContextViewType.NormalSceneView
            || m_ContextViewType == ContextViewType.PrefabIsolation;

        bool IsSimSceneView => m_ContextViewType == ContextViewType.SimulationView
            || m_ContextViewType == ContextViewType.DeviceView;

        bool IsRenderStackView => m_ContextViewType == ContextViewType.GameView
            || m_ContextViewType == ContextViewType.OtherView;

        static bool UseFallbackRendering => CompositeRenderModuleOptions.instance.UseFallbackCompositeRendering;

        static bool UseCameraStackInFallback => CompositeRenderModuleOptions.instance.UseCameraStackInFallback;

        public bool UseCompositeCamera => !UseFallbackRendering || UseCameraStackInFallback && IsRenderStackView;

        /// <summary>
        /// Called right before the composite camera updates from the target camera
        /// </summary>
        public event Action BeforeCompositeCameraUpdate;

        /// <summary>
        /// Creates a new composite render context for a given view.
        /// </summary>
        /// <param name="contextViewType">What type of view is the target.</param>
        /// <param name="targetCamera">The camera that renders the view</param>
        /// <param name="cameraTargetDescriptor">the render texture descriptor for the target cameras render texture</param>
        /// <param name="backgroundColor">The background color for compositing the view.</param>
        /// <param name="compositeLayerMask">Layer mask for the composite objects.</param>
        /// <param name="showImageEffects">Show image effects in the view.</param>
        /// <param name="backgroundSceneActive">Is the background of the composite the target camera scene</param>
        /// <param name="desaturateComposited">Desaturate the composited cameras output.</param>
        /// <param name="useXRay">Is the x-ray shader in use in the view.</param>
        public CompositeRenderContext(ContextViewType contextViewType, Camera targetCamera,
            RenderTextureDescriptor cameraTargetDescriptor, Color backgroundColor, LayerMask compositeLayerMask,
            bool showImageEffects = false, bool backgroundSceneActive = false, bool desaturateComposited = false,
            bool useXRay = true)
        {
            m_ContextViewType = contextViewType;
            m_TargetCamera = targetCamera;

            // Camera rendering cannot decide if camera needs an intermediate texture from the command buffer
            // To prevent the camera from rendering to a back buffer and flipping the texture in the composite
            // We force the camera to use an intermediate render texture.
            m_TargetCamera.forceIntoRenderTexture = true;

            m_UseXRay = useXRay;
            m_ShowImageEffects = showImageEffects;
            m_CompositeLayerMask = compositeLayerMask;

            m_BackgroundClearFlags = m_TargetCamera.clearFlags;
            m_TargetCameraCullingMask = m_TargetCamera.cullingMask;

            m_CompositeMaterial = new Material(Shader.Find(k_CompositeShader))
            {
                // Avoid user modification and ensure cleanup of material if there is an exception
                hideFlags = HideFlags.HideAndDontSave
            };

            m_BackgroundSceneActive = backgroundSceneActive;
            m_DesaturateComposited = desaturateComposited;
            m_BackgroundColor = backgroundColor;
            m_TargetDescriptor = cameraTargetDescriptor;

            SetupCompositeCamera();

            SetRenderTextureDescriptor(m_TargetDescriptor);

            SimulationSceneModule.SimulationSceneClosing += CleanCompositeView;
            SimulationSceneModule.SimulationSceneOpened += CleanCompositeView;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            SimulationSceneModule.SimulationSceneClosing -= CleanCompositeView;
            SimulationSceneModule.SimulationSceneOpened -= CleanCompositeView;

            var moduleLoaderCore = ModuleLoaderCore.instance;
            var simSceneModule = moduleLoaderCore.GetModule<SimulationSceneModule>();
            var simReady = simSceneModule != null && simSceneModule.IsSimulationReady;

            if (m_CompositeCameraRenderer != null)
            {
                UnityObjectUtils.Destroy(m_CompositeCameraRenderer);
                m_CompositeCameraRenderer = null;
            }

            if (m_TargetCamera != null)
            {
                TearDownCommandBuffer(m_CameraEvent, m_TargetCamera);
                if (simReady && (IsSimSceneView || m_ContextViewType == ContextViewType.OtherView))
                    simSceneModule.RemoveCameraFromSimulation(m_TargetCamera);

                if (IsSimSceneView)
                    SceneManager.MoveGameObjectToScene(m_TargetCamera.gameObject, SceneManager.GetActiveScene());

                m_TargetCamera.cullingMask = m_TargetCameraCullingMask;
                m_TargetCamera = null;
            }

            if (m_CompositeMaterial != null)
            {
                UnityObjectUtils.Destroy(m_CompositeMaterial);
                m_CompositeMaterial = null;
            }

            if (m_CompositeCamera != null)
            {
                UnityObjectUtils.Destroy(m_CompositeCamera.gameObject);
                m_CompositeCamera = null;
            }

            if (m_CompositeCameraTexture != null)
            {
                m_CompositeCameraTexture.Release();
                UnityObjectUtils.Destroy(m_CompositeCameraTexture);
                m_CompositeCameraTexture = null;
            }
        }

        /// <summary>
        /// Refresh the render setup of the composite context
        /// </summary>
        void RefreshCompositeRenderSetup()
        {
            if (m_TargetCamera == null)
                return;

            var simSceneModule = ModuleLoaderCore.instance.GetModule<SimulationSceneModule>();
            if (simSceneModule == null || !simSceneModule.IsSimulationReady)
                return;

            if (UseCompositeCamera && m_CompositeCamera == null)
                SetupCompositeCamera();
            else
                UpdateCompositeCamera();

            switch (m_ContextViewType)
            {
                case ContextViewType.NormalSceneView:
                case ContextViewType.PrefabIsolation:
                {
                    break;
                }
                case ContextViewType.GameView:
                case ContextViewType.OtherView:
                {
                    m_TargetCamera.scene = simSceneModule.ContentScene;
                    // Setup composite camera to render the env scene under the camera rendering content scene
                    if (!UseFallbackRendering)
                        simSceneModule.AddEnvironmentGameObject(m_CompositeCamera.gameObject);

                    if (UseCompositeCamera)
                        m_CompositeCamera.scene = simSceneModule.EnvironmentScene;

                    break;
                }
                case ContextViewType.DeviceView:
                case ContextViewType.SimulationView:
                {
                    if (UseFallbackRendering)
                    {
                        m_TargetCamera.scene = simSceneModule.ContentScene;
                        return;
                    }

                    if (m_BackgroundSceneActive)
                    {
                        m_TargetCamera.scene = simSceneModule.EnvironmentScene;

                        // Setup composite camera to render the content scene on top of camera rendering env scene
                        simSceneModule.AddContentGameObject(m_CompositeCamera.gameObject, true);
                        m_CompositeCamera.scene = simSceneModule.ContentScene;
                    }
                    else
                    {
                        m_TargetCamera.scene = simSceneModule.ContentScene;

                        // Setup composite camera to render the env scene under the camera rendering content scene
                        simSceneModule.AddEnvironmentGameObject(m_CompositeCamera.gameObject, true);
                        m_CompositeCamera.scene = simSceneModule.EnvironmentScene;
                    }
                    break;
                }
            }
        }

        void OnPlayModeStateChanged(PlayModeStateChange playModeState)
        {
            switch (m_ContextViewType)
            {
                case ContextViewType.NormalSceneView:
                case ContextViewType.PrefabIsolation:
                {
                    if (m_CompositeCamera == null)
                        SetupCompositeCamera();

                    break;
                }
                case ContextViewType.DeviceView:
                case ContextViewType.SimulationView:
                {
                    if (m_CompositeCamera == null && compositeSimulationReady)
                        SetupCompositeCamera();

                    break;
                }
                case ContextViewType.GameView:
                case ContextViewType.OtherView:
                {
                    break;
                }
            }
        }

        void SetupCompositeCamera()
        {
            if (m_TargetCamera == null)
                return;

            if (m_CompositeCamera != null)
            {
                UnityObjectUtils.Destroy(m_CompositeCamera.gameObject);
                m_CompositeCamera = null;
            }

            if (UseCompositeCamera)
            {
                var compositeCameraGo = new GameObject($"{m_TargetCamera.name} Composite", typeof(Camera))
                {
                    hideFlags = m_ContextViewType == ContextViewType.GameView ? HideFlags.None : HideFlags.HideAndDontSave
                };

                m_CompositeCamera = compositeCameraGo.GetComponent<Camera>();
                m_CompositeCamera.name = compositeCameraGo.name;
                m_CompositeCamera.backgroundColor = m_BackgroundColor;
                m_CompositeCamera.clearFlags = m_BackgroundClearFlags;
                SetRenderTextureDescriptor(m_TargetDescriptor);
                m_TargetCameraCullingMask = m_TargetCamera.cullingMask;
            }

            // Scene view composite rendering is called with SceneView.beforeSceneGui & SceneView.duringSceneGui
            // Should only be used where you need the target camera to handle the render callbacks
            if (IsRenderStackView && !UseFallbackRendering && m_CompositeCameraRenderer == null)
            {
                m_CompositeCameraRenderer = m_TargetCamera.GetComponent<CompositeCameraRenderer>();

                if (m_CompositeCameraRenderer == null)
                {
                    m_CompositeCameraRenderer = m_TargetCamera.gameObject.AddComponent<CompositeCameraRenderer>();
                    m_CompositeCameraRenderer.PreCullCamera += PreCompositeCullTargetCamera;
                    m_CompositeCameraRenderer.PostRenderCamera += PostCompositeRenderTargetCamera;
                }
            }
            else if (UseCompositeCamera)
            {
                m_CompositeCamera.transform.parent = m_TargetCamera.transform;
                m_CompositeMaterial.EnableKeyword(k_ShaderKeywordCompositeUnder);
            }

            UpdateCompositeCamera();

            if (UseCompositeCamera)
                m_CompositeCamera.transform.localScale = Vector3.one;
        }

        /// <summary>
        /// Assigns the cameras in a composite context to the simulation.
        /// </summary>
        public void AssignCameraToSimulation()
        {
            if (!compositeSimulationReady)
                return;

            var simSceneModule = ModuleLoaderCore.instance.GetModule<SimulationSceneModule>();
            if (simSceneModule == null)
            {
                Debug.LogError("SimulationSceneModule not found. Camera compositing failed to set up.");
                return;
            }

            if (!simSceneModule.IsCameraAssignedToSimulationScene(m_TargetCamera)
                && !(m_ContextViewType == ContextViewType.GameView || IsBaseSceneView))
                simSceneModule.AssignCameraToSimulation(m_TargetCamera);

            if (UseCompositeCamera && !simSceneModule.IsCameraAssignedToSimulationScene(m_CompositeCamera))
                simSceneModule.AssignCameraToSimulation(m_CompositeCamera);

            // When using fallback rendering and using the composite camera,
            // the camera is parented to the target camera
            if (!UseFallbackRendering)
            {
                var compositeCameraGo = m_CompositeCamera.gameObject;

                if (m_BackgroundSceneActive && compositeCameraGo.scene != simSceneModule.ContentScene)
                {
                    simSceneModule.AddContentGameObject(compositeCameraGo, true);
                    m_CompositeCamera.scene = simSceneModule.ContentScene;
                }
                else if (compositeCameraGo.scene != simSceneModule.EnvironmentScene)
                {
                    simSceneModule.AddEnvironmentGameObject(compositeCameraGo, true);
                    m_CompositeCamera.scene = simSceneModule.EnvironmentScene;
                }
            }

            RefreshCompositeRenderSetup();
        }

        /// <summary>
        /// The render texture descriptor to use for the camera renders.
        /// </summary>
        /// <param name="renderTextureDescriptor">The render texture descriptor to use for the render texture.</param>
        internal void SetRenderTextureDescriptor(RenderTextureDescriptor renderTextureDescriptor)
        {
            m_TargetDescriptor = renderTextureDescriptor;

            // Adapted from SceneView.CreateCameraTargetTexture
            // make sure we actually support R16G16B16A16_SFloat
            GraphicsFormat format;

            if (UseCompositeCamera)
            {
                format = (m_CompositeCamera.allowHDR && SystemInfo.IsFormatSupported(GraphicsFormat.R16G16B16A16_SFloat, FormatUsage.Render))
                    ? GraphicsFormat.R16G16B16A16_SFloat : SystemInfo.GetGraphicsFormat(DefaultFormat.LDR);
            }
            else if (m_TargetCamera)
            {
                format = (m_TargetCamera.allowHDR && SystemInfo.IsFormatSupported(GraphicsFormat.R16G16B16A16_SFloat, FormatUsage.Render))
                    ? GraphicsFormat.R16G16B16A16_SFloat : SystemInfo.GetGraphicsFormat(DefaultFormat.LDR);
            }
            else
            {
                //Debug.Log("No target camera!");
                return;
            }

            // Need to make sure we set the graphics format to our valid format
            // or we will get an out of range value for the render texture format
            // when we try creating the render texture
            m_TargetDescriptor.graphicsFormat = format;
            // Need to enable depth buffer if the target camera did not already have it.
            if (m_TargetDescriptor.depthBufferBits < 24)
                m_TargetDescriptor.depthBufferBits = 24;

            // Using a target texture can interfere with game view camera stack
            if (UseCompositeCamera && UseCameraStackInFallback && m_ContextViewType == ContextViewType.GameView)
            {
                if (m_CompositeCamera.targetTexture != null)
                {
                    m_CompositeCamera.targetTexture.Release();
                    m_CompositeCamera.targetTexture = null;
                }

                if (m_CompositeCameraTexture != null)
                {
                    m_CompositeCamera.targetTexture = null;
                    m_CompositeCameraTexture.Release();
                    UnityObjectUtils.Destroy(m_CompositeCameraTexture);
                    m_CompositeCameraTexture = null;
                }

                return;
            }

            if (UseCompositeCamera && m_CompositeCameraTexture != null && m_CompositeCameraTexture.graphicsFormat != format)
            {
                m_CompositeCamera.targetTexture = null;
                m_CompositeCameraTexture.Release();
                UnityObjectUtils.Destroy(m_CompositeCameraTexture);
                m_CompositeCameraTexture = null;
            }

            if (m_CompositeCameraTexture == null)
            {
                var nameString = UseCompositeCamera ? m_CompositeCamera.name : m_TargetCamera.name;
                m_CompositeCameraTexture = new RenderTexture(m_TargetDescriptor)
                {
                    name = $"{nameString} RenderTexture",
                    antiAliasing = 1,
                    hideFlags = HideFlags.HideAndDontSave
                };
            }
            else if (m_CompositeCameraTexture.width != m_TargetDescriptor.width
                || m_CompositeCameraTexture.height != m_TargetDescriptor.height)
            {
                m_CompositeCameraTexture.Release();
                m_CompositeCameraTexture.width = m_TargetDescriptor.width;
                m_CompositeCameraTexture.height = m_TargetDescriptor.height;
            }

            m_CompositeCameraTexture.Create();

            if (UseCompositeCamera)
                m_CompositeCamera.targetTexture = m_CompositeCameraTexture;
        }

        bool EnsureCompositeRenderingSettings(out SimulationRenderSettings simulationRenderSettings)
        {
            simulationRenderSettings = null;
            m_TargetCameraCullingMask = m_TargetCamera.cullingMask;
            m_ShadowDistance = QualitySettings.shadowDistance;

            var simSceneModule = ModuleLoaderCore.instance.GetModule<SimulationSceneModule>();
            if (IsBaseSceneView)
            {
                var targetCameraScene = m_TargetCamera.scene;
                var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
                if (prefabStage == null)
                {
                    m_ContextViewType = ContextViewType.NormalSceneView;
                }
                else
                {
                    m_PrefabStageScene = prefabStage.scene;
                    if (!m_PrefabStageScene.IsValid() || targetCameraScene != m_PrefabStageScene)
                    {
                        m_ContextViewType = ContextViewType.NormalSceneView;
                    }

                    if (CompositeRenderRuntimeUtils.SceneSimulationRenderSettings.TryGetValue(m_PrefabStageScene,
                        out var prefabRenderSettings))
                    {
                        m_ContextViewType = ContextViewType.PrefabIsolation;
                        simulationRenderSettings = prefabRenderSettings;
                    }
                }
            }
            else if (compositeSimulationReady)
            {
                if (UseCompositeCamera)
                {
                    m_TargetCamera.scene = m_BackgroundSceneActive ? simSceneModule.EnvironmentScene : simSceneModule.ContentScene;

                    if (!(UseFallbackRendering && IsRenderStackView))
                    {
                        var compositeCameraGo = m_CompositeCamera.gameObject;

                        if (compositeCameraGo.transform.parent != null)
                            compositeCameraGo.transform.parent = null;

                        if (m_BackgroundSceneActive && compositeCameraGo.scene != simSceneModule.ContentScene)
                            simSceneModule.AddContentGameObject(compositeCameraGo, true);
                        else if (compositeCameraGo.scene != simSceneModule.EnvironmentScene)
                            simSceneModule.AddEnvironmentGameObject(compositeCameraGo, true);
                    }

                    m_CompositeCamera.scene = m_BackgroundSceneActive ? simSceneModule.ContentScene : simSceneModule.EnvironmentScene;
                }
                else
                {
                    m_TargetCamera.scene = simSceneModule.ContentScene;
                }
            }

            if (IsBaseSceneView && !compositeSimulationReady)
            {
                if (simulationRenderSettings == null)
                    simulationRenderSettings = new SimulationRenderSettings();

                return false;
            }

            if (m_ContextViewType == ContextViewType.GameView || m_ContextViewType == ContextViewType.OtherView)
            {
                if (m_TargetDescriptor.width != m_TargetCamera.scaledPixelWidth
                    || m_TargetDescriptor.height != m_TargetCamera.scaledPixelHeight)
                {
                    m_TargetDescriptor.width = m_TargetCamera.scaledPixelWidth;
                    m_TargetDescriptor.height = m_TargetCamera.scaledPixelHeight;
                    SetRenderTextureDescriptor(m_TargetDescriptor);
                }

                m_CameraEvent = CameraEvent.AfterForwardAlpha;
            }

            UpdateCompositeCamera();

            // Need to control the rendering of the background camera here.
            // This is to make sure the render setting are applied correctly
            // and the skybox renders to the correct view.
            if (!IsBaseSceneView)
                CompositeRenderRuntimeUtils.SceneSimulationRenderSettings.TryGetValue(simSceneModule.EnvironmentScene, out simulationRenderSettings);

            if (simulationRenderSettings == null)
                simulationRenderSettings = new SimulationRenderSettings();

            return true;
        }

        internal void PreCompositeCullTargetCamera()
        {
            if (!CompositeRenderModule.GetActiveCompositeRenderModule(out var compositeRenderModule) ||
                compositeRenderModule.HoldForGameView ||
                !EnsureCompositeRenderingSettings(out var simulationRenderSettings))
                return;

            compositeRenderModule.OnBeforeBackgroundCameraRender(this);

            if (!UseCompositeCamera)
                return;

            switch (m_ContextViewType)
            {
                case ContextViewType.NormalSceneView:
                {
                    break;
                }
                case ContextViewType.PrefabIsolation:
                {
                    RenderPrefabIsolation(simulationRenderSettings);
                    break;
                }
                case ContextViewType.GameView:
                case ContextViewType.DeviceView:
                case ContextViewType.SimulationView:
                case ContextViewType.OtherView:
                {
                    if (!UseFallbackRendering)
                    {
                        if (m_BackgroundSceneActive)
                            RenderCompositeAsForeground(simulationRenderSettings);
                        else
                            RenderCompositeAsBackground(simulationRenderSettings);
                    }

                    if (m_BackgroundSceneActive)
                    {
                        m_CameraEvent = CameraEvent.AfterForwardAlpha;
                        m_CompositeMaterial.DisableKeyword(k_ShaderKeywordCompositeUnder);

                        if (m_DesaturateComposited)
                        {
                            m_CompositeMaterial.EnableKeyword(k_ShaderKeywordDesaturateOverlay);
                            m_CompositeMaterial.DisableKeyword(k_ShaderKeywordDesaturateBase);
                        }
                        else
                        {
                            m_CompositeMaterial.DisableKeyword(k_ShaderKeywordDesaturateOverlay);
                            m_CompositeMaterial.DisableKeyword(k_ShaderKeywordDesaturateBase);
                        }
                    }
                    else
                    {
                        m_CameraEvent = CameraEvent.AfterImageEffectsOpaque;
                        m_CompositeMaterial.EnableKeyword(k_ShaderKeywordCompositeUnder);

                        if (m_DesaturateComposited)
                        {
                            m_CompositeMaterial.EnableKeyword(k_ShaderKeywordDesaturateBase);
                            m_CompositeMaterial.DisableKeyword(k_ShaderKeywordDesaturateOverlay);
                        }
                        else
                        {
                            m_CompositeMaterial.DisableKeyword(k_ShaderKeywordDesaturateOverlay);
                            m_CompositeMaterial.DisableKeyword(k_ShaderKeywordDesaturateBase);
                        }
                    }

                    if (!UseFallbackRendering)
                        SetupCommandBuffers(m_CameraEvent, m_TargetCamera);

                    break;
                }
            }
        }

        internal void PostCompositeRenderTargetCamera()
        {
            SetupImageEffects();

            if (!UseFallbackRendering)
            {
                Unsupported.RestoreOverrideLightingSettings();
                QualitySettings.shadowDistance = m_ShadowDistance;
            }

            if (!CompositeRenderModule.GetActiveCompositeRenderModule(out var compositeRenderModule))
                return;

            if (compositeRenderModule.HoldForGameView)
            {
                if (m_ContextViewType == ContextViewType.GameView)
                    compositeRenderModule.HoldForGameView = false;

                return;
            }

            if (m_ContextViewType == ContextViewType.PrefabIsolation)
                return;

            if (!UseFallbackRendering)
                TearDownCommandBuffer(m_CameraEvent, m_TargetCamera);

            if (!compositeSimulationReady)
                return;

            CleanCompositeView();
        }

        void SetupCommandBuffers(CameraEvent evt, Camera sceneCamera)
        {
            if (!UseCompositeCamera || !compositeSimulationReady)
                return;

            var cameraTarget = new RenderTargetIdentifier(BuiltinRenderTextureType.CameraTarget);

            // Blit the composite camera output into the target camera's render buffer.
            var buffer = new CommandBuffer { name = "Composite Renderer Buffer" };
            buffer.BeginSample(buffer.name);
            buffer.GetTemporaryRT(k_OverlayTexID, m_TargetDescriptor);
            var textureID = new RenderTargetIdentifier(k_OverlayTexID);
            buffer.Blit(cameraTarget, textureID);
            buffer.Blit(m_CompositeCameraTexture, cameraTarget, m_CompositeMaterial);
            buffer.ReleaseTemporaryRT(k_OverlayTexID);
            buffer.EndSample(buffer.name);
            m_CompositeLayerBuffer = buffer;
            sceneCamera.AddCommandBuffer(evt, m_CompositeLayerBuffer);
        }

        void TearDownCommandBuffer(CameraEvent evt, Camera sceneCamera)
        {
            if (!UseCompositeCamera || m_CompositeLayerBuffer == null)
                return;

            sceneCamera.RemoveCommandBuffer(evt, m_CompositeLayerBuffer);
            m_CompositeLayerBuffer.Release();
            m_CompositeLayerBuffer = null;
        }

        internal void UpdateCompositeCamera()
        {
            BeforeCompositeCameraUpdate?.Invoke();

            if (UseCompositeCamera)
            {
                m_CompositeCamera.transform.SetWorldPose(m_TargetCamera.transform.GetWorldPose());

                m_CompositeCamera.fieldOfView = m_TargetCamera.fieldOfView;
                m_CompositeCamera.aspect = m_TargetCamera.aspect;
                m_CompositeCamera.nearClipPlane = m_TargetCamera.nearClipPlane;
                m_CompositeCamera.farClipPlane = m_TargetCamera.farClipPlane;
                m_CompositeCamera.orthographic = m_TargetCamera.orthographic;
                m_CompositeCamera.orthographicSize = m_TargetCamera.orthographicSize;

                if (m_ContextViewType == ContextViewType.GameView)
                    m_CompositeCamera.depth = m_TargetCamera.depth - 1;
            }

            switch (m_ContextViewType)
            {
                case ContextViewType.NormalSceneView:
                case ContextViewType.PrefabIsolation:
                    m_TargetCamera.cullingMask = Tools.visibleLayers;
                    break;
                case ContextViewType.SimulationView:
                case ContextViewType.DeviceView:
                {
                    if (!UseFallbackRendering)
                    {
                        if (m_BackgroundSceneActive)
                        {
                            m_TargetCamera.cullingMask = m_CompositeLayerMask.value;
                            m_CompositeCamera.cullingMask = m_TargetCameraCullingMask & ~m_CompositeLayerMask.value;
                            m_CompositeCamera.clearFlags = CameraClearFlags.SolidColor;
                            m_CompositeCamera.backgroundColor = Color.clear;
                        }
                        else
                        {
                            m_TargetCamera.cullingMask &= ~m_CompositeLayerMask.value;
                            m_CompositeCamera.cullingMask = m_CompositeLayerMask.value;
                            m_CompositeCamera.clearFlags = m_BackgroundClearFlags;
                            m_CompositeCamera.backgroundColor = m_BackgroundColor;
                        }

                        m_TargetCamera.clearFlags = CameraClearFlags.SolidColor;
                        m_TargetCamera.backgroundColor = Color.clear;
                    }
                    else
                    {
                        m_TargetCamera.cullingMask = Tools.visibleLayers;
                    }

                    break;
                }
                case ContextViewType.GameView:
                case ContextViewType.OtherView:
                {
                    if (UseCompositeCamera)
                    {
                        m_CompositeCamera.depth = m_TargetCamera.depth - 1;

                        m_TargetCamera.cullingMask &= ~m_CompositeLayerMask.value;
                        m_TargetCamera.clearFlags = UseCameraStackInFallback ?
                            CameraClearFlags.Depth : CameraClearFlags.SolidColor;
                        m_TargetCamera.backgroundColor = Color.clear;

                        m_CompositeCamera.cullingMask = m_CompositeLayerMask.value;
                        m_CompositeCamera.clearFlags = m_BackgroundClearFlags;
                        m_CompositeCamera.backgroundColor = m_BackgroundColor;
                    }
                    else
                    {
                        m_TargetCamera.cullingMask = m_TargetCameraCullingMask;

                        if (m_ContextViewType == ContextViewType.GameView)
                        {
                            m_TargetCamera.clearFlags = m_BackgroundClearFlags;
                            m_TargetCamera.backgroundColor = m_BackgroundColor;
                        }
                    }

                    if (UseCompositeCamera)
                    {
                        // Don't try to render the camera the frame it was created
                        if (UseFallbackRendering && !m_CompositeCamera.gameObject.activeSelf)
                            m_CompositeCamera.gameObject.SetActive(true);

                        if (!UseFallbackRendering)
                            m_CompositeCameraRenderer.enabled = !UseFallbackRendering;
                    }

                    break;
                }
                case ContextViewType.Undefined:
                    break;
            }
        }

        void RenderCompositeAsBackground(SimulationRenderSettings overrideRenderSettingsSettings = null)
        {
            if (!UseCompositeCamera)
                return;

            // Don't try to render the camera the frame it was created
            if (!m_CompositeCamera.gameObject.activeSelf)
            {
                m_CompositeCamera.gameObject.SetActive(true);
                return;
            }

            var targetSceneValid = m_CompositeCamera.scene.IsValid();
            if (targetSceneValid && !UseFallbackRendering)
            {
                Unsupported.SetOverrideLightingSettings(m_CompositeCamera.scene);

                if (overrideRenderSettingsSettings != null)
                    overrideRenderSettingsSettings.ApplyTempRenderSettings();
            }

            RenderCompositeCamera();

            if (targetSceneValid && !UseFallbackRendering)
            {
                Unsupported.RestoreOverrideLightingSettings();
                QualitySettings.shadowDistance = m_ShadowDistance;
            }

            GL.Clear(true, false, Color.black);
        }

        void RenderCompositeAsForeground(SimulationRenderSettings overrideRenderSettingsSettings = null)
        {
            if (!UseCompositeCamera)
                return;

            // Don't try to render the camera the frame it was created
            if (!m_CompositeCamera.gameObject.activeSelf)
            {
                m_CompositeCamera.gameObject.SetActive(true);
                return;
            }

            var simSceneModule = ModuleLoaderCore.instance.GetModule<SimulationSceneModule>();
            if (simSceneModule == null)
                return;

            var targetSceneValid = simSceneModule.EnvironmentScene.IsValid();

            if (targetSceneValid && !UseFallbackRendering)
            {
                Unsupported.RestoreOverrideLightingSettings();
                QualitySettings.shadowDistance = m_ShadowDistance;
            }

            RenderCompositeCamera();

            GL.Clear(true, false, Color.black);

            if (targetSceneValid && !UseFallbackRendering)
            {
                Unsupported.SetOverrideLightingSettings(simSceneModule.EnvironmentScene);

                if (overrideRenderSettingsSettings != null)
                    overrideRenderSettingsSettings.ApplyTempRenderSettings();
            }
        }

        void RenderPrefabIsolation(SimulationRenderSettings overrideRenderSettingsSettings = null)
        {
            if (!UseCompositeCamera)
                return;

            if (m_CompositeCamera.gameObject.activeSelf)
            {
                m_CompositeCamera.gameObject.SetActive(false);
                return;
            }

            if (!UseFallbackRendering)
            {
                Unsupported.SetOverrideLightingSettings(m_PrefabStageScene);
                QualitySettings.shadowDistance = m_ShadowDistance;

                if (overrideRenderSettingsSettings != null)
                    overrideRenderSettingsSettings.ApplyTempRenderSettings();
            }
        }

        /// <summary>
        /// Sets the background color for the composite.
        /// </summary>
        /// <param name="color">Color to use as a background color.</param>
        public void SetBackgroundColor(Color color)
        {
            m_BackgroundColor = color;
        }

        /// <summary>
        /// Sets whether the background scene of the composite should be active.
        /// Used for scene picking in a scene view.
        /// </summary>
        /// <param name="backgroundSceneActive">Should the background be the active scene</param>
        public void SetBackgroundSceneActive(bool backgroundSceneActive)
        {
            m_BackgroundSceneActive = backgroundSceneActive;
            RefreshCompositeRenderSetup();
        }

        /// <summary>
        /// Sets the target camera for the context.
        /// </summary>
        /// <param name="targetCamera">Camera to use as the Target Camera for the context.</param>
        public void SetTargetCamera(Camera targetCamera)
        {
            m_TargetCamera = targetCamera;
            m_TargetCameraCullingMask = m_TargetCamera.cullingMask;
            SetupCompositeCamera();
            RefreshCompositeRenderSetup();
        }

        void SetupImageEffects()
        {
            if (UseFallbackRendering || IsBaseSceneView)
                return;

#if INCLUDE_POST_PROCESSING
            if (!m_ShowImageEffects && m_CompositePostProcess != null)
            {
                m_CompositePostProcess.enabled = false;
                return;
            }
#else
            if (!m_ShowImageEffects)
                return;
#endif

            if (!compositeSimulationReady)
                return;

            var moduleLoader = ModuleLoaderCore.instance;
            var simSceneModule = moduleLoader.GetModule<SimulationSceneModule>();
            var targetCameraGameObject = m_TargetCamera.gameObject;
            m_CameraScene = targetCameraGameObject.scene;

#if INCLUDE_POST_PROCESSING
            if (m_ContextViewType != ContextViewType.GameView && !UseFallbackRendering)
            {
                if (simSceneModule != null)
                {
                    if (m_BackgroundSceneActive)
                    {
                        simSceneModule.AddEnvironmentGameObject(targetCameraGameObject, true);
                        m_TargetCamera.scene = simSceneModule.EnvironmentScene;
                    }
                    else
                    {
                        simSceneModule.AddContentGameObject(targetCameraGameObject, true);
                        m_TargetCamera.scene = simSceneModule.ContentScene;
                    }
                }
                else
                {
                    Debug.LogError("SimulationSceneModule not found.  Image effects failed to set up.");
                }
            }

            var compositeRenderModule = moduleLoader.GetModule<CompositeRenderModule>();
            if (compositeRenderModule != null && !compositeRenderModule.HoldForGameView)
            {
                if (m_CompositePostProcess == null)
                    m_CompositePostProcess = m_CompositeCamera.gameObject.AddComponent<PostProcessLayer>();

                m_CompositePostProcess.volumeLayer = m_CompositeLayerMask;
                m_CompositePostProcess.finalBlitToCameraTarget = false;
                m_CompositePostProcess.enabled = m_ShowImageEffects && !m_BackgroundSceneActive;
            }
#endif
        }

        void CleanCompositeView()
        {
            if (m_TargetCamera == null)
                return;

            switch (m_ContextViewType)
            {
                case ContextViewType.NormalSceneView:
                case ContextViewType.PrefabIsolation:
                {
                    m_TargetCamera.cullingMask = Tools.visibleLayers;
                    break;
                }
                case ContextViewType.SimulationView:
                case ContextViewType.DeviceView:
                {
                    m_TargetCamera.cullingMask = !UseFallbackRendering ? m_TargetCameraCullingMask : Tools.visibleLayers;
                    m_TargetCamera.clearFlags = m_BackgroundClearFlags;
                    m_TargetCamera.backgroundColor = m_BackgroundColor;

                    if (m_ShowImageEffects)
                    {
                        SceneManager.MoveGameObjectToScene(m_TargetCamera.gameObject,
                            m_CameraScene.IsValid() ? m_CameraScene : SceneManager.GetActiveScene());
                    }
                    break;
                }
                case ContextViewType.GameView:
                case ContextViewType.OtherView:
                {
                    m_TargetCamera.cullingMask = UseFallbackRendering ? m_TargetCamera.cullingMask : m_TargetCameraCullingMask;
                    break;
                }
                case ContextViewType.Undefined:
                    break;
            }
        }

        void RenderCompositeCamera()
        {
            if (!UseCompositeCamera)
                return;

            if (compositeSimulationReady && !m_BackgroundSceneActive)
                QualitySettings.shadowDistance *= MARSWorldScaleModule.GetWorldScale();

            m_CompositeCamera.Render();

            if (compositeSimulationReady && !m_BackgroundSceneActive)
                QualitySettings.shadowDistance = m_ShadowDistance;
        }
    }
}
