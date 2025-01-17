﻿using System;
using System.Collections;
using Unity.XRTools.ModuleLoader;
using Unity.XRTools.Utils;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Video;

namespace Unity.MARS.Simulation
{
    class SimulationVideoContextManager : MonoBehaviour, IModuleBehaviorCallbacks, IProvidesSimulationVideoFeed
    {
        const string k_BackgroundObjectName = "Video Background";
        const string k_BackgroundShader = "Unlit/Texture";
        const int k_InvalidWidth = 16;
        const int k_DefaultRequestedWidth = 640;
        const int k_DefaultRequestedHeight = 480;
        const int k_DefaultRequestedFPS = 60;
        static readonly Vector2 k_HorizontalFlipScale = new Vector2(-1f, 1f);
        static readonly Vector2 k_HorizontalFlipOffset = new Vector2(1f, 0f);

        Renderer m_BackgroundRenderer;
        Material m_BackgroundMaterial;
        RenderTexture m_RecordedVideoTexture;

#if UNITY_EDITOR || !PLATFORM_LUMIN
        WebCamTexture m_LiveVideoTexture;
#endif

        [NonSerialized]
        bool m_Running;

        [NonSerialized]
        bool m_ShouldStartWebCam;

        int m_WebCamDeviceIndex;

        public VideoPlayer VideoPlayer { get; private set; }

        public Texture VideoFeedTexture { get; private set; }

        public float VideoFocalLength { get; private set; }

        public CameraFacingDirection VideoFacingDirection { get; private set; }

        public bool IsUsingVideo => m_BackgroundRenderer != null;

        public GUIContent[] WebCamDeviceContents { get; private set; }

        public event Action<Texture> VideoFeedTextureChanged;

        public void LoadModule()
        {
            m_Running = false;
            VideoPlayer = gameObject.AddComponent<VideoPlayer>();
            VideoPlayer.renderMode = VideoRenderMode.RenderTexture;
            UpdateWebCamDeviceCandidates();
        }

        public void UnloadModule() { WebCamDeviceContents = null; }

        public void LoadProvider() { }

        public void ConnectSubscriber(object obj)
        {
            this.TryConnectSubscriber<IProvidesSimulationVideoFeed>(obj);
        }

        public void UnloadProvider() { }

        public void OnBehaviorAwake() { }

        public void OnBehaviorEnable()
        {
            m_Running = true;
            if (m_ShouldStartWebCam)
                StartCoroutine(StartWebCamCoroutine());

            if (m_BackgroundRenderer != null)
                SetupVideoFeedTexture();
        }

        public void OnBehaviorStart() { }

        public void OnBehaviorUpdate() { }

        public void OnBehaviorDisable()
        {
#if UNITY_EDITOR || !PLATFORM_LUMIN
            if (m_LiveVideoTexture != null)
            {
                m_LiveVideoTexture.Stop();
                m_LiveVideoTexture = null;
            }
#endif

            m_RecordedVideoTexture = null;
            VideoFeedTexture = null;
            m_Running = false;
        }

        public void OnBehaviorDestroy() { }

        public void SetupVideoBackground(Transform backgroundParent)
        {
            var backgroundObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
            backgroundObject.name = k_BackgroundObjectName;
            backgroundObject.transform.SetParent(backgroundParent, false);
            UnityObjectUtils.Destroy(backgroundObject.GetComponent<Collider>());
            m_BackgroundRenderer = backgroundObject.GetComponent<Renderer>();
            m_BackgroundRenderer.receiveShadows = false;
            m_BackgroundRenderer.shadowCastingMode = ShadowCastingMode.On;
            m_BackgroundMaterial = new Material(Shader.Find(k_BackgroundShader));
            m_BackgroundMaterial.mainTextureScale = k_HorizontalFlipScale;
            m_BackgroundMaterial.mainTextureOffset = k_HorizontalFlipOffset;
            m_BackgroundRenderer.sharedMaterial = m_BackgroundMaterial;

            if (m_Running)
                SetupVideoFeedTexture();
        }

        public void TearDownVideoBackground()
        {
#if UNITY_EDITOR || !PLATFORM_LUMIN
            m_LiveVideoTexture = null;
#endif

            m_ShouldStartWebCam = false;
            if (VideoPlayer != null)
                VideoPlayer.clip = null;

            if (m_BackgroundRenderer != null)
                UnityObjectUtils.Destroy(m_BackgroundRenderer.gameObject);
        }

