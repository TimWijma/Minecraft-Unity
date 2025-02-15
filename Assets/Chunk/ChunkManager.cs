using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ChunkManager : MonoBehaviour
{
    public int chunkSize = 16;
    public int renderDistance = 5;

    public Transform player;
    public GameObject chunkPrefab;

    private const int MAX_CHUNK_DEPTH = -1;
    private const int MAX_CHUNK_HEIGHT = 5;

    private Vector3Int currentChunkIndex;
    // public Dictionary<Vector3Int, Chunk> chunks = new();
    public Dictionary<Vector3Int, Chunk> loadedChunks = new();
    // private int chunksGenerated = 0;

    // private const int seedX = 0;
    // private const int seedZ = 0;

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

                    if (!loadedChunks.ContainsKey(chunkIndex))
                    {
                        Chunk chunk = CreateChunk(chunkIndex);
                        loadedChunks[chunkIndex] = chunk;

                        yield return null;
                    }
                }
            }
        }

        yield return null;

        foreach (var chunk in loadedChunks.Values)
        {
            chunk.GenerateStructures(loadedChunks);
        }

        yield return null;

        foreach (var chunk in loadedChunks.Values)
        {
            chunk.GenerateMesh();
        }

        List<Vector3Int> chunksToRemove = new List<Vector3Int>();
        foreach (var chunkIndex in loadedChunks.Keys)
        {
            if (!newChunks.Contains(chunkIndex))
            {
                chunksToRemove.Add(chunkIndex);
            }
        }

        foreach (var chunkIndex in chunksToRemove)
        {
            loadedChunks.Remove(chunkIndex);
        }
    }

    public Chunk CreateChunk(Vector3Int chunkIndex, bool isNeighbor = false)
    {
        Vector3Int chunkPosition = chunkIndex * chunkSize;
        GameObject chunkObject = Instantiate(chunkPrefab, chunkPosition, Quaternion.identity);
        Chunk chunk = chunkObject.GetComponent<Chunk>();
        chunk.Initialize(chunkIndex, isNeighbor);

        return chunk;
    }

    // public void PlaceBlockGlobal(Vector3 worldPosition, string blockId, Vector3Int originalChunk)
    // {
    //     Vector3Int chunkIndex = GetChunkIndexForPosition(worldPosition);

    //     if (chunks.TryGetValue(chunkIndex, out Chunk chunk))
    //     {
    //         chunk.SetBlock(worldPosition, blockId, false);
    //     }
    //     else
    //     {
    //         Debug.LogWarning($"Chunk not found at {chunkIndex}. PlaceBlock called from chunk {originalChunk}");
    //     }
    // }
}
