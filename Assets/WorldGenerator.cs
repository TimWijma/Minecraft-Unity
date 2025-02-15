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

    private readonly Queue<Vector3Int> chunkLoadQueue = new();
    private bool isGeneratingChunks = false;

    private readonly Queue<GameObject> chunkPool = new();

    private readonly List<Vector3Int> chunksToRemove = new();
    private readonly List<Vector3Int> sortedChunkQueue = new();

    private readonly HashSet<Chunk> activeChunks = new();

    void Start()
    {
        InitializeChunkPool();

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
            Debug.Log($"Player moved to chunk {currentChunkIndex}");

            List<Chunk> chunksToUnload = new();
            foreach (var chunk in activeChunks)
            {
                if (!loadedChunks.ContainsKey(chunk.chunkIndex))
                {
                    chunksToUnload.Add(chunk);
                }
            }

            foreach (var chunk in chunksToUnload)
            {
                ReleaseChunkToPool(chunk);
            }

            if (isGeneratingChunks)
            {
                StopCoroutine(ProcessChunkQueue());
                isGeneratingChunks = false;
            }

            chunkLoadQueue.Clear();
            sortedChunkQueue.Clear();

            Debug.Log($"Total active chunks: {FindObjectsByType<Chunk>(FindObjectsSortMode.None).Length}");

            StartCoroutine(UpdateChunks());
        }
    }

    void InitializeChunkPool()
    {
        int
        initialPoolSize = (renderDistance * 2 + 1) * (renderDistance * 2 + 1) * (renderDistance * 2 + 1);

        for (int i = 0; i < initialPoolSize; i++)
        {
            AddChunkToPool();
        }
    }

    private GameObject GetChunkFromPool()
    {
        if (chunkPool.Count == 0)
        {
            AddChunkToPool();
            Debug.LogWarning("Chunk pool was empty. Instantiated and added a new chunk to the pool.");
        }

        return chunkPool.Dequeue();
    }

    private void AddChunkToPool()
    {
        GameObject chunkObject = Instantiate(chunkPrefab, Vector3.zero, Quaternion.identity);

        chunkObject.SetActive(false);
        chunkPool.Enqueue(chunkObject);
    }

    public void ReleaseChunkToPool(Chunk chunk)
    {
        if (chunk == null || chunk.gameObject == null)
        {
            Debug.LogWarning($"Tried to release null chunk {chunk.chunkIndex} to the pool.");
        }

        activeChunks.Remove(chunk);
        loadedChunks.Remove(chunk.chunkIndex);

        chunk.StopAllCoroutines();
        chunk.Reset();
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
        List<Vector3Int> newChunks = new();

        chunksToRemove.Clear();

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

        foreach (var chunkIndex in loadedChunks.Keys.ToList())
        {
            if (!newChunks.Contains(chunkIndex))
            {
                chunksToRemove.Add(chunkIndex);
            }
        }

        foreach (var chunkIndex in chunksToRemove)
        {
            if (loadedChunks.TryGetValue(chunkIndex, out Chunk chunk))
            {
                ReleaseChunkToPool(chunk);
            }
            else
            {
                Debug.LogWarning($"Trying to remove chunk {chunkIndex} that is not in loadedChunks!");
            }
        }

        yield return null;

        if (!isGeneratingChunks)
        {
            StartCoroutine(ProcessChunkQueue());
        }
    }

    IEnumerator ProcessChunkQueue()
    {
        isGeneratingChunks = true;

        while (chunksToRemove.Count > 0)
        {
            yield return null;
        }

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

            List<Task<ChunkData>> dataTasks = new();
            int chunksToProcess = Mathf.Min(sortedChunkQueue.Count, 4);

            for (int i = 0; i < chunksToProcess; i++)
            {
                Vector3Int chunkIndex = sortedChunkQueue[i];
                dataTasks.Add(Task.Run(() => GenerateChunkData(chunkIndex)));
            }

            sortedChunkQueue.RemoveRange(0, chunksToProcess);

            foreach (var chunkIndex in sortedChunkQueue)
            {
                chunkLoadQueue.Enqueue(chunkIndex);
            }

            yield return new WaitUntil(() => dataTasks.All(t => t.IsCompleted));

            List<Task<MeshData>> meshTasks = new();
            List<Chunk> newChunks = new();

            foreach (var task in dataTasks)
            {
                ChunkData chunkData = task.Result;
                Chunk chunk = InstantiateChunk(chunkData.chunkIndex);
                chunk.ApplyChunkData(chunkData);
                loadedChunks[chunkData.chunkIndex] = chunk;

                newChunks.Add(chunk);

                chunk.GenerateStructures(loadedChunks);

                meshTasks.Add(Task.Run(() => chunk.GenerateMeshData()));
            }

            yield return new WaitUntil(() => meshTasks.All(t => t.IsCompleted));

            for (int i = 0; i < newChunks.Count; i++)
            {
                newChunks[i].ApplyMeshData(meshTasks[i].Result);
                yield return null;
            }
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
        chunk.chunkIndex = chunkIndex;

        activeChunks.Add(chunk);

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

    private void OnDrawGizmos()
    {
        if (Application.isPlaying && sortedChunkQueue != null)
        {
            // Draw the current chunk index
            Gizmos.color = Color.red;
            Vector3 currentPos = new Vector3(currentChunkIndex.x, currentChunkIndex.y, currentChunkIndex.z) * chunkSize;
            Gizmos.DrawWireCube(currentPos + Vector3.one * chunkSize / 2, Vector3.one * chunkSize);

            // Draw the sorted queue with different colors
            for (int i = 0; i < sortedChunkQueue.Count; i++)
            {
                float t = (float)i / sortedChunkQueue.Count;
                Gizmos.color = Color.Lerp(Color.green, Color.yellow, t);

                Vector3 chunkPos = new Vector3(sortedChunkQueue[i].x, sortedChunkQueue[i].y, sortedChunkQueue[i].z) * chunkSize;
                Gizmos.DrawWireCube(chunkPos + Vector3.one * chunkSize / 2, Vector3.one * (chunkSize * 0.9f));
            }
        }

        if (Application.isPlaying)
        {
            foreach (var chunk in FindObjectsByType<Chunk>(FindObjectsSortMode.None))
            {
                if (!loadedChunks.ContainsKey(chunk.chunkIndex))
                {
                    Gizmos.color = Color.magenta;
                    Gizmos.DrawWireCube(chunk.transform.position + Vector3.one * chunkSize / 2, Vector3.one * chunkSize);
                }
            }
        }

    }

    public Chunk FindChunk(int x, int y, int z)
    {
        Vector3Int chunkIndex = new Vector3Int(x, y, z);
        return loadedChunks.ContainsKey(chunkIndex) ? loadedChunks[chunkIndex] : null;
    }
}
