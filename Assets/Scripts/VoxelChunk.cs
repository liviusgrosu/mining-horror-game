using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class VoxelChunk : MonoBehaviour
{
    public const int ChunkSize = 16;
    public const int SampleSize = ChunkSize + 1; // 17 samples per axis

    public Vector3Int chunkCoord;

    private float[] _densities;
    private bool _isDirty;

    private MeshFilter _meshFilter;
    private MeshCollider _meshCollider;

    // Reusable lists to avoid GC allocs every rebuild
    private readonly List<Vector3> _vertices = new();
    private readonly List<int> _triangles = new();

    // Corner offset lookup (local to each marching cube cell)
    private static readonly Vector3Int[] CornerOffsets = new Vector3Int[8]
    {
        new(0, 0, 0), // 0
        new(1, 0, 0), // 1
        new(1, 0, 1), // 2
        new(0, 0, 1), // 3
        new(0, 1, 0), // 4
        new(1, 1, 0), // 5
        new(1, 1, 1), // 6
        new(0, 1, 1), // 7
    };

    public void Initialize(Vector3Int coord, Material material)
    {
        chunkCoord = coord;
        _densities = new float[SampleSize * SampleSize * SampleSize];

        _meshFilter = GetComponent<MeshFilter>();
        _meshCollider = GetComponent<MeshCollider>();
        GetComponent<MeshRenderer>().sharedMaterial = material;

        _meshFilter.mesh = new Mesh
        {
            indexFormat = UnityEngine.Rendering.IndexFormat.UInt32
        };

        tag = "VoxelTerrain";
    }

    private int Index(int x, int y, int z)
    {
        return x + y * SampleSize + z * SampleSize * SampleSize;
    }

    public float GetDensity(int x, int y, int z)
    {
        return _densities[Index(x, y, z)];
    }

    public void SetDensity(int x, int y, int z, float value)
    {
        _densities[Index(x, y, z)] = value;
    }

    public void SetAllDensities(float value)
    {
        for (int i = 0; i < _densities.Length; i++)
        {
            _densities[i] = value;
        }
    }

    public void MarkDirty()
    {
        _isDirty = true;
    }

    void LateUpdate()
    {
        if (_isDirty)
        {
            Rebuild();
            _isDirty = false;
        }
    }

    public void Rebuild()
    {
        _vertices.Clear();
        _triangles.Clear();

        var iso = MarchingCubesTables.IsoLevel;

        // March through each cell in the chunk
        for (var z = 0; z < ChunkSize; z++)
        for (var y = 0; y < ChunkSize; y++)
        for (var x = 0; x < ChunkSize; x++)
        {
            // Sample 8 corners
            var cornerDensities = new float[8];
            var cornerPositions = new Vector3[8];

            for (var i = 0; i < 8; i++)
            {
                var offset = CornerOffsets[i];
                var sx = x + offset.x;
                var sy = y + offset.y;
                var sz = z + offset.z;

                cornerDensities[i] = _densities[Index(sx, sy, sz)];
                cornerPositions[i] = new Vector3(sx, sy, sz);
            }

            // Build cube index from corner densities
            var cubeIndex = 0;
            for (var i = 0; i < 8; i++)
            {
                if (cornerDensities[i] >= iso)
                {
                    cubeIndex |= (1 << i);
                }
            }

            var edgeMask = MarchingCubesTables.EdgeTable[cubeIndex];
            if (edgeMask == 0)
            {
                continue;
            }

            // Interpolate edge vertices
            var edgeVertices = new Vector3[12];
            for (var i = 0; i < 12; i++)
            {
                if ((edgeMask & (1 << i)) == 0)
                {
                    continue;
                }
                var c0 = MarchingCubesTables.EdgeConnections[i, 0];
                var c1 = MarchingCubesTables.EdgeConnections[i, 1];
                edgeVertices[i] = InterpolateEdge(
                    cornerPositions[c0], cornerPositions[c1],
                    cornerDensities[c0], cornerDensities[c1], iso);
            }

            // Generate triangles
            for (var i = 0; MarchingCubesTables.TriTable[cubeIndex, i] != -1; i += 3)
            {
                var baseIndex = _vertices.Count;

                _vertices.Add(edgeVertices[MarchingCubesTables.TriTable[cubeIndex, i]]);
                _vertices.Add(edgeVertices[MarchingCubesTables.TriTable[cubeIndex, i + 1]]);
                _vertices.Add(edgeVertices[MarchingCubesTables.TriTable[cubeIndex, i + 2]]);

                _triangles.Add(baseIndex);
                _triangles.Add(baseIndex + 1);
                _triangles.Add(baseIndex + 2);
            }
        }

        var mesh = _meshFilter.mesh;
        mesh.Clear();
        mesh.SetVertices(_vertices);
        mesh.SetTriangles(_triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        _meshCollider.sharedMesh = null;
        _meshCollider.sharedMesh = mesh;
    }

    private static Vector3 InterpolateEdge(Vector3 p1, Vector3 p2, float v1, float v2, float iso)
    {
        if (Mathf.Abs(v1 - v2) < 0.00001f)
        {
            return (p1 + p2) * 0.5f;
        }

        var t = (iso - v1) / (v2 - v1);
        return Vector3.Lerp(p1, p2, t);
    }
}
