using System.Collections.Generic;
using Unity.XRTools.ModuleLoader;
using Unity.XRTools.Utils;
using UnityEngine;

namespace Unity.MARS.Behaviors
{
    class MARSPlaneVisualizer : MonoBehaviour, IUsesPlaneFinding, IUsesCameraOffset, ISimulatable
    {
        IProvidesPlaneFinding IFunctionalitySubscriber<IProvidesPlaneFinding>.provider { get; set; }
        IProvidesCameraOffset IFunctionalitySubscriber<IProvidesCameraOffset>.provider { get; set; }

#pragma warning disable 649
        [SerializeField]
        GameObject m_PlanePrefab;
#pragma warning restore 649

        [SerializeField]
        bool m_UsePlaneGeometry = true;

        [SerializeField]
        PlaneEdgeSettings m_EdgeSettings = PlaneEdgeSettings.Default;

#if UNITY_EDITOR
        bool m_UseParentLayer;
#endif

        readonly Dictionary<MarsTrackableId, KeyValuePair<GameObject, Mesh>> m_Planes =
            new Dictionary<MarsTrackableId, KeyValuePair<GameObject, Mesh>>();
        readonly Dictionary<MarsTrackableId, MeshCollider> m_Colliders = new Dictionary<MarsTrackableId, MeshCollider>();

        void OnEnable()
        {
#if UNITY_EDITOR
            var getSimScene = EditorOnlyDelegates.GetSimulatedEnvironmentScene;
            m_UseParentLayer = getSimScene != null &&
                gameObject.scene == getSimScene();
#endif

            this.SubscribePlaneAdded(PlaneAddedHandler);
            this.SubscribePlaneUpdated(PlaneUpdatedHandler);
            this.SubscribePlaneRemoved(PlaneRemovedHandler);

            var planes = new List<MRPlane>();
            this.GetPlanes(planes);
            foreach (var plane in planes)
            {
                CreateOrUpdateGameObject(plane);
            }
        }

        void OnDisable()
        {
            this.UnsubscribePlaneAdded(PlaneAddedHandler);
            this.UnsubscribePlaneUpdated(PlaneUpdatedHandler);
            this.UnsubscribePlaneRemoved(PlaneRemovedHandler);

            foreach (var pair in m_Planes)
            {
                var kvp = pair.Value;
                UnityObjectUtils.Destroy(kvp.Key);
                UnityObjectUtils.Destroy(kvp.Value);
            }

            m_Planes.Clear();
            m_Colliders.Clear();
        }

        void CreateOrUpdateGameObject(MRPlane plane)
        {
            if (MARSCore.instance.paused)
                return;

            GameObject go;
            Mesh mesh = null;
            KeyValuePair<GameObject, Mesh> kvp;
            var id = plane.id;
            var vertices = plane.vertices;
            var useGeometry = m_UsePlaneGeometry && vertices != null;
            if (m_Planes.TryGetValue(id, out kvp))
            {
                go = kvp.Key;
                mesh = kvp.Value;
                if (useGeometry && mesh == null)
                {
                    mesh = new Mesh();
                    m_Planes[id] = new KeyValuePair<GameObject, Mesh>(go, mesh);
                }
            }
            else
            {
                go = Instantiate(m_PlanePrefab, transform);

#if UNITY_EDITOR
                // This allows the instantiated objects to use layer locking for scene picking in the editor.
                if (m_UseParentLayer)
                    go.SetLayerAndHideFlagsRecursively(gameObject.layer, HideFlags.DontSave);
                else
                    go.SetHideFlagsRecursively(HideFlags.DontSave);
#else
                go.SetHideFlagsRecursively(HideFlags.DontSave);
#endif

                if (useGeometry)
                {
                    mesh = new Mesh();

                    var meshFilter = go.GetComponentInChildren<MeshFilter>();
                    if (meshFilter)
                        meshFilter.sharedMesh = mesh;

                    var meshCollider = go.GetComponentInChildren<MeshCollider>();
                    if (meshCollider)
                    {
                        meshCollider.sharedMesh = mesh;
                        m_Colliders.Add(id, meshCollider);
                    }
                }

                m_Planes.Add(id, new KeyValuePair<GameObject, Mesh>(go, mesh));
            }

            var goTransform = go.transform;
            var pose = this.ApplyOffsetToPose(plane.pose);
            goTransform.SetWorldPose(pose);

            var cameraScale = this.GetCameraScale();
            if (useGeometry)
            {
                List<Vector2> texCoords2 = null;
                if (m_EdgeSettings.MakeEdge)
                {
                    plane = PlaneUtils.GeneratePlaneWithBorders(plane, out texCoords2, m_EdgeSettings);
                }

                goTransform.localScale = Vector3.one * cameraScale;
                mesh.triangles = null;
                mesh.vertices = plane.vertices.ToArray();
                mesh.uv = plane.textureCoordinates.ToArray();
                if (texCoords2 != null)
                    mesh.uv2 = texCoords2.ToArray();
                else
                    mesh.uv2 = new Vector2[mesh.vertexCount];  // Set all to zero
                mesh.triangles = plane.indices.ToArray();
                mesh.RecalculateBounds();
                mesh.RecalculateNormals();

                if (m_Colliders.TryGetValue(id, out var collider))
                    collider.sharedMesh = mesh; // this need to re-applied to take effect
            }
            else
            {
                var extents = plane.extents;
                goTransform.position += pose.rotation * (plane.center * cameraScale);
                goTransform.localScale = new Vector3(extents.x, 1f, extents.y) * cameraScale;
            }
        }

        void PlaneAddedHandler(MRPlane plane)
        {
            if (m_PlanePrefab)
                CreateOrUpdateGameObject(plane);
        }

        void PlaneUpdatedHandler(MRPlane plane)
        {
            if (m_PlanePrefab)
                CreateOrUpdateGameObject(plane);
        }

        void PlaneRemovedHandler(MRPlane plane)
        {
            KeyValuePair<GameObject, Mesh> kvp;
            if (m_Planes.TryGetValue(plane.id, out kvp))
            {
                UnityObjectUtils.Destroy(kvp.Key);
                UnityObjectUtils.Destroy(kvp.Value);
                m_Planes.Remove(plane.id);
            }

            if (m_Colliders.ContainsKey(plane.id))
                m_Colliders.Remove(plane.id);
        }
    }
}
