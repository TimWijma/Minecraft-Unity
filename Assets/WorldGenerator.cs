using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MeshGenerator : MonoBehaviour
{
    public int chunkSize = 16;
    public int renderDistance = 5;

    public Transform player;
    public GameObject chunkPrefab;

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

        if (Vector3.Distance(newChunkCenter, currentChunkCenter) > 0.1f)
        {
            Debug.Log($"Moving to new chunk center: {newChunkCenter}");

            Chunk oldChunk = currentChunk;
            if (oldChunk != null)
            {
                oldChunk.GetComponent<ChunkBorder>().Unhighlight();
            }

            // if (activeGenerationCoroutine != null)
            // {
            //     StopCoroutine(activeGenerationCoroutine);
            //     chunksToGenerate.Clear(); // Clear any pending chunks
            // }

            currentChunkCenter = newChunkCenter;
            chunksGenerated = 0;
            GenerateChunksInRadius(renderDistance);
            Debug.Log($"Chunks generated: {chunksGenerated}");

            if (chunks.TryGetValue(currentChunkCenter, out currentChunk))
            {
                currentChunk.GetComponent<ChunkBorder>().Highlight();
            }
            else
            {
                Debug.LogError($"Current chunk not found at {currentChunkCenter}");
            }


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

    void ClearChunks()
    {
        float maxDistance = (renderDistance + 0.5f) * chunkSize;

        foreach (var (chunkCenter, chunkObject) in chunks)
        {
            float distance = Vector3.Distance(chunkCenter, currentChunkCenter);

            if (distance > maxDistance)
            {
                if (chunkObject != null)
                {
                    chunkObject.gameObject.SetActive(false);
                }
            };
        }
    }

    void GenerateChunksInRadius(int radius)
    {
        Debug.Log($"Generating chunks");

        for (int z = -radius; z <= radius; z++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                for (int x = -radius; x <= radius; x++)
                {
                    Vector3 offset = new(x * chunkSize, y * chunkSize, z * chunkSize);
                    Vector3 chunkCenter = currentChunkCenter + offset;

                    if (
                        Mathf.Abs(x) <= radius &&
                        Mathf.Abs(y) <= radius &&
                        Mathf.Abs(z) <= radius
                    )
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
        while (chunksToGenerate.Count > 0)
        {
            GenerateChunk(chunksToGenerate.Dequeue());
            yield return null;
        }
        Debug.Log($"Finished generating chunks");
        activeGenerationCoroutine = null;
    }

    Chunk GenerateChunk(Vector3 chunkCenter)
    {
        if (chunks.ContainsKey(chunkCenter))
        {
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
