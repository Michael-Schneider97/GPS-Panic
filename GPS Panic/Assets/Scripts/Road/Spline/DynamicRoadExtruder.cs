using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;
using System.Collections.Generic;

namespace GPSPanic.Road.Spline
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(SplineContainer))]
    public class DynamicRoadExtruder : MonoBehaviour
    {
        [Header("Lane Settings")]
        [SerializeField] private float laneWidth = 3.5f;
        [SerializeField] private float shoulderWidth = 1.0f;
        [SerializeField] private float roadThickness = 0.1f;

        [Header("Resolution")]
        [SerializeField] private int segmentsPerUnit = 2;
        [SerializeField] private int uTileFrequency = 1;

        private SplineContainer splineContainer;
        private MeshFilter meshFilter;

        public void GenerateRoadMesh(int laneCount)
        {
            if (splineContainer == null) splineContainer = GetComponent<SplineContainer>();
            if (meshFilter == null) meshFilter = GetComponent<MeshFilter>();

            var spline = splineContainer.Spline;
            float totalWidth = (laneCount * laneWidth) + (shoulderWidth * 2);
            int radialSegments = 2; // Flat road top, but could be more for rounded curbs
            
            float length = spline.GetLength();
            int longitudinalSegments = Mathf.Max(2, Mathf.CeilToInt(length * segmentsPerUnit));

            Mesh mesh = new Mesh();
            mesh.name = "DynamicRoad";

            List<Vector3> vertices = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> triangles = new List<int>();

            for (int i = 0; i <= longitudinalSegments; i++)
            {
                float t = i / (float)longitudinalSegments;
                spline.Evaluate(t, out float3 pos, out float3 tangent, out float3 up);
                
                float3 right = math.normalize(math.cross(tangent, up));
                
                // Create vertices for this cross-section (Left Shoulder to Right Shoulder)
                for (int j = 0; j <= 1; j++) // Simple flat road: 0 is left edge, 1 is right edge
                {
                    float side = (j == 0) ? -0.5f : 0.5f;
                    Vector3 vertPos = (Vector3)(pos + (right * side * totalWidth));
                    vertices.Add(vertPos);
                    
                    // UVs: X is across road, Y is along road
                    uvs.Add(new Vector2(j, t * length * uTileFrequency));
                }
            }

            // Create Triangles
            for (int i = 0; i < longitudinalSegments; i++)
            {
                int start = i * 2;
                triangles.Add(start);
                triangles.Add(start + 1);
                triangles.Add(start + 2);

                triangles.Add(start + 1);
                triangles.Add(start + 3);
                triangles.Add(start + 2);
            }

            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
            mesh.SetIndices(triangles, MeshTopology.Triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            meshFilter.mesh = mesh;
            
            // Update the Collider if exists
            if (TryGetComponent<MeshCollider>(out var meshCollider))
            {
                meshCollider.sharedMesh = mesh;
            }
        }
    }
}
