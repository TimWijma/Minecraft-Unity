using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    public int chunkSize = 16;
    public int renderDistance = 1;

    public Transform player;
    public GameObject chunkPrefab;

    private Vector3Int currentChunkIndex;
    public Dictionary<Vector3Int, Chunk> loadedChunks = new();

    private Queue<Vector3Int> chunkLoadQueue = new();
    private bool isGeneratingChunks = false;

    void Start()
    {
        player.position = new Vector3(0, 100, 0);
        currentChunkIndex = GetChunkIndexForPosition(player.position);
        StartCoroutine(UpdateChunks());
    }

    void Update()
    {
        Vector3Int newChunkIndex = GetChunkIndexForPosition(player.position);
        if (newChunkIndex != currentChunkIndex)
        {
            currentChunkIndex = newChunkIndex;
            StartCoroutine(UpdateChunks());
        }
    }

    public Vector3Int GetChunkIndexForPosition(Vector3 position)
    {
        int x = Mathf.FloorToInt(position.x / chunkSize);
        int y = Mathf.FloorToInt(position.y / chunkSize);
        int z = Mathf.FloorToInt(position.z / chunkSize);

        return new Vector3Int(x, y, z);
    }

    IEnumerator UpdateChunks()
    {
        HashSet<Vector3Int> newChunks = new();

        for (int x = -renderDistance; x <= renderDistance; x++)
        {
            for (int y = -renderDistance; y <= renderDistance; y++)
            {
                for (int z = -renderDistance; z <= renderDistance; z++)
                {
                    Vector3Int chunkIndex = currentChunkIndex + new Vector3Int(x, y, z);
                    newChunks.Add(chunkIndex);

                    if (!loadedChunks.ContainsKey(chunkIndex) && !chunkLoadQueue.Contains(chunkIndex))
                    {
                        chunkLoadQueue.Enqueue(chunkIndex);
                    }
                }
            }
        }

        if (!isGeneratingChunks)
        {
            StartCoroutine(ProcessChunkQueue());
        }

        List<Vector3Int> chunksToRemove = loadedChunks.Keys
            .Where(chunkIndex => !newChunks.Contains(chunkIndex))
            .ToList();

        foreach (var chunkIndex in chunksToRemove)
        {
            loadedChunks.Remove(chunkIndex);
        }

        yield return null;
    }

    IEnumerator ProcessChunkQueue()
    {
        isGeneratingChunks = true;

        while (chunkLoadQueue.Count > 0)
        {
            Vector3Int chunkIndex = chunkLoadQueue.Dequeue();
            if (!loadedChunks.ContainsKey(chunkIndex))
            {
                Chunk chunk = CreateChunk(chunkIndex);
                loadedChunks[chunkIndex] = chunk;
                yield return null;
            }
        }

        List<Chunk> chunkList = new(loadedChunks.Values);
        foreach (var chunk in chunkList)
        {
            chunk.GenerateStructures(loadedChunks);
            yield return null;
        }

        foreach (var chunk in chunkList)
        {
            chunk.GenerateMesh();
            yield return null;
        }

        isGeneratingChunks = false;
    }

    public Chunk CreateChunk(Vector3Int chunkIndex, bool isNeighbor = false)
    {
        Vector3Int chunkPosition = chunkIndex * chunkSize;
        GameObject chunkObject = Instantiate(chunkPrefab, chunkPosition, Quaternion.identity);
        Chunk chunk = chunkObject.GetComponent<Chunk>();
        chunk.Initialize(chunkIndex, isNeighbor);

        return chunk;
    }



    public Chunk GetChunkAtPosition(Vector3 position)
    {
        Vector3Int chunkIndex = GetChunkIndexForPosition(position);
        if (loadedChunks.TryGetValue(chunkIndex, out Chunk chunk))
        {
            return chunk;
        }

        return null;
    }

    public void PlaceBlockGlobal(Vector3 worldPosition, string blockId, Vector3Int originalChunk)
    {
        Vector3Int chunkIndex = GetChunkIndexForPosition(worldPosition);

        if (loadedChunks.TryGetValue(chunkIndex, out Chunk chunk))
        {
            chunk.SetBlock(worldPosition, blockId, false);
        }
        else
        {
            Debug.LogWarning($"Chunk not found at {chunkIndex}. PlaceBlock called from chunk {originalChunk}");
        }
    }
}
