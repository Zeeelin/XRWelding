using Oculus.Interaction;
using UnityEngine;

public class SurfaceDrawing : MonoBehaviour
{
    // hardness between 2 (sharp) and 0.5 (round)
    public float hardness = 1f;

    private Mesh mesh;
    private Vector3[] vertices;
    private Vector3[] originalVertices;

    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        vertices = mesh.vertices;
        originalVertices = mesh.vertices; 

        mesh.MarkDynamic();
    }

    /// <summary>
    /// Alter the shape of dynamic mesh in reference to the pin art toys
    /// </summary>
    /// <param name="worldHitPoint"></param>
    /// <param name="radius"></param>
    /// <param name="strength"></param>
    /// <param name="maxAccumulation"></param>
    public void DeformMesh(Vector3 worldHitPoint, float radius, float strength, float maxAccumulation)
    {
        Vector3 localHitPoint = transform.InverseTransformPoint(worldHitPoint);
        bool meshChanged = false;

        float radiusSqr = radius * radius;

        for (int i = 0; i < vertices.Length; i++)
        {
            float dx = localHitPoint.x - vertices[i].x;
            float dz = localHitPoint.z - vertices[i].z;
            float distSqr = dx * dx + dz * dz;

            // Check against radius squared
            if (distSqr < radiusSqr)
            {

                Vector3 deformation = Vector3.up * strength * hardness * Time.deltaTime;
                vertices[i] += deformation;

                // Height limit set by maxAxxumulation
                float limit = originalVertices[i].y + maxAccumulation;
                if (vertices[i].y > limit)
                {
                    vertices[i].y = limit;
                }

                meshChanged = true;
            }
        }

        if (meshChanged)
        {
            mesh.vertices = vertices;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
        }
    }

}
