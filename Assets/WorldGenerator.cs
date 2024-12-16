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

    private Queue<Vector3Int> chunksToGenerateBlocks = new();
    private Queue<Vector3Int> chunksToGenerateStructures = new();
    private Queue<Vector3Int> chunksToGenerateMeshes = new();

    private HashSet<Vector3Int> waitingForNeighbors = new();

    private Coroutine blockGenerationCoroutine;
    private Coroutine structureGenerationCoroutine;
    private Coroutine meshGenerationCoroutine;

    void Start()
    {
        player.position = new Vector3(0, 100, 0);

        currentChunkIndex = GetChunkIndexForPosition(player.position);

        CreateChunksInRadius(renderDistance);
        // activeGenerationCoroutine = StartCoroutine(MeshGenerationSequence());
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

            CreateChunksInRadius(renderDistance);
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

    void CreateChunksInRadius(int radius)
    {
        chunksToGenerateBlocks.Clear();
        chunksToGenerateStructures.Clear();
        chunksToGenerateMeshes.Clear();

        float maxDistance = radius + 1.5f;

        for (int z = -radius; z <= radius; z++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                for (int x = -radius; x <= radius; x++)
                {
                    Vector3Int chunkIndex = currentChunkIndex + new Vector3Int(x, y, z);

                    if (chunkIndex.y < MAX_CHUNK_DEPTH || chunkIndex.y > MAX_CHUNK_HEIGHT) continue;
                    if (Vector3Int.Distance(chunkIndex, currentChunkIndex) > maxDistance) continue;

                    chunksToGenerateBlocks.Enqueue(chunkIndex);
                    chunksToGenerateStructures.Enqueue(chunkIndex);
                }
            }
        }

        if (blockGenerationCoroutine == null)
        {
            blockGenerationCoroutine = StartCoroutine(BlockGenerationSequence());
        }

        StartCoroutine(GenerationSequence());
    }

    void RenderChunksInRadius(int radius)
    {
        float maxDistance = radius + 1.5f;

        for (int z = -radius; z <= radius; z++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                for (int x = -radius; x <= radius; x++)
                {
                    Vector3Int chunkIndex = currentChunkIndex + new Vector3Int(x, y, z);

                    if (chunkIndex.y < MAX_CHUNK_DEPTH || chunkIndex.y > MAX_CHUNK_HEIGHT) continue;
                    if (Vector3Int.Distance(chunkIndex, currentChunkIndex) > maxDistance) continue;

                    if (!chunks.ContainsKey(chunkIndex) || chunks[chunkIndex] == null) continue;

                    chunksToGenerateMeshes.Enqueue(chunkIndex);
                }
            }
        }

        if (meshGenerationCoroutine == null)
        {
            meshGenerationCoroutine = StartCoroutine(MeshGenerationSequence());
        }
    }

    void GenerateBlocks(Vector3Int chunkIndex, bool isNeighbor = false)
    {
        if (chunks.ContainsKey(chunkIndex)) return;

        Vector3Int chunkPosition = chunkIndex * chunkSize;
        GameObject chunkObject = Instantiate(chunkPrefab, chunkPosition, Quaternion.identity);
        Chunk chunk = chunkObject.GetComponent<Chunk>();
        chunk.Initialize(chunkIndex, isNeighbor);
        chunksGenerated++;
        chunks.Add(chunkIndex, chunk);
    }

    private IEnumerator BlockGenerationSequence()
    {
        const float maxTimePerFrame = 1f / 60f;

        // Generate blocks
        while (chunksToGenerateBlocks.Count > 0)
        {
            float startTime = Time.realtimeSinceStartup;
            while (chunksToGenerateBlocks.Count > 0)
            {
                if (Time.realtimeSinceStartup - startTime > maxTimePerFrame)
                {
                    yield return null;
                    startTime = Time.realtimeSinceStartup;
                }
                Vector3Int chunkIndex = chunksToGenerateBlocks.Dequeue();
                GenerateBlocks(chunkIndex);
            }
            yield return null;
        }

        blockGenerationCoroutine = null;
    }

    private IEnumerator GenerationSequence()
    {
        while (chunksToGenerateBlocks.Count > 0)
        {
            yield return null;
        }

        structureGenerationCoroutine = StartCoroutine(StructureGenerationSequence());

        while (chunksToGenerateStructures.Count > 0 || waitingForNeighbors.Count > 0)
        {
            yield return null;
        }

        RenderChunksInRadius(renderDistance);
    }

    private IEnumerator StructureGenerationSequence()
    {
        const float maxTimePerFrame = 1f / 60f;

        // Generate structures
        while (chunksToGenerateStructures.Count > 0)
        {
            float startTime = Time.realtimeSinceStartup;

            if (waitingForNeighbors.Count > 0)
            {
                var waitingChunks = waitingForNeighbors.ToArray();
                foreach (Vector3Int chunkIndex in waitingChunks)
                {
                    if (EnsureNeighborsExist(chunkIndex))
                    {
                        waitingForNeighbors.Remove(chunkIndex);
                        GenerateStructures(chunkIndex);
                    }
                }
            }

            while (chunksToGenerateStructures.Count > 0)
            {
                if (Time.realtimeSinceStartup - startTime > maxTimePerFrame)
                {
                    yield return null;
                    startTime = Time.realtimeSinceStartup;
                }
                Vector3Int chunkIndex = chunksToGenerateStructures.Dequeue();

                if (EnsureNeighborsExist(chunkIndex))
                {
                    GenerateStructures(chunkIndex);
                }
            }
            yield return null;
        }

        structureGenerationCoroutine = null;
    }

    void GenerateMesh(Vector3Int chunkIndex)
    {
        Chunk chunk = chunks[chunkIndex];
        chunk.GenerateMesh();
    }

    private IEnumerator MeshGenerationSequence()
    {
        const float maxTimePerFrame = 1f / 60f;

        // Generate meshes
        while (chunksToGenerateMeshes.Count > 0)
        {
            float startTime = Time.realtimeSinceStartup;
            while (chunksToGenerateMeshes.Count > 0)
            {
                if (Time.realtimeSinceStartup - startTime > maxTimePerFrame)
                {
                    yield return null;
                    startTime = Time.realtimeSinceStartup;
                }
                Vector3Int chunkIndex = chunksToGenerateMeshes.Dequeue();
                GenerateMesh(chunkIndex);
            }
            yield return null;
        }

        meshGenerationCoroutine = null;
    }

    private bool EnsureNeighborsExist(Vector3Int chunkIndex)
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

        bool allNeighborsExist = true;
        foreach (Vector3Int neighbourChunkIndex in neighbourChunks)
        {
            // Skip if out of world bounds
            if (neighbourChunkIndex.y < MAX_CHUNK_DEPTH || neighbourChunkIndex.y > MAX_CHUNK_HEIGHT) continue;

            // Ensure the neighbor chunk exists
            if (!chunks.ContainsKey(neighbourChunkIndex) ||
                chunks[neighbourChunkIndex] == null ||
                !chunks[neighbourChunkIndex].blocksGenerated)
            {
                if (!chunks.ContainsKey(neighbourChunkIndex))
                {
                    chunksToGenerateBlocks.Enqueue(neighbourChunkIndex);
                }

                waitingForNeighbors.Add(chunkIndex);
                allNeighborsExist = false;
            }
        }

        return allNeighborsExist;
    }

    private void GenerateStructures(Vector3Int chunkIndex)
    {
        if (!chunks.TryGetValue(chunkIndex, out Chunk chunk)) return;

        if (chunk.structuresGenerated) return;

        if (!chunk.blocksGenerated) return;

        int seedX = chunkIndex.x * chunkSize;
        int seedZ = chunkIndex.z * chunkSize;

        TreeGenerator treeGenerator = new(this, chunk);
        treeGenerator.GenerateTrees(seedX, seedZ);

        chunk.structuresGenerated = true;
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
