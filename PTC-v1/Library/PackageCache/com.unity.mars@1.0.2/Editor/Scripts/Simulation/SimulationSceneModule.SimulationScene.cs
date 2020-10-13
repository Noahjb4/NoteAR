using System;
using System.Collections.Generic;
using Unity.XRTools.Utils;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Unity.MARS
{
    public partial class SimulationSceneModule
    {
        //Adapted from PreviewRenderUtility.cs PreviewScene
        class SimulationScene : IDisposable
        {
            static readonly CreateSceneParameters k_EnvironmentSceneParameters = new CreateSceneParameters(LocalPhysicsMode.Physics3D);

            readonly HashSet<Camera> m_Cameras = new HashSet<Camera>();
            readonly bool m_IsPreview;

            internal Scene contentScene { get; private set; }
            internal Scene environmentScene { get; private set; }
            internal GameObject contentRoot { get; private set; }
            internal GameObject environmentRoot { get; private set; }
            static bool useFallbackRendering => CompositeRenderModuleOptions.instance.UseFallbackCompositeRendering;

            internal SimulationScene()
            {
                // When not in play mode, use a preview scene for simulation
                m_IsPreview = !(EditorApplication.isPlayingOrWillChangePlaymode);

                contentScene = m_IsPreview
                    ? EditorSceneManager.NewPreviewScene()
                    : SceneManager.CreateScene("Simulated Content Scene");

                if (!contentScene.IsValid())
                    throw new InvalidOperationException("Preview scene could not be created");

                contentRoot = new GameObject("Simulated Content Root");
                SceneManager.MoveGameObjectToScene(contentRoot, contentScene);

                environmentScene = m_IsPreview
                    ? useFallbackRendering ? contentScene : EditorSceneManager.NewPreviewScene()
                    : SceneManager.CreateScene("Simulated Environment Scene", k_EnvironmentSceneParameters);

                if (!environmentScene.IsValid())
                    throw new InvalidOperationException("Preview scene could not be created");

                environmentRoot = new GameObject("Simulated Environment Root");
                SceneManager.MoveGameObjectToScene(environmentRoot, environmentScene);

                EditorOnlyDelegates.GetAllSimulationSceneCameras = GetAllSimulationSceneCameras;
            }

            internal void AddSimulatedGameObject(GameObject go, bool isContent, bool keepAtRoot)
            {
                if (keepAtRoot)
                {
                    go.transform.parent = null;
                    SceneManager.MoveGameObjectToScene(go, isContent ? contentScene : environmentScene);
                }
                else
                {
                    go.transform.SetParent(isContent ? contentRoot.transform : environmentRoot.transform, true);
                }
            }

            internal bool IsCameraAssignedToSimulationScene(Camera camera)
            {
                var cameraScene = camera.scene;
                return m_Cameras.Contains(camera) && (cameraScene == contentScene || cameraScene == environmentScene);
            }

            internal void AssignCameraToSimulation(Camera camera)
            {
                if (IsCameraAssignedToSimulationScene(camera))
                {
                    Debug.LogWarning($"{camera.name} is already assigned to Sim View.");
                    return;
                }

                m_Cameras.Add(camera);
            }

            internal void RemoveCameraFromSimulationScene(Camera camera, bool dispose = false)
            {
                if (dispose || !m_Cameras.Remove(camera))
                    return;

                camera.scene = SceneManager.GetActiveScene();
            }

            public void Dispose()
            {
                if (m_Cameras != null && m_Cameras.Count > 0)
                {
                    foreach (var camera in m_Cameras)
                    {
                        if (camera != null)
                            RemoveCameraFromSimulationScene(camera, true);
                    }
                    m_Cameras.Clear();
                }

                CloseScene(contentScene, contentRoot);
                contentRoot = null;
                CloseScene(environmentScene, environmentRoot);
                environmentRoot = null;

                EditorOnlyDelegates.GetAllSimulationSceneCameras = null;
                contentScene = new Scene();
                environmentScene = new Scene();
            }

            void CloseScene(Scene scene, GameObject root)
            {
                if (!scene.IsValid())
                    return;

                if (root != null)
                    UnityObjectUtils.Destroy(root);

                if (m_IsPreview)
                    EditorSceneManager.ClosePreviewScene(scene);
                else
                    SceneManager.UnloadSceneAsync(scene);
            }

            void GetAllSimulationSceneCameras(List<Camera> cameras)
            {
                cameras.AddRange(m_Cameras);
            }
        }
    }
}
