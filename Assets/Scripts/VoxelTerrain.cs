using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A placeable solid voxel block that the player can mine into.
/// Assign a source model to voxelize its shape, or leave it empty for a solid cube.
/// </summary>
public class VoxelTerrain : MonoBehaviour
{
    public static VoxelTerrain Instance;

    [Header("Source Model (optional — leave empty for solid cube)")]
    [SerializeField] private GameObject sourceModel;
    [SerializeField] private float voxelsPerUnit = 1f;

    [Header("Block Size (used when no source model, in chunks of 16 voxels)")]
    [SerializeField] private Vector3Int sizeInChunks = new Vector3Int(2, 2, 2);

    [Header("Rendering")]
    [SerializeField] private Material terrainMaterial;

    [Header("Mining")]
    [SerializeField] private float mineRadius = 1.5f;

    private Dictionary<Vector3Int, VoxelChunk> _chunks = new Dictionary<Vector3Int, VoxelChunk>();
    private bool _isGenerating;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (sourceModel != null)
            StartCoroutine(GenerateFromModelCoroutine());
        else
            GenerateBlock();
    }

    private void GenerateBlock()
    {
        for (var cx = 0; cx < sizeInChunks.x; cx++)
        for (var cy = 0; cy < sizeInChunks.y; cy++)
        for (var cz = 0; cz < sizeInChunks.z; cz++)
            CreateChunk(new Vector3Int(cx, cy, cz));

        var totalX = sizeInChunks.x * VoxelChunk.ChunkSize;
        var totalY = sizeInChunks.y * VoxelChunk.ChunkSize;
        var totalZ = sizeInChunks.z * VoxelChunk.ChunkSize;

        foreach (var (chunkCoordinates, chunk) in _chunks)
        {
            for (var z = 0; z < VoxelChunk.SampleSize; z++)
            for (var y = 0; y < VoxelChunk.SampleSize; y++)
            for (var x = 0; x < VoxelChunk.SampleSize; x++)
            {
                var wx = chunkCoordinates.x * VoxelChunk.ChunkSize + x;
                var wy = chunkCoordinates.y * VoxelChunk.ChunkSize + y;
                var wz = chunkCoordinates.z * VoxelChunk.ChunkSize + z;

                var isEdge = wx == 0 || wx >= totalX ||
                             wy == 0 || wy >= totalY ||
                             wz == 0 || wz >= totalZ;

                chunk.SetDensity(x, y, z, isEdge ? 0f : 1f);
            }
        }

        foreach (var chunk in _chunks.Values)
            chunk.Rebuild();
    }

    private IEnumerator GenerateFromModelCoroutine()
    {
        _isGenerating = true;

        var meshFilter = sourceModel.GetComponent<MeshFilter>();
        if (meshFilter == null || meshFilter.sharedMesh == null)
        {
            Debug.LogError("VoxelTerrain: Source model needs a MeshFilter with a mesh.");
            _isGenerating = false;
            yield break;
        }

        var sourceMesh = meshFilter.sharedMesh;
        var sourceTransform = sourceModel.transform;

        // Read vertices and transform to world space
        var localVerts = sourceMesh.vertices;
        var worldVerts = new Vector3[localVerts.Length];
        for (var i = 0; i < localVerts.Length; i++)
            worldVerts[i] = sourceTransform.TransformPoint(localVerts[i]);

        var tris = sourceMesh.triangles;

        // Calculate world-space bounds
        var bounds = new Bounds(worldVerts[0], Vector3.zero);
        for (var i = 1; i < worldVerts.Length; i++)
            bounds.Encapsulate(worldVerts[i]);

        var voxelSize = 1f / voxelsPerUnit;
        var voxelCountX = Mathf.CeilToInt(bounds.size.x / voxelSize) + 2;
        var voxelCountY = Mathf.CeilToInt(bounds.size.y / voxelSize) + 2;
        var voxelCountZ = Mathf.CeilToInt(bounds.size.z / voxelSize) + 2;

        sizeInChunks = new Vector3Int(
            Mathf.CeilToInt((float)voxelCountX / VoxelChunk.ChunkSize),
            Mathf.CeilToInt((float)voxelCountY / VoxelChunk.ChunkSize),
            Mathf.CeilToInt((float)voxelCountZ / VoxelChunk.ChunkSize));

        var totalSamples = sizeInChunks.x * sizeInChunks.y * sizeInChunks.z
                           * VoxelChunk.SampleSize * VoxelChunk.SampleSize * VoxelChunk.SampleSize;
        var totalTriTests = (long)totalSamples * (tris.Length / 3);

        Debug.Log($"VoxelTerrain: Voxelizing model — {sizeInChunks.x * VoxelChunk.ChunkSize}x" +
                  $"{sizeInChunks.y * VoxelChunk.ChunkSize}x{sizeInChunks.z * VoxelChunk.ChunkSize} voxels, " +
                  $"{sizeInChunks.x * sizeInChunks.y * sizeInChunks.z} chunks, " +
                  $"{tris.Length / 3} triangles");

        if (totalTriTests > 500_000_000)
        {
            Debug.LogWarning($"VoxelTerrain: ~{totalTriTests / 1_000_000}M ray-triangle tests. " +
                             "Consider lowering voxelsPerUnit or model scale to avoid long load times.");
        }

        var gridOrigin = bounds.min - new Vector3(voxelSize, voxelSize, voxelSize);
        transform.position = gridOrigin;
        transform.localScale = Vector3.one * voxelSize;

        for (var cx = 0; cx < sizeInChunks.x; cx++)
        for (var cy = 0; cy < sizeInChunks.y; cy++)
        for (var cz = 0; cz < sizeInChunks.z; cz++)
            CreateChunk(new Vector3Int(cx, cy, cz));

        // Hide the source model while we voxelize
        sourceModel.SetActive(false);

        // Process one chunk per frame to avoid freezing
        var chunksProcessed = 0;
        var totalChunks = _chunks.Count;

        foreach (var (chunkCoordinates, chunk) in _chunks)
        {
            for (var z = 0; z < VoxelChunk.SampleSize; z++)
            for (var y = 0; y < VoxelChunk.SampleSize; y++)
            for (var x = 0; x < VoxelChunk.SampleSize; x++)
            {
                var worldPos = new Vector3(
                    gridOrigin.x + (chunkCoordinates.x * VoxelChunk.ChunkSize + x) * voxelSize,
                    gridOrigin.y + (chunkCoordinates.y * VoxelChunk.ChunkSize + y) * voxelSize,
                    gridOrigin.z + (chunkCoordinates.z * VoxelChunk.ChunkSize + z) * voxelSize);

                // Quick bounds check — points outside the mesh AABB are definitely outside
                if (!bounds.Contains(worldPos))
                {
                    chunk.SetDensity(x, y, z, 0f);
                    continue;
                }

                var inside = IsInsideMesh(worldPos, worldVerts, tris);
                chunk.SetDensity(x, y, z, inside ? 1f : 0f);
            }

            chunk.Rebuild();
            chunksProcessed++;
            Debug.Log($"VoxelTerrain: Chunk {chunksProcessed}/{totalChunks} done");
            yield return null;
        }

        Destroy(sourceModel);
        _isGenerating = false;
        Debug.Log("VoxelTerrain: Voxelization complete");
    }

    private static bool IsInsideMesh(Vector3 point, Vector3[] verts, int[] tris)
    {
        var crossings = 0;

        for (var i = 0; i < tris.Length; i += 3)
        {
            var v0 = verts[tris[i]];
            var v1 = verts[tris[i + 1]];
            var v2 = verts[tris[i + 2]];

            if (RayHitsTriangle(point, v0, v1, v2))
                crossings++;
        }

        return crossings % 2 == 1;
    }

    /// <summary>
    /// Möller–Trumbore ray-triangle intersection.
    /// Ray origin = point, ray direction = +X.
    /// </summary>
    private static bool RayHitsTriangle(Vector3 origin, Vector3 v0, Vector3 v1, Vector3 v2)
    {
        var edge1 = v1 - v0;
        var edge2 = v2 - v0;

        // h = rayDir x edge2, rayDir is (1,0,0) so cross product simplifies
        var hy = -edge2.z;
        var hz = edge2.y;

        var a = edge1.y * hy + edge1.z * hz;

        if (a > -0.00001f && a < 0.00001f)
            return false;

        var f = 1f / a;
        var s = origin - v0;
        var u = f * (s.y * hy + s.z * hz);

        if (u < 0f || u > 1f)
            return false;

        var qx = s.y * edge1.z - s.z * edge1.y;
        var qy = s.z * edge1.x - s.x * edge1.z;
        var qz = s.x * edge1.y - s.y * edge1.x;

        var v = f * qx;

        if (v < 0f || u + v > 1f)
            return false;

        var t = f * (edge2.x * qx + edge2.y * qy + edge2.z * qz);

        return t > 0.00001f;
    }

    private void CreateChunk(Vector3Int chunkCoordinates)
    {
        var go = new GameObject($"Chunk ({chunkCoordinates.x}, {chunkCoordinates.y}, {chunkCoordinates.z})");
        go.transform.SetParent(transform, false);
        go.transform.localPosition = new Vector3(
            chunkCoordinates.x * VoxelChunk.ChunkSize,
            chunkCoordinates.y * VoxelChunk.ChunkSize,
            chunkCoordinates.z * VoxelChunk.ChunkSize);

        go.AddComponent<MeshFilter>();
        go.AddComponent<MeshRenderer>();
        go.AddComponent<MeshCollider>();

        var chunk = go.AddComponent<VoxelChunk>();
        chunk.Initialize(chunkCoordinates, terrainMaterial);

        _chunks[chunkCoordinates] = chunk;
    }

    public void Mine(Vector3 worldPoint)
    {
        if (_isGenerating) return;

        var localPoint = transform.InverseTransformPoint(worldPoint);

        var minX = Mathf.FloorToInt(localPoint.x - mineRadius);
        var maxX = Mathf.CeilToInt(localPoint.x + mineRadius);
        var minY = Mathf.FloorToInt(localPoint.y - mineRadius);
        var maxY = Mathf.CeilToInt(localPoint.y + mineRadius);
        var minZ = Mathf.FloorToInt(localPoint.z - mineRadius);
        var maxZ = Mathf.CeilToInt(localPoint.z + mineRadius);

        var dirtyChunks = new HashSet<Vector3Int>();
        var radiusSqr = mineRadius * mineRadius;

        for (var z = minZ; z <= maxZ; z++)
        for (var y = minY; y <= maxY; y++)
        for (var x = minX; x <= maxX; x++)
        {
            var dx = x - localPoint.x;
            var dy = y - localPoint.y;
            var dz = z - localPoint.z;
            var distSqr = dx * dx + dy * dy + dz * dz;

            if (distSqr > radiusSqr)
                continue;

            var chunkX = Mathf.FloorToInt((float)x / VoxelChunk.ChunkSize);
            var chunkY = Mathf.FloorToInt((float)y / VoxelChunk.ChunkSize);
            var chunkZ = Mathf.FloorToInt((float)z / VoxelChunk.ChunkSize);
            var chunkCoord = new Vector3Int(chunkX, chunkY, chunkZ);

            if (!_chunks.TryGetValue(chunkCoord, out var chunk))
                continue;

            var localX = x - chunkX * VoxelChunk.ChunkSize;
            var localY = y - chunkY * VoxelChunk.ChunkSize;
            var localZ = z - chunkZ * VoxelChunk.ChunkSize;

            if (localX < 0 || localX >= VoxelChunk.SampleSize ||
                localY < 0 || localY >= VoxelChunk.SampleSize ||
                localZ < 0 || localZ >= VoxelChunk.SampleSize)
                continue;

            chunk.SetDensity(localX, localY, localZ, 0f);

            dirtyChunks.Add(chunkCoord);

            switch (localX)
            {
                case 0 when chunkX > 0:
                    UpdateNeighborBoundary(chunkX - 1, chunkY, chunkZ, VoxelChunk.ChunkSize, localY, localZ, dirtyChunks);
                    break;
                case VoxelChunk.ChunkSize when chunkX < sizeInChunks.x - 1:
                    UpdateNeighborBoundary(chunkX + 1, chunkY, chunkZ, 0, localY, localZ, dirtyChunks);
                    break;
            }

            switch (localY)
            {
                case 0 when chunkY > 0:
                    UpdateNeighborBoundary(chunkX, chunkY - 1, chunkZ, localX, VoxelChunk.ChunkSize, localZ, dirtyChunks);
                    break;
                case VoxelChunk.ChunkSize when chunkY < sizeInChunks.y - 1:
                    UpdateNeighborBoundary(chunkX, chunkY + 1, chunkZ, localX, 0, localZ, dirtyChunks);
                    break;
            }

            switch (localZ)
            {
                case 0 when chunkZ > 0:
                    UpdateNeighborBoundary(chunkX, chunkY, chunkZ - 1, localX, localY, VoxelChunk.ChunkSize, dirtyChunks);
                    break;
                case VoxelChunk.ChunkSize when chunkZ < sizeInChunks.z - 1:
                    UpdateNeighborBoundary(chunkX, chunkY, chunkZ + 1, localX, localY, 0, dirtyChunks);
                    break;
            }
        }

        foreach (var chunkCoordinates in dirtyChunks)
        {
            if (_chunks.TryGetValue(chunkCoordinates, out var chunk))
                chunk.MarkDirty();
        }
    }

    private void UpdateNeighborBoundary(int cx, int cy, int cz, int lx, int ly, int lz, HashSet<Vector3Int> dirtyChunks)
    {
        var neighborCoord = new Vector3Int(cx, cy, cz);
        if (_chunks.TryGetValue(neighborCoord, out var neighbor))
        {
            neighbor.SetDensity(lx, ly, lz, 0f);
            dirtyChunks.Add(neighborCoord);
        }
    }
}
