using System.Collections.Generic;
using System.Linq;
using Unity.XRTools.Utils;
using UnityEngine;

namespace Unity.MARS
{
    [System.Serializable]
    public struct PlaneEdgeSettings
    {
        public bool MakeEdge;
        public float EdgeWidth;
        public float EdgeRepeatDistance;

        public static PlaneEdgeSettings Default => new PlaneEdgeSettings()
            { MakeEdge = false, EdgeWidth = 0.07f, EdgeRepeatDistance = 0.07f };
    }

    public static class PlaneUtils
    {
        static readonly Vector3 k_Up = Vector3.up;

        /// <summary>
        /// Triangulates the polygon and tiles the UV data correctly from the polygon center.
        /// Sets normals to local up.
        /// </summary>
        /// <param name="pose">Input Pose of the source plane</param>
        /// <param name="vertices">Input Vertices of the polygon.</param>
        /// <param name="indices">Output Index buffer to fill for triangulation</param>
        /// <param name="normals">Output for vertex normals</param>
        /// <param name="texCoords">Output uv coordinates.</param>
        public static void TriangulatePlaneFromVertices(in Pose pose, List<Vector3> vertices, List<int> indices, List<Vector3> normals, List<Vector2> texCoords)
        {
            var uvPose = GeometryUtils.PolygonUVPoseFromPlanePose(pose);
            var vertsCount = vertices.Count;
            GeometryUtils.TriangulatePolygon(indices, vertsCount);

            for (var i = 0; i < vertsCount; i++)
            {
                var uvCoord = GeometryUtils.PolygonVertexToUV(vertices[i], pose, uvPose);
                texCoords.Add(uvCoord);
                normals.Add(k_Up);
            }
        }

        static void EnsureCount<X>(List<X> list, int count)
        {
            while (list.Count < count)
                list.Add(default(X));
        }

        static List<X> DefaultInitializedList<X>(int count)
        {
            return new List<X>(Enumerable.Repeat(default(X), count));
        }

        public static MRPlane GeneratePlaneWithBorders(MRPlane source, out List<Vector2> texCoords2, PlaneEdgeSettings settings)
        {
            MRPlane result = source;

            var uvPose = GeometryUtils.PolygonUVPoseFromPlanePose(source.pose);

            var borderWidth = settings.EdgeWidth;
            var borderLengthTiling = settings.EdgeRepeatDistance;

            var ringLength = source.vertices.Count;
            var nVerts = ringLength * 2 + 2;

            result.vertices = DefaultInitializedList<Vector3>(nVerts);
            result.textureCoordinates = DefaultInitializedList<Vector2>(nVerts);
            texCoords2 = DefaultInitializedList<Vector2>(nVerts);
            result.indices = new List<int>(source.indices);
            if (result.normals != null)
            {
                result.normals = new List<Vector3>(source.normals);
                result.normals.AddRange(source.normals);
            }

            var tex2Along = 0.0f;
            for (var edgeIndex = 0; edgeIndex < ringLength; edgeIndex++)
            {
                var sourceLoc = source.vertices[edgeIndex];
                var sourceNext = source.vertices[(edgeIndex+1)%ringLength];
                var sourcePrev = source.vertices[(edgeIndex + ringLength - 1) % ringLength];
                var inDir = Vector3.Lerp(
                    Vector3.Cross(Vector3.up, (sourceNext - sourceLoc)),
                    Vector3.Cross(Vector3.up, (sourceLoc - sourcePrev)),
                    0.5f).normalized;

                var inLoc = sourceLoc + (inDir * borderWidth);
                var inUV1 = GeometryUtils.PolygonVertexToUV(inLoc, source.pose, uvPose);
                var otUV1 = GeometryUtils.PolygonVertexToUV(sourceLoc, source.pose, uvPose);

                var outerIndex = edgeIndex + ringLength;
                result.vertices[edgeIndex] = inLoc;
                result.vertices[outerIndex] = sourceLoc;
                result.textureCoordinates[edgeIndex] = inUV1;
                result.textureCoordinates[outerIndex] = otUV1;
                texCoords2[edgeIndex] = new Vector2(tex2Along, 0.0f);
                texCoords2[outerIndex] = new Vector2(tex2Along, 1.0f);

                tex2Along += (sourceNext - sourceLoc).magnitude / borderLengthTiling;

                var nextEdgeIndex = (edgeIndex + 1) % ringLength;
                if (nextEdgeIndex < ringLength)
                {
                    var innerNext = nextEdgeIndex + ringLength;

                    result.indices.Add(edgeIndex);
                    result.indices.Add(outerIndex);
                    result.indices.Add(nextEdgeIndex);

                    result.indices.Add(nextEdgeIndex);
                    result.indices.Add(outerIndex);
                    result.indices.Add(innerNext);
                }
            }

            // Close loop by duplicating first two verts with new uv2.
            var startIndex = 0;
            var endIndex = ringLength * 2;
            result.vertices[endIndex] = result.vertices[startIndex];
            result.vertices[endIndex + 1] = result.vertices[startIndex + ringLength];
            result.textureCoordinates[endIndex] = result.textureCoordinates[startIndex];
            result.textureCoordinates[endIndex + 1] = result.textureCoordinates[startIndex + ringLength];
            if (result.normals != null)
            {
                result.normals.Add(result.normals[startIndex]);
                result.normals.Add(result.normals[startIndex + ringLength]);
            }
            tex2Along = Mathf.Round(tex2Along);
            texCoords2[endIndex] = new Vector2(tex2Along, 0f);
            texCoords2[endIndex + 1] = new Vector2(tex2Along, 1f);

            return result;
        }
    }
}
