using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshExample : MonoBehaviour
{
    public int chunkSize = 16;
    public int renderDistance = 3;
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
        // HighlightCurrentChunk();
    }

    void Update()
    {
        Vector3 newChunkCenter = GetChunkCenterForPosition(player.position);

        // Only generate a new chunk if the player has moved to a different chunk
        if (Vector3.Distance(newChunkCenter, currentChunkCenter) > 0.01f)
        {
            // UnhighlightCurrentChunk();
            currentChunkCenter = newChunkCenter;
            ClearChunks();
            GenerateChunksInRadius(renderDistance);
            currentChunk = chunks[currentChunkCenter];
            // HighlightCurrentChunk();
        }
    }

    Vector3 GetChunkCenterForPosition(Vector3 position)
    {
        // Calculate chunk center based on chunk size and vertex spacing
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

        Mesh mesh = new();
        meshFilter.mesh = mesh;

        int baseVertCount = (chunkSize + 1) * (chunkSize + 1);
        Vector3[] vertices = new Vector3[baseVertCount];

        int[] triangles = new int[chunkSize * chunkSize * 6];

        for (int z = 0, i = 0; z <= chunkSize; z++)
        {
            for (int x = 0; x <= chunkSize; x++, i++)
            {
                float localX = x - chunkSize / 2f;
                float localZ = z - chunkSize / 2f;

                float worldX = localX + chunkCenter.x;
                float worldZ = localZ + chunkCenter.z;

                float y = Mathf.PerlinNoise(worldX * 0.2f, worldZ * 0.2f) * heightScale;

                vertices[i] = new Vector3(localX, y, localZ);
            }
        }

        int vert = 0;
        int tris = 0;
        for (int z = 0; z < chunkSize; z++)
        {
            for (int x = 0; x < chunkSize; x++)
            {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + chunkSize + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + chunkSize + 1;
                triangles[tris + 5] = vert + chunkSize + 2;

                vert++;
                tris += 6;
            }
            vert++;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

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
