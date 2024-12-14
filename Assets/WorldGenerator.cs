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

    void Start()
    {
        player.position = new Vector3(0, 10, 0);

        currentChunkCenter = GetChunkCenterForPosition(player.position);
        GenerateChunksInRadius(renderDistance);
        currentChunk = chunks[currentChunkCenter];
        currentChunk.GetComponent<ChunkBorder>().Highlight();
    }

    void Update()
    {
        if (player == null || currentChunk == null) return;

        Vector3 newChunkCenter = GetChunkCenterForPosition(player.position);

        if (Vector3.Distance(newChunkCenter, currentChunkCenter) > 0.1f)
        {
            Chunk oldChunk = currentChunk;

            if (oldChunk != null)
            {
                oldChunk.GetComponent<ChunkBorder>().Unhighlight();
            }
            currentChunkCenter = newChunkCenter;
            ClearChunks();
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
        List<Vector3> chunksToRemove = new();
        float maxDistance = (renderDistance + 0.5f) * chunkSize;

        foreach (var chunk in chunks)
        {
            Vector3 chunkCenter = chunk.Key;
            Chunk chunkObject = chunk.Value;

            if (chunkObject == null)
            {
                chunksToRemove.Add(chunkCenter);
                continue;
            }

            float distance =
                Mathf.Abs(chunkCenter.x - currentChunkCenter.x) +
                Mathf.Abs(chunkCenter.y - currentChunkCenter.y) +
                Mathf.Abs(chunkCenter.z - currentChunkCenter.z);

            if (distance > maxDistance)
            {
                chunksToRemove.Add(chunkCenter);
                if (chunkObject != null)
                {
                    chunkObject.gameObject.SetActive(false);
                }
            };
        }
    }

    void GenerateChunksInRadius(int radius)
    {
        HashSet<Vector3> newChunks = new();

        for (int z = -radius; z <= radius; z++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                for (int x = -radius; x <= radius; x++)
                {
                    Vector3 offset = new(x * chunkSize, y * chunkSize, z * chunkSize);
                    Vector3 chunkCenter = currentChunkCenter + offset;

                    if (
                        Mathf.Abs(x) <= radius ||
                        Mathf.Abs(y) <= radius ||
                        Mathf.Abs(z) <= radius
                    )
                    {
                        newChunks.Add(chunkCenter);
                        GenerateChunk(chunkCenter);
                    }
                }
            }
        }

        List<Vector3> chunksToRemove = new();
        foreach (var chunk in chunks)
        {
            if (!newChunks.Contains(chunk.Key))
            {
                chunksToRemove.Add(chunk.Key); ;
            }
        }

        foreach (var pos in chunksToRemove)
        {
            if (chunks.TryGetValue(pos, out Chunk chunk))
            {
                chunk.gameObject.SetActive(false);
            }
        }
    }

    void GenerateChunk(Vector3 chunkCenter)
    {
        if (chunks.ContainsKey(chunkCenter))
        {
            chunks[chunkCenter].gameObject.SetActive(true);
            return;
        };

        GameObject chunkObject = Instantiate(chunkPrefab, chunkCenter, Quaternion.identity);
        Chunk chunk = chunkObject.GetComponent<Chunk>();
        chunk.InitializeChunk(chunkCenter);
        chunksGenerated++;
        chunks.Add(chunkCenter, chunk);
    }
}
