using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A placeable solid voxel block that the player can mine into.
/// Drop this on an empty GameObject, set the size and material, and it generates
/// a solid cube made of marching-cubes chunks that can be carved with Mine().
/// </summary>
public class VoxelTerrain : MonoBehaviour
{
    public static VoxelTerrain Instance;

    [Header("Block Size (in chunks — each chunk is 16 voxels)")]
    [SerializeField] private Vector3Int sizeInChunks = new Vector3Int(2, 2, 2);

    [Header("Rendering")]
    [SerializeField] private Material terrainMaterial;

    private Dictionary<Vector3Int, VoxelChunk> _chunks = new Dictionary<Vector3Int, VoxelChunk>();

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
        GenerateBlock();
    }

    private void GenerateBlock()
    {
        for (var cx = 0; cx < sizeInChunks.x; cx++)
        for (var cy = 0; cy < sizeInChunks.y; cy++)
        for (var cz = 0; cz < sizeInChunks.z; cz++)
        {
            var chunkCoordinates = new Vector3Int(cx, cy, cz);
            CreateChunk(chunkCoordinates);
        }

        // Fill densities: solid inside, empty on outer boundary, so surfaces generate
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

                // Outer boundary stays at 0 so marching cubes generates a surface
                var isEdge = wx == 0 || wx >= totalX ||
                              wy == 0 || wy >= totalY ||
                              wz == 0 || wz >= totalZ;

                chunk.SetDensity(x, y, z, isEdge ? 0f : 1f);
            }
        }

        // Build all chunk meshes
        foreach (var chunk in _chunks.Values)
            chunk.Rebuild();
    }

    private void CreateChunk(Vector3Int chunkCoordinates)
    {
        var go = new GameObject($"Chunk ({chunkCoordinates.x}, {chunkCoordinates.y}, {chunkCoordinates.z})");
        go.transform.SetParent(transform);
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

    [Header("Mining")]
    [SerializeField] private float mineRadius = 1.5f;

    public void Mine(Vector3 worldPoint)
    {
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
                // Sync shared samples on chunk boundaries
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
