using System.Collections;
using System.Collections.Generic;
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
    private Dictionary<Vector3Int, Chunk> chunks = new();
    private Chunk currentChunk;
    private int chunksGenerated = 0;

    private Queue<Vector3Int> chunksToGenerate = new();
    private Coroutine activeGenerationCoroutine;

    void Start()
    {
        player.position = new Vector3(0, 40, 0); ;

        currentChunkIndex = GetChunkIndexForPosition(player.position);
        currentChunk = GenerateChunk(currentChunkIndex);
        Debug.Log($"Current chunk center: {currentChunk}");
        // currentChunk.GetComponent<ChunkBorder>().Highlight();
        GenerateChunksInRadius(renderDistance);
    }

    void Update()
    {
        if (player == null || currentChunk == null) return;

        Vector3Int newChunkIndex = GetChunkIndexForPosition(player.position);

        if (newChunkIndex != currentChunkIndex)
        {
            Debug.Log($"Moving to new chunk center: {newChunkIndex}");

            if (activeGenerationCoroutine != null)
            {
                StopCoroutine(activeGenerationCoroutine);
                chunksToGenerate.Clear(); // Clear any pending chunks
            }

            currentChunkIndex = newChunkIndex;
            chunksGenerated = 0;
            GenerateChunksInRadius(renderDistance);
            Debug.Log($"Chunks generated: {chunksGenerated}");

            ClearChunks();
        }
    }

    Vector3Int GetChunkIndexForPosition(Vector3 position)
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

        foreach (var (chunkIndex, chunkObject) in chunks)
        {
            bool shouldKeep = chunksToKeep.Contains(chunkIndex);
            if (chunkObject != null)
            {
                chunkObject.gameObject.SetActive(shouldKeep);
            }
        }
    }

    void GenerateChunksInRadius(int radius)
    {
        Debug.Log($"Generating chunks");
        float maxDistance = radius + 1.5f;

        for (int z = -radius; z <= radius; z++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                for (int x = -radius; x <= radius; x++)
                {
                    Vector3Int chunkIndex = currentChunkIndex + new Vector3Int(x, y, z);

                    if (chunkIndex.y < MAX_CHUNK_DEPTH || chunkIndex.y > MAX_CHUNK_HEIGHT) continue;

                    if (Vector3Int.Distance(chunkIndex, currentChunkIndex) <= maxDistance)
                    {
                        chunksToGenerate.Enqueue(chunkIndex);
                    }
                }
            }
        }

        Debug.Log($"Chunks to generate: {chunksToGenerate.Count}");

        if (chunksToGenerate.Count > 0)
        {
            activeGenerationCoroutine = StartCoroutine(GenerateChunksCoroutine());
        }
    }

    private IEnumerator GenerateChunksCoroutine()
    {
        const float maxTimePerFrame = 1f / 60f;
        while (chunksToGenerate.Count > 0)
        {
            float startTime = Time.realtimeSinceStartup;
            while (chunksToGenerate.Count > 0)
            {
                if (Time.realtimeSinceStartup - startTime > maxTimePerFrame)
                {
                    yield return null;
                    startTime = Time.realtimeSinceStartup;
                }
                GenerateChunk(chunksToGenerate.Dequeue());
            }
            yield return null;
        }
        Debug.Log($"Finished generating chunks");
        activeGenerationCoroutine = null;
    }

    Chunk GenerateChunk(Vector3Int chunkIndex)
    {
        if (chunkIndex.y < MAX_CHUNK_DEPTH || chunkIndex.y > MAX_CHUNK_HEIGHT) return null;

        if (chunks.ContainsKey(chunkIndex) && chunks[chunkIndex] != null)
        {
            chunks[chunkIndex].gameObject.SetActive(true);

            return chunks[chunkIndex];
        }

        Vector3Int chunkPosition = chunkIndex * chunkSize;
        GameObject chunkObject = Instantiate(chunkPrefab, chunkPosition, Quaternion.identity);
        Chunk chunk = chunkObject.GetComponent<Chunk>();
        chunk.InitializeChunk(chunkIndex);
        chunksGenerated++;
        chunks.Add(chunkIndex, chunk);

        return chunk;
    }
}