        public void PrepareForLiveVideo(int webCamDeviceIndex)
        {
            m_WebCamDeviceIndex = webCamDeviceIndex;
            if (m_Running && isActiveAndEnabled)
                StartCoroutine(StartWebCamCoroutine());
            else
                m_ShouldStartWebCam = true;
        }

        public void UpdateWebCamDeviceCandidates()
        {
#if UNITY_EDITOR || !PLATFORM_LUMIN
            var devices = WebCamTexture.devices;
            WebCamDeviceContents = new GUIContent[devices.Length];
            for (var i = 0; i < devices.Length; i++)
            {
                WebCamDeviceContents[i] = new GUIContent(devices[i].name);
            }
#endif
        }

        IEnumerator StartWebCamCoroutine()
        {
#if UNITY_EDITOR || !PLATFORM_LUMIN
            var devices = WebCamTexture.devices;
            if (m_WebCamDeviceIndex >= devices.Length)
            {
                Debug.LogWarning("Selected web cam device is not available. Resync simulation to refresh available devices.");
                yield break;
            }

#if UNITY_EDITOR
            var isTemporal = EditorOnlyDelegates.IsSimulatingTemporal();
            if (!EditorOnlyDelegates.PerformCameraPermissionCheck(isTemporal))
                yield break;
#endif

            var device = devices[m_WebCamDeviceIndex];

            var requestedWidth = k_DefaultRequestedWidth;
            var requestedHeight = k_DefaultRequestedHeight;
            var requestedFPS = k_DefaultRequestedFPS;
            var availableResolutions = device.availableResolutions;
            if (availableResolutions != null && availableResolutions.Length > 0)
            {
                var resolution = availableResolutions[0];
                requestedWidth = resolution.width;
                requestedHeight = resolution.height;
                requestedFPS = resolution.refreshRate;
            }

            m_LiveVideoTexture = new WebCamTexture(device.name, requestedWidth, requestedHeight, requestedFPS);

            VideoFacingDirection = device.isFrontFacing ? CameraFacingDirection.User : CameraFacingDirection.World;
            VideoFocalLength = SimulationVideoContextSettings.instance.SimulatedFocalLength;
            m_LiveVideoTexture.Play();
            while (m_LiveVideoTexture != null && m_LiveVideoTexture.width <= k_InvalidWidth)
                yield return null;

            if (m_LiveVideoTexture == null)
                yield break;

            if (m_BackgroundRenderer != null && VideoFeedTexture != m_LiveVideoTexture)
                ChangeVideoFeedTexture(m_LiveVideoTexture);
#else
            Debug.LogWarning("Webcam API not available on Lumin platform");
            yield break;
#endif
        }

        public void SetVideoClip(VideoClip value, float focalLength)
        {
            VideoPlayer.clip = value;
            VideoFacingDirection = CameraFacingDirection.User;
            VideoFocalLength = focalLength;
            if (m_Running && m_BackgroundRenderer != null)
                CreateRecordedVideoTexture((int)value.width, (int)value.height);
        }

        void SetupVideoFeedTexture()
        {
            var videoClip = VideoPlayer.clip;
            if (videoClip != null)
                CreateRecordedVideoTexture((int)videoClip.width, (int)videoClip.height);
#if UNITY_EDITOR || !PLATFORM_LUMIN
            else if (m_LiveVideoTexture != null && m_LiveVideoTexture.width > k_InvalidWidth && VideoFeedTexture != m_LiveVideoTexture)
                ChangeVideoFeedTexture(m_LiveVideoTexture);
#endif
        }

        void CreateRecordedVideoTexture(int width, int height)
        {
            m_RecordedVideoTexture = new RenderTexture(width, height, 0);
            VideoPlayer.targetTexture = m_RecordedVideoTexture;
            ChangeVideoFeedTexture(m_RecordedVideoTexture);
        }

        void ChangeVideoFeedTexture(Texture texture)
        {
            var width = texture.width;
            var height = texture.height;
            VideoFeedTexture = texture;
            m_BackgroundMaterial.mainTexture = VideoFeedTexture;
            var backgroundTransform = m_BackgroundRenderer.transform;
            var backgroundScale = SimulationVideoContextSettings.instance.VideoBackgroundScale;
            backgroundTransform.localScale = new Vector3(width, height, 1f) * backgroundScale;
            backgroundTransform.localPosition = new Vector3(0f, 0f, VideoFocalLength * backgroundScale);
            VideoFeedTextureChanged?.Invoke(VideoFeedTexture);
        }
    }
}
