using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class VoxelChunk : MonoBehaviour
{
    public const int ChunkSize = 16;
    public const int SampleSize = ChunkSize + 1; // 17 samples per axis

    public Vector3Int chunkCoord;

    // Serialized so densities persist in the scene between domain reloads
    [HideInInspector, SerializeField] private float[] _densities;
    [HideInInspector, SerializeField] private bool _initialized;

    private bool _isDirty;
    private MeshFilter _meshFilter;
    private MeshCollider _meshCollider;

    private readonly List<Vector3> _vertices = new();
    private readonly List<int> _triangles = new();
    private readonly List<Vector2> _uvs = new();

    private static readonly Vector3Int[] CornerOffsets = new Vector3Int[8]
    {
        new(0, 0, 0),
        new(1, 0, 0),
        new(1, 0, 1),
        new(0, 0, 1),
        new(0, 1, 0),
        new(1, 1, 0),
        new(1, 1, 1),
        new(0, 1, 1),
    };

    private void OnEnable()
    {
        _meshFilter = GetComponent<MeshFilter>();
        _meshCollider = GetComponent<MeshCollider>();

        // Rebuild mesh from serialized densities after domain reload / scene load
        if (_initialized && _densities != null && _densities.Length > 0)
        {
            EnsureMesh();
            Rebuild();
        }
    }

    public void Initialize(Vector3Int coord, Material material)
    {
        chunkCoord = coord;
        _densities = new float[SampleSize * SampleSize * SampleSize];
        _initialized = true;

        _meshFilter = GetComponent<MeshFilter>();
        _meshCollider = GetComponent<MeshCollider>();
        GetComponent<MeshRenderer>().sharedMaterial = material;

        EnsureMesh();
        tag = "VoxelTerrain";
    }

    private void EnsureMesh()
    {
        if (_meshFilter.sharedMesh == null)
        {
            _meshFilter.sharedMesh = new Mesh
            {
                name = $"VoxelChunk_{chunkCoord}",
                indexFormat = UnityEngine.Rendering.IndexFormat.UInt32
            };
        }
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
            _densities[i] = value;
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
        if (_densities == null || _densities.Length == 0) return;

        _vertices.Clear();
        _triangles.Clear();
        _uvs.Clear();

        var iso = MarchingCubesTables.IsoLevel;

        for (var z = 0; z < ChunkSize; z++)
        for (var y = 0; y < ChunkSize; y++)
        for (var x = 0; x < ChunkSize; x++)
        {
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

            var cubeIndex = 0;
            for (var i = 0; i < 8; i++)
            {
                if (cornerDensities[i] >= iso)
                    cubeIndex |= (1 << i);
            }

            var edgeMask = MarchingCubesTables.EdgeTable[cubeIndex];
            if (edgeMask == 0)
                continue;

            var edgeVertices = new Vector3[12];
            for (var i = 0; i < 12; i++)
            {
                if ((edgeMask & (1 << i)) == 0)
                    continue;
                var c0 = MarchingCubesTables.EdgeConnections[i, 0];
                var c1 = MarchingCubesTables.EdgeConnections[i, 1];
                edgeVertices[i] = InterpolateEdge(
                    cornerPositions[c0], cornerPositions[c1],
                    cornerDensities[c0], cornerDensities[c1], iso);
            }

            for (var i = 0; MarchingCubesTables.TriTable[cubeIndex, i] != -1; i += 3)
            {
                var baseIndex = _vertices.Count;

                var v0 = edgeVertices[MarchingCubesTables.TriTable[cubeIndex, i]];
                var v1 = edgeVertices[MarchingCubesTables.TriTable[cubeIndex, i + 1]];
                var v2 = edgeVertices[MarchingCubesTables.TriTable[cubeIndex, i + 2]];

                _vertices.Add(v0);
                _vertices.Add(v1);
                _vertices.Add(v2);

                var faceNormal = Vector3.Cross(v1 - v0, v2 - v0);
                var absX = Mathf.Abs(faceNormal.x);
                var absY = Mathf.Abs(faceNormal.y);
                var absZ = Mathf.Abs(faceNormal.z);

                var scale = 1f / ChunkSize;
                if (absY >= absX && absY >= absZ)
                {
                    _uvs.Add(new Vector2(v0.x, v0.z) * scale);
                    _uvs.Add(new Vector2(v1.x, v1.z) * scale);
                    _uvs.Add(new Vector2(v2.x, v2.z) * scale);
                }
                else if (absX >= absZ)
                {
                    _uvs.Add(new Vector2(v0.z, v0.y) * scale);
                    _uvs.Add(new Vector2(v1.z, v1.y) * scale);
                    _uvs.Add(new Vector2(v2.z, v2.y) * scale);
                }
                else
                {
                    _uvs.Add(new Vector2(v0.x, v0.y) * scale);
                    _uvs.Add(new Vector2(v1.x, v1.y) * scale);
                    _uvs.Add(new Vector2(v2.x, v2.y) * scale);
                }

                _triangles.Add(baseIndex);
                _triangles.Add(baseIndex + 1);
                _triangles.Add(baseIndex + 2);
            }
        }

        EnsureMesh();
        var mesh = _meshFilter.sharedMesh;
        mesh.Clear();
        mesh.SetVertices(_vertices);
        mesh.SetUVs(0, _uvs);
        mesh.SetTriangles(_triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        _meshCollider.sharedMesh = null;
        _meshCollider.sharedMesh = mesh;
    }

    private static Vector3 InterpolateEdge(Vector3 p1, Vector3 p2, float v1, float v2, float iso)
    {
        if (Mathf.Abs(v1 - v2) < 0.00001f)
            return (p1 + p2) * 0.5f;

        var t = (iso - v1) / (v2 - v1);
        return Vector3.Lerp(p1, p2, t);
    }
}
