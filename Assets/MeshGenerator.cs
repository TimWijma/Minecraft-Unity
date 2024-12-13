using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator : MonoBehaviour
{
    public int chunkSize = 16;
    public int renderDistance = 5;
    public int heightScale = 2;

    public Transform player;
    public GameObject chunkPrefab;
    public Material defaultMaterial;
    public Material highlightMaterial;

    private Vector3 currentChunkCenter;
    private Dictionary<Vector3, GameObject> chunks = new();
    private GameObject currentChunk;

    void Start()
    {
        player.position = new Vector3(0, 10, 0);

        currentChunkCenter = GetChunkCenterForPosition(player.position);
        GenerateChunk(currentChunkCenter);
        currentChunk = chunks[currentChunkCenter];
        HighlightCurrentChunk();
    }

    void Update()
    {
        Vector3 newChunkCenter = GetChunkCenterForPosition(player.position);

        if (Vector3.Distance(newChunkCenter, currentChunkCenter) > 0.01f)
        {
            UnhighlightCurrentChunk();
            currentChunkCenter = newChunkCenter;
            ClearChunks();
            GenerateChunksInRadius(renderDistance);
            currentChunk = chunks[currentChunkCenter];
            HighlightCurrentChunk();
        }
    }

    Vector3 GetChunkCenterForPosition(Vector3 position)
    {
        float x = Mathf.Floor(position.x / chunkSize) * chunkSize + (chunkSize / 2f);
        float z = Mathf.Floor(position.z / chunkSize) * chunkSize + (chunkSize / 2f);

        return new Vector3(x, 0, z);
    }

    void ClearChunks()
    {
        List<Vector3> chunksToRemove = new();

        foreach (var chunk in chunks)
        {
            Vector3 chunkCenter = chunk.Key;
            GameObject chunkObject = chunk.Value;

            if (Vector3.Distance(chunkCenter, currentChunkCenter) > renderDistance * chunkSize)
            {
                chunksToRemove.Add(chunkCenter);
                Destroy(chunkObject);
            }
        }

        foreach (var chunk in chunksToRemove)
        {
            chunks.Remove(chunk);

        }
    }

    void GenerateChunksInRadius(int radius)
    {
        for (int z = -radius; z <= radius; z++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                Vector3 offset = new(x * chunkSize, 0, z * chunkSize);
                Vector3 chunkCenter = currentChunkCenter + offset;
                GenerateChunk(chunkCenter);
            }
        }
    }

    void GenerateChunk(Vector3 chunkCenter)
    {
        if (chunks.ContainsKey(chunkCenter)) return;

        GameObject chunkObject = Instantiate(chunkPrefab, chunkCenter, Quaternion.identity);
        chunks.Add(chunkCenter, chunkObject);

        chunkObject.AddComponent<ChunkData>().Initialize(chunkSize);
        StartCoroutine(CreateChunkComponents(chunkObject));

        MeshFilter meshFilter = chunkObject.GetComponent<MeshFilter>();
        MeshCollider meshCollider = chunkObject.GetComponent<MeshCollider>();

        var meshBuilder = new ChunkMeshBuilder(chunkSize * chunkSize);

        for (int z = 0; z < chunkSize; z++)
        {
            for (int x = 0; x < chunkSize; x++)
            {
                float localX = x - chunkSize / 2f;
                float localZ = z - chunkSize / 2f;

                float worldX = chunkCenter.x + localX;
                float worldZ = chunkCenter.z + localZ;

                int y = Mathf.FloorToInt(Mathf.PerlinNoise(worldX / 10f, worldZ / 10f) * heightScale);
                meshBuilder.AddCube(new Vector3(localX, y, localZ));
            }
        }

        Mesh mesh = meshBuilder.BuildMesh();

        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;
    }

    private IEnumerator CreateChunkComponents(GameObject chunkObject)
    {
        yield return new WaitForEndOfFrame();

        chunkObject.GetComponent<ChunkMarker>().CreateMarker();
        chunkObject.GetComponent<ChunkBorder>().CreateBorder();
    }

    void HighlightCurrentChunk()
    {
        if (currentChunk != null)
        {
            MeshRenderer renderer = currentChunk.GetComponent<MeshRenderer>();
            renderer.material = highlightMaterial;
        }
    }

    void UnhighlightCurrentChunk()
    {
        if (currentChunk != null)
        {
            MeshRenderer renderer = currentChunk.GetComponent<MeshRenderer>();
            renderer.material = defaultMaterial;
        }
    }
}
