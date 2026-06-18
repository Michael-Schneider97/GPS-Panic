using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;
using System.Collections.Generic;

namespace GPSPanic.Road.Spline
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(SplineContainer))]
    public class RoadMesh2D : MonoBehaviour
    {
        [Header("2D Visual Settings")]
        public Color roadColor = new Color(0.15f, 0.15f, 0.15f);
        public Color shoulderColor = new Color(0.25f, 0.25f, 0.25f);
        [SerializeField] private float laneWidth = 3.5f;
        [SerializeField] private float shoulderWidth = 1.0f;

        [Header("Generation Settings")]
        [SerializeField] private int segmentsPerUnit = 2;

        private static Material _sharedMaterial;

        public void Generate2DRoad(int startLanes, int endLanes)
        {
            if (GetComponent<SplineContainer>() == null) return;
            var spline = GetComponent<SplineContainer>().Spline;
            float length = spline.GetLength();
            if (length < 0.1f) return;

            int longitudinalSegments = Mathf.Max(2, Mathf.CeilToInt(length * segmentsPerUnit));

            Mesh mesh = new Mesh();
            mesh.name = "ArchitecturalRoad";

            List<Vector3> vertices = new List<Vector3>();
            List<Color> colors = new List<Color>();
            List<int> triangles = new List<int>();

            for (int i = 0; i <= longitudinalSegments; i++)
            {
                float t = i / (float)longitudinalSegments;
                spline.Evaluate(t, out float3 pos, out float3 tangent, out float3 up);
                
                // Dynamic Lane Count for this slice
                float currentLaneCount = Mathf.Lerp(startLanes, endLanes, t);
                float totalWidth = (currentLaneCount * laneWidth) + (shoulderWidth * 2);

                float2 dir2D = math.normalizesafe(new float2(tangent.x, tangent.y), new float2(0, 1));
                float3 side = new float3(-dir2D.y, dir2D.x, 0);

                float halfRoad = (currentLaneCount * laneWidth) / 2f;
                float halfTotal = totalWidth / 2f;

                vertices.Add(SafeVector((Vector3)(pos - (side * halfTotal))));
                vertices.Add(SafeVector((Vector3)(pos - (side * halfRoad))));
                vertices.Add(SafeVector((Vector3)(pos + (side * halfRoad))));
                vertices.Add(SafeVector((Vector3)(pos + (side * halfTotal))));

                colors.Add(shoulderColor);
                colors.Add(roadColor);
                colors.Add(roadColor);
                colors.Add(shoulderColor);
            }

            for (int i = 0; i < longitudinalSegments; i++)
            {
                int curr = i * 4;
                int next = (i + 1) * 4;
                
                // Left Shoulder
                AddQuad(triangles, curr, next, curr + 1, next + 1);
                // Main Road
                AddQuad(triangles, curr + 1, next + 1, curr + 2, next + 2);
                // Right Shoulder
                AddQuad(triangles, curr + 2, next + 2, curr + 3, next + 3);
            }

            mesh.SetVertices(vertices);
            mesh.SetColors(colors);
            mesh.SetIndices(triangles, MeshTopology.Triangles, 0);
            
            // Critical for visuals: Remove normals to keep it unlit
            mesh.normals = new Vector3[0];
            mesh.RecalculateBounds();

            GetComponent<MeshFilter>().mesh = mesh;
            
            MeshRenderer renderer = GetComponent<MeshRenderer>();
            renderer.sortingOrder = -10;

            if (_sharedMaterial == null)
            {
                _sharedMaterial = new Material(Shader.Find("Sprites/Default"));
                _sharedMaterial.SetColor("_Color", Color.white);
            }
            renderer.sharedMaterial = _sharedMaterial;
        }

        private void AddQuad(List<int> tris, int v0, int v1, int v2, int v3)
        {
            // v0, v1 is the bottom edge, v2, v3 is the top edge
            tris.Add(v0); tris.Add(v1); tris.Add(v2);
            tris.Add(v2); tris.Add(v1); tris.Add(v3);
        }

        private Vector3 SafeVector(Vector3 v)
        {
            if (float.IsNaN(v.x) || float.IsInfinity(v.x)) v.x = 0;
            if (float.IsNaN(v.y) || float.IsInfinity(v.y)) v.y = 0;
            if (float.IsNaN(v.z) || float.IsInfinity(v.z)) v.z = 0;
            return v;
        }
    }
}
