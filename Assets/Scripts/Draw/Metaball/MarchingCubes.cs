using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MarchingCubes : MonoBehaviour
{
    [Header("Mesh Quality")]
    [Tooltip("Target size of each voxel in meters. Recommended ~0.005 to 0.01. Increase for performance")]
    [SerializeField] private float voxelSize = 0.008f;

    [Tooltip("Positive values SHRINK the mesh. Negative values INFLATE it.")]
    [SerializeField] public float surfaceOffset = 0.002f;

    [Tooltip("Extra space added beyond sphere surfaces.")]
    [SerializeField] private float boundsPadding = 0.12f;

    [Header("Appearance")]
    [SerializeField] public Material weldMaterial;

    private MeshFilter liveMeshFilter;
    private MeshRenderer liveMeshRenderer;
    private bool liveDirty = false;

    private Vector4[] pendingTargets;
    private int pendingTargetCount;
    private Vector4[] pendingInfluencers;
    private int pendingInfluencerCount;
    private float pendingWeldStrength;

    private void Awake()
    {
        liveMeshFilter = GetComponent<MeshFilter>();
        liveMeshRenderer = GetComponent<MeshRenderer>();
        if (weldMaterial != null)
            liveMeshRenderer.sharedMaterial = weldMaterial;
    }

    /// <summary>
    /// Mark edited in Weld Controller
    /// </summary>
    /// <param name="targets"></param>
    /// <param name="targetCount"></param>
    /// <param name="influencers"></param>
    /// <param name="influencerCount"></param>
    /// <param name="weldStrength"></param>
    public void MarkDirty(Vector4[] targets, int targetCount, Vector4[] influencers, int influencerCount, float weldStrength)
    {
        pendingTargets = targets;
        pendingTargetCount = targetCount;
        pendingInfluencers = influencers;
        pendingInfluencerCount = influencerCount;
        pendingWeldStrength = weldStrength;
        liveDirty = true;
    }

    /// <summary>
    /// Clear any mesh that is live - at the end of each stroke
    /// </summary>
    public void ClearLivePreview()
    {
        if (liveMeshFilter.mesh != null)
        {
            Destroy(liveMeshFilter.mesh);
            liveMeshFilter.mesh = null;
        }
        liveDirty = false;
    }

    /// <summary>
    /// Generates a static mesh for a chunk
    /// Returns the raw Mesh data to be combined later.
    /// </summary>
    public Mesh BakeStaticMesh(Vector4[] targets, int targetCount, Vector4[] influencers, int influencerCount, float weldStrength)
    {
        if (targetCount == 0) return null;

        Vector4[] localTargets = WorldToLocal(targets, targetCount);
        Vector4[] localInfluencers = WorldToLocal(influencers, influencerCount);

        ComputeBounds(localTargets, targetCount, out Vector3 gridCenter, out float boundsSize, out int calcGridSize);

        Vector4[] centeredInfluencers = CenterSpheres(localInfluencers, influencerCount, gridCenter);
        float[] grid = GenerateScalarField(centeredInfluencers, influencerCount, boundsSize, calcGridSize, weldStrength);
        Mesh mesh = Triangulate(grid, boundsSize, calcGridSize);

        if (mesh == null || mesh.vertexCount == 0) return null;

        // Shift vertices by gridCenter so they sit at the correct local position
        Vector3[] verts = mesh.vertices;
        for (int i = 0; i < verts.Length; i++)
            verts[i] += gridCenter;

        mesh.vertices = verts;
        mesh.RecalculateBounds();

        return mesh; 
    }

    private void LateUpdate()
    {
        if (!liveDirty) return;
        liveDirty = false;

        if (pendingTargets == null || pendingTargetCount == 0)
        {
            ClearLivePreview();
            return;
        }

        RebuildLivePreview();
    }

    private void RebuildLivePreview()
    {
        Vector4[] localTargets = WorldToLocal(pendingTargets, pendingTargetCount);
        Vector4[] localInfluencers = WorldToLocal(pendingInfluencers, pendingInfluencerCount);

        ComputeBounds(localTargets, pendingTargetCount, out Vector3 gridCenter, out float boundsSize, out int calcGridSize);

        Vector4[] centeredInfluencers = CenterSpheres(localInfluencers, pendingInfluencerCount, gridCenter);
        float[] grid = GenerateScalarField(centeredInfluencers, pendingInfluencerCount, boundsSize, calcGridSize, pendingWeldStrength);
        Mesh mesh = Triangulate(grid, boundsSize, calcGridSize);

        if (mesh == null || mesh.vertexCount == 0)
        {
            ClearLivePreview();
            return;
        }

        Vector3[] verts = mesh.vertices;
        for (int i = 0; i < verts.Length; i++)
            verts[i] += gridCenter;

        mesh.vertices = verts;
        mesh.RecalculateBounds();

        if (liveMeshFilter.mesh != null)
            Destroy(liveMeshFilter.mesh);
        liveMeshFilter.mesh = mesh;
    }

    private Vector4[] WorldToLocal(Vector4[] spheres, int count)
    {
        var local = new Vector4[count];
        for (int i = 0; i < count; i++)
        {
            Vector3 lp = transform.InverseTransformPoint(new Vector3(spheres[i].x, spheres[i].y, spheres[i].z));
            local[i] = new Vector4(lp.x, lp.y, lp.z, spheres[i].w);
        }
        return local;
    }

    private void ComputeBounds(Vector4[] localTargets, int count, out Vector3 center, out float size, out int calcGridSize)
    {
        var mn = new Vector3(localTargets[0].x, localTargets[0].y, localTargets[0].z);
        var mx = mn;

        for (int i = 0; i < count; i++)
        {
            float r = localTargets[i].w;
            Vector3 c = new Vector3(localTargets[i].x, localTargets[i].y, localTargets[i].z);
            mn = Vector3.Min(mn, c - Vector3.one * r);
            mx = Vector3.Max(mx, c + Vector3.one * r);
        }

        mn -= Vector3.one * boundsPadding;
        mx += Vector3.one * boundsPadding;

        float maxExt = Mathf.Max(mx.x - mn.x, Mathf.Max(mx.y - mn.y, mx.z - mn.z));
        calcGridSize = Mathf.CeilToInt(maxExt / voxelSize);
        size = calcGridSize * voxelSize;

        Vector3 idealCenter = (mn + mx) * 0.5f;
        Vector3 gridMin = idealCenter - Vector3.one * (size * 0.5f);

        gridMin.x = Mathf.Round(gridMin.x / voxelSize) * voxelSize;
        gridMin.y = Mathf.Round(gridMin.y / voxelSize) * voxelSize;
        gridMin.z = Mathf.Round(gridMin.z / voxelSize) * voxelSize;

        center = gridMin + Vector3.one * (size * 0.5f);
    }

    private Vector4[] CenterSpheres(Vector4[] local, int count, Vector3 gridCenter)
    {
        var out_ = new Vector4[count];
        for (int i = 0; i < count; i++)
        {
            Vector3 p = new Vector3(local[i].x, local[i].y, local[i].z) - gridCenter;
            out_[i] = new Vector4(p.x, p.y, p.z, local[i].w);
        }
        return out_;
    }

    private float[] GenerateScalarField(Vector4[] spheres, int count, float size, int calcGridSize, float weldStrength)
    {
        int n = calcGridSize + 1;
        float step = size / calcGridSize;
        float half = size * 0.5f;
        var grid = new float[n * n * n];

        for (int z = 0; z <= calcGridSize; z++)
            for (int y = 0; y <= calcGridSize; y++)
                for (int x = 0; x <= calcGridSize; x++)
                {
                    Vector3 p = new Vector3(x * step - half, y * step - half, z * step - half);
                    float d = 1000f;

                    for (int i = 0; i < count; i++)
                    {
                        Vector3 sp = new Vector3(spheres[i].x, spheres[i].y, spheres[i].z);

                        // Fast distance optimization bounds check
                        float maxInf = spheres[i].w + weldStrength * 2f;
                        if (Mathf.Abs(p.x - sp.x) > maxInf || Mathf.Abs(p.y - sp.y) > maxInf || Mathf.Abs(p.z - sp.z) > maxInf) continue;

                        float dist = Vector3.Distance(p, sp) - spheres[i].w;
                        float h = Mathf.Max(weldStrength - Mathf.Abs(d - dist), 0f) / weldStrength;
                        d = Mathf.Min(d, dist) - h * h * weldStrength * 0.25f;
                    }

                    // grid[x + y * n + z * n * n] = d;
                    grid[x + y * n + z * n * n] = d + surfaceOffset;
                }

        return grid;
    }

    private Mesh Triangulate(float[] grid, float size, int calcGridSize)
    {
        var verts = new List<Vector3>();
        var tris = new List<int>();
        int n = calcGridSize + 1;
        float step = size / calcGridSize;
        float half = size * 0.5f;

        for (int z = 0; z < calcGridSize; z++)
            for (int y = 0; y < calcGridSize; y++)
                for (int x = 0; x < calcGridSize; x++)
                {
                    var cv = new float[8];
                    for (int c = 0; c < 8; c++)
                    {
                        var (cx, cy, cz) = CornerCoords(x, y, z, c);
                        cv[c] = grid[cx + cy * n + cz * n * n];
                    }

                    int ci = 0;
                    if (cv[0] < 0f) ci |= 1; if (cv[1] < 0f) ci |= 2;
                    if (cv[2] < 0f) ci |= 4; if (cv[3] < 0f) ci |= 8;
                    if (cv[4] < 0f) ci |= 16; if (cv[5] < 0f) ci |= 32;
                    if (cv[6] < 0f) ci |= 64; if (cv[7] < 0f) ci |= 128;

                    int[] row = CubeLookup.TriangleConnectionTable[ci];
                    if (row[0] == -1) continue;

                    for (int t = 0; row[t] != -1; t += 3)
                    {
                        int b = verts.Count;
                        verts.Add(InterpVert(x, y, z, row[t], cv, step, half));
                        verts.Add(InterpVert(x, y, z, row[t + 1], cv, step, half));
                        verts.Add(InterpVert(x, y, z, row[t + 2], cv, step, half));
                        tris.Add(b); tris.Add(b + 1); tris.Add(b + 2);
                    }
                }

        if (verts.Count == 0) return null;

        var m = new Mesh();
        m.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        m.SetVertices(verts);
        m.SetTriangles(tris, 0);
        m.RecalculateNormals();
        m.RecalculateBounds();
        return m;
    }

    private Vector3 InterpVert(int x, int y, int z, int edge, float[] val, float step, float half)
    {
        int c1 = CubeLookup.EdgeVertexIndices[edge, 0];
        int c2 = CubeLookup.EdgeVertexIndices[edge, 1];
        Vector3 p1 = CornerPos(x, y, z, c1, step, half);
        Vector3 p2 = CornerPos(x, y, z, c2, step, half);
        float v1 = val[c1], v2 = val[c2];
        float t = Mathf.Abs(v2 - v1) < 0.0001f ? 0.5f : Mathf.Clamp01(-v1 / (v2 - v1));
        return Vector3.Lerp(p1, p2, t);
    }

    private Vector3 CornerPos(int x, int y, int z, int corner, float step, float half)
    {
        var (cx, cy, cz) = CornerCoords(x, y, z, corner);
        return new Vector3(cx * step - half, cy * step - half, cz * step - half);
    }

    private (int, int, int) CornerCoords(int x, int y, int z, int corner) =>
        (x + (corner & 1), y + ((corner >> 1) & 1), z + ((corner >> 2) & 1));
}