using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    public int chunkSize = 16;
    public int renderDistance = 5;

    public Transform player;
    public GameObject chunkPrefab;

    private const int MAX_CHUNK_DEPTH = -1;
    private const int MAX_CHUNK_HEIGHT = 5;

    private Vector3Int currentChunkIndex;
    public Dictionary<Vector3Int, Chunk> chunks = new();
    private int chunksGenerated = 0;

    private const int seedX = 0;
    private const int seedZ = 0;

    void Start()
    {
        player.position = new Vector3(0, 100, 0);

        currentChunkIndex = GetChunkIndexForPosition(player.position);

        StartCoroutine(CreateChunksInRadius(renderDistance));
    }

    void Update()
    {
        if (player == null) return;

        Vector3Int newChunkIndex = GetChunkIndexForPosition(player.position);
        if (newChunkIndex != currentChunkIndex)
        {
            Debug.Log($"Moving to new chunk center: {newChunkIndex}");

            currentChunkIndex = newChunkIndex;
            chunksGenerated = 0;

            ClearChunks();

            // CreateChunksInRadius(renderDistance);
            StartCoroutine(CreateChunksInRadius(renderDistance));
        }
    }

    public Vector3Int GetChunkIndexForPosition(Vector3 position)
    {
        int x = Mathf.FloorToInt(position.x / chunkSize);
        int y = Mathf.FloorToInt(position.y / chunkSize);
        int z = Mathf.FloorToInt(position.z / chunkSize);

        return new Vector3Int(x, y, z);
    }

    public Chunk GetChunkAtPosition(Vector3 position)
    {
        Vector3Int chunkIndex = GetChunkIndexForPosition(position);
        if (chunks.TryGetValue(chunkIndex, out Chunk chunk))
        {
            return chunk;
        }

        return null;
    }

    Chunk[] GetChunksInRenderDistance(int radius)
    {
        return chunks.Values.Where(chunk =>
            Vector3Int.Distance(chunk.chunkIndex, currentChunkIndex) <= radius
        ).ToArray();
    }

    void ClearChunks()
    {
        float maxDistance = renderDistance + 1.5f;

        HashSet<Vector3Int> chunksToKeep = new();
        for (int z = -renderDistance; z <= renderDistance; z++)
        {
            for (int y = -renderDistance; y <= renderDistance; y++)
            {
                for (int x = -renderDistance; x <= renderDistance; x++)
                {
                    Vector3Int chunkIndex = currentChunkIndex + new Vector3Int(x, y, z);
                    if (Vector3Int.Distance(chunkIndex, currentChunkIndex) <= maxDistance)
                    {
                        chunksToKeep.Add(chunkIndex);
                    }
                }
            }
        }

        var chunksToRemove = chunks.Keys.Except(chunksToKeep).ToList();
        foreach (var chunkIndex in chunksToRemove)
        {
            if (chunks.TryGetValue(chunkIndex, out Chunk chunk))
            {
                Destroy(chunk.gameObject);
            }
            chunks.Remove(chunkIndex);
        }
    }

    IEnumerator CreateChunksInRadius(int radius)
    {
        yield return StartCoroutine(InitializeChunks(radius));
        yield return StartCoroutine(GenerateStructures(radius));
        yield return StartCoroutine(GenerateMeshes(radius));
    }

    IEnumerator InitializeChunks(int radius)
    {
        for (int z = -radius; z <= radius; z++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                for (int x = -radius; x <= radius; x++)
                {
                    Vector3Int chunkIndex = currentChunkIndex + new Vector3Int(x, y, z);

                    if (chunkIndex.y < MAX_CHUNK_DEPTH || chunkIndex.y > MAX_CHUNK_HEIGHT) continue;
                    if (Vector3Int.Distance(chunkIndex, currentChunkIndex) > radius) continue;
                    if (chunks.ContainsKey(chunkIndex)) continue;

                    CreateChunk(chunkIndex);
                    Debug.Log($"Generated chunk {chunkIndex}");

                    yield return null;
                }
            }
        }
    }

    void CreateChunk(Vector3Int chunkIndex, bool isNeighbor = false)
    {
        Vector3Int chunkPosition = chunkIndex * chunkSize;
        GameObject chunkObject = Instantiate(chunkPrefab, chunkPosition, Quaternion.identity);
        Chunk chunk = chunkObject.GetComponent<Chunk>();
        chunk.Initialize(chunkIndex, isNeighbor);
        chunksGenerated++;
        chunks.Add(chunkIndex, chunk);
    }

    IEnumerator GenerateStructures(int radius)
    {
        foreach (var chunk in GetChunksInRenderDistance(radius))
        {
            if (chunk.structuresGenerated) continue;

            Vector3Int chunkIndex = chunk.chunkIndex;

            Debug.Log($"Generating structures for chunk {chunkIndex}");

            EnsureNeighborsExist(chunkIndex);

            TreeGenerator treeGenerator = new(this, chunk);
            treeGenerator.GenerateTrees();

            chunk.structuresGenerated = true;
        }

        yield return null;
    }

    private void EnsureNeighborsExist(Vector3Int chunkIndex)
    {
        Vector3Int[] neighbourChunks = new Vector3Int[]
        {
            chunkIndex + Direction.Left.ToVector3Int(),
            chunkIndex + Direction.Right.ToVector3Int(),
            chunkIndex + Direction.Front.ToVector3Int(),
            chunkIndex + Direction.Back.ToVector3Int(),
            chunkIndex + Direction.Top.ToVector3Int(),
            chunkIndex + Direction.Bottom.ToVector3Int()
        };

        foreach (Vector3Int neighbourChunkIndex in neighbourChunks)
        {
            // Skip if out of world bounds
            if (neighbourChunkIndex.y < MAX_CHUNK_DEPTH || neighbourChunkIndex.y > MAX_CHUNK_HEIGHT) continue;

            // Ensure the neighbor chunk exists
            if (!chunks.ContainsKey(neighbourChunkIndex))
            {
                CreateChunk(neighbourChunkIndex, isNeighbor: true);

                Debug.Log($"Generated neighbor chunk {neighbourChunkIndex}");
            }
        }
    }

    IEnumerator GenerateMeshes(int radius)
    {
        foreach (var chunk in GetChunksInRenderDistance(radius))
        {
            if (chunk == null || chunk.gameObject == null) continue; // Skip destroyed chunks
            if (chunk.meshGenerated) continue;

            chunk.GenerateMesh();

            Debug.Log($"Generated mesh for chunk {chunk.chunkIndex}");

            chunk.meshGenerated = true;

            yield return null;
        }
    }


    public void PlaceBlockGlobal(Vector3 worldPosition, BlockType blockType)
    {
        Vector3Int chunkIndex = GetChunkIndexForPosition(worldPosition);

        if (chunks.TryGetValue(chunkIndex, out Chunk chunk))
        {
            chunk.SetBlockType(worldPosition, blockType);
        }
    }
}
