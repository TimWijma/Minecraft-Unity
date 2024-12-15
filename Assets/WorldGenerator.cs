using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    public int chunkSize = 16;
    public int renderDistance = 5;

    public Transform player;
    public GameObject chunkPrefab;

    private const int WORLD_DEPTH = -8;
    private const int WORLD_HEIGHT = 100;

    private Vector3 currentChunkCenter;
    private Dictionary<Vector3, Chunk> chunks = new();
    private Chunk currentChunk;
    private int chunksGenerated = 0;

    private Queue<Vector3> chunksToGenerate = new();
    private Coroutine activeGenerationCoroutine;

    void Start()
    {
        player.position = new Vector3(0, 40, 0); ;

        currentChunkCenter = GetChunkCenterForPosition(player.position);
        currentChunk = GenerateChunk(currentChunkCenter);
        Debug.Log($"Current chunk center: {currentChunk}");
        // currentChunk.GetComponent<ChunkBorder>().Highlight();
        GenerateChunksInRadius(renderDistance);
    }

    void Update()
    {
        if (player == null || currentChunk == null) return;

        Vector3 newChunkCenter = GetChunkCenterForPosition(player.position);

        if (newChunkCenter != currentChunkCenter)
        {
            Debug.Log($"Moving to new chunk center: {newChunkCenter}");

            if (activeGenerationCoroutine != null)
            {
                StopCoroutine(activeGenerationCoroutine);
                chunksToGenerate.Clear(); // Clear any pending chunks
            }

            currentChunkCenter = newChunkCenter;
            chunksGenerated = 0;
            GenerateChunksInRadius(renderDistance);
            Debug.Log($"Chunks generated: {chunksGenerated}");

            ClearChunks();
        }
    }

    Vector3 GetChunkCenterForPosition(Vector3 position)
    {
        float x = Mathf.Floor(position.x / chunkSize) * chunkSize + (chunkSize / 2f);
        float y = Mathf.Floor(position.y / chunkSize) * chunkSize + (chunkSize / 2f);
        float z = Mathf.Floor(position.z / chunkSize) * chunkSize + (chunkSize / 2f);

        return new Vector3(x, y, z);
    }

    public Chunk GetChunkAtPosition(Vector3 position)
    {
        Vector3 chunkCenter = GetChunkCenterForPosition(position);
        if (chunks.TryGetValue(chunkCenter, out Chunk chunk))
        {
            return chunk;
        }

        return null;
    }

    void ClearChunks()
    {
        float maxDistance = (renderDistance + 0.5f) * chunkSize;

        HashSet<Vector3> chunksToKeep = new();
        for (int z = -renderDistance; z <= renderDistance; z++)
        {
            for (int y = -renderDistance; y <= renderDistance; y++)
            {
                for (int x = -renderDistance; x <= renderDistance; x++)
                {
                    Vector3 offset = new(x * chunkSize, y * chunkSize, z * chunkSize);
                    Vector3 chunkCenter = currentChunkCenter + offset;

                    if (Vector3.Distance(chunkCenter, currentChunkCenter) <= maxDistance)
                    {
                        chunksToKeep.Add(chunkCenter);
                    }
                }
            }
        }

        foreach (var (chunkCenter, chunkObject) in chunks)
        {
            if (chunksToKeep.Contains(chunkCenter))
            {
                if (chunkObject != null)
                {
                    chunkObject.gameObject.SetActive(true);
                }
            }
            else
            {
                chunkObject.gameObject.SetActive(false);
            }
        }
    }

    void GenerateChunksInRadius(int radius)
    {
        Debug.Log($"Generating chunks");
        float maxDistance = (radius + 0.5f) * chunkSize;

        for (int z = -radius; z <= radius; z++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                for (int x = -radius; x <= radius; x++)
                {
                    Vector3 offset = new(x * chunkSize, y * chunkSize, z * chunkSize);
                    Vector3 chunkCenter = currentChunkCenter + offset;

                    if (chunkCenter.y < WORLD_DEPTH || chunkCenter.y > WORLD_HEIGHT) continue;

                    if (Vector3.Distance(chunkCenter, currentChunkCenter) <= maxDistance)
                    {
                        chunksToGenerate.Enqueue(chunkCenter);
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

    Chunk GenerateChunk(Vector3 chunkCenter)
    {
        if (chunkCenter.y < WORLD_DEPTH || chunkCenter.y > WORLD_HEIGHT) return null;

        if (chunks.ContainsKey(chunkCenter) && chunks[chunkCenter] != null)
        {
            chunks[chunkCenter].gameObject.SetActive(true);

            return chunks[chunkCenter];
        }

        GameObject chunkObject = Instantiate(chunkPrefab, chunkCenter, Quaternion.identity);
        Chunk chunk = chunkObject.GetComponent<Chunk>();
        chunk.InitializeChunk(chunkCenter);
        chunksGenerated++;
        chunks.Add(chunkCenter, chunk);

        return chunk;
    }
}
