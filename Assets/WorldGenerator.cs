using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

    private Queue<GameObject> chunkPool = new();
    public int initialPoolSize = 50;

    private HashSet<Vector3Int> newChunks = new();
    private List<Vector3Int> chunksToRemove = new();

    private List<Vector3Int> sortedChunkQueue = new();

    void Start()
    {
        InitializeChunkPool(initialPoolSize);

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
            // LoadPlayerChunkFirst();
            StartCoroutine(UpdateChunks());
        }
    }

    void InitializeChunkPool(int size)
    {
        for (int i = 0; i < size; i++)
        {
            GameObject chunkObject = Instantiate(chunkPrefab, Vector3.zero, Quaternion.identity);
            chunkObject.SetActive(false);
            chunkPool.Enqueue(chunkObject);
        }
    }

    private GameObject GetChunkFromPool()
    {
        if (chunkPool.Count > 0)
        {
            return chunkPool.Dequeue();
        }
        else
        {
            GameObject chunkObject = Instantiate(chunkPrefab, Vector3.zero, Quaternion.identity);
            return chunkObject;
        }
    }

    public void ReleaseChunkToPool(Chunk chunk)
    {
        chunk.gameObject.SetActive(false);
        chunkPool.Enqueue(chunk.gameObject);
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
        newChunks.Clear();

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

        chunksToRemove.Clear();

        foreach (var chunkIndex in loadedChunks.Keys)
        {
            if (!newChunks.Contains(chunkIndex))
            {
                chunksToRemove.Add(chunkIndex);
            }
        }

        foreach (var chunkIndex in chunksToRemove)
        {
            Chunk chunk = loadedChunks[chunkIndex];
            ReleaseChunkToPool(chunk);
            loadedChunks.Remove(chunkIndex);
        }

        yield return null;
    }

    private void LoadPlayerChunkFirst()
    {
        Vector3Int playerChunkIndex = GetChunkIndexForPosition(player.position);
        if (!loadedChunks.ContainsKey(playerChunkIndex) && !chunkLoadQueue.Contains(playerChunkIndex))
        {
            ChunkData chunkData = GenerateChunkData(playerChunkIndex);
            Chunk chunk = InstantiateChunk(playerChunkIndex);
            chunk.ApplyData(chunkData);
            loadedChunks[playerChunkIndex] = chunk;
            chunk.GenerateStructures(loadedChunks);
            chunk.GenerateMesh();
        }
    }

    IEnumerator ProcessChunkQueue()
    {
        isGeneratingChunks = true;

        while (chunkLoadQueue.Count > 0)
        {
            sortedChunkQueue.Clear();
            sortedChunkQueue.AddRange(chunkLoadQueue);
            chunkLoadQueue.Clear();

            sortedChunkQueue.Sort((a, b) =>
            {
                float distanceA = Vector3.Distance(a, currentChunkIndex);
                float distanceB = Vector3.Distance(b, currentChunkIndex);
                return distanceA.CompareTo(distanceB);
            });

            foreach (var chunkIndex in sortedChunkQueue)
            {
                Debug.Log($"Player position: {currentChunkIndex}, Chunk index: {chunkIndex}");
                chunkLoadQueue.Enqueue(chunkIndex);
            }

            List<Task<ChunkData>> tasks = new();
            int chunksToProcess = Math.Min(chunkLoadQueue.Count, 4);

            for (int i = 0; i < chunksToProcess; i++)
            {
                Vector3Int chunkIndex = chunkLoadQueue.Dequeue();
                tasks.Add(Task.Run(() => GenerateChunkData(chunkIndex)));
            }

            yield return new WaitUntil(() => tasks.All(t => t.IsCompleted));

            foreach (var task in tasks)
            {
                ChunkData chunkData = task.Result;
                Chunk chunk = InstantiateChunk(chunkData.chunkIndex);
                chunk.ApplyData(chunkData);
                loadedChunks[chunkData.chunkIndex] = chunk;
            }

            yield return null;
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

    private ChunkData GenerateChunkData(Vector3Int chunkIndex)
    {
        ChunkData chunkData = new(chunkIndex);
        chunkData.Generate();

        return chunkData;
    }

    public Chunk InstantiateChunk(Vector3Int chunkIndex)
    {
        Vector3Int chunkPosition = chunkIndex * chunkSize;
        GameObject chunkObject = GetChunkFromPool();
        chunkObject.transform.position = chunkPosition;

        Chunk chunk = chunkObject.GetComponent<Chunk>();
        chunk.Reset();
        chunk.chunkIndex = chunkIndex;

        chunkObject.SetActive(true);
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
