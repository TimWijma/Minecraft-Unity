using System.Collections;
using System.Security.Cryptography;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    public int chunkSize = 16;
    public int heightScale = 2;
    public Material defaultMaterial;
    public Material highlightMaterial;

    private BlockType[,,] blocks;
    private MeshFilter meshFilter;
    private MeshCollider meshCollider;
    private MeshRenderer meshRenderer;
    private Vector3 chunkCenter;

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();
        meshRenderer = GetComponent<MeshRenderer>();
    }

    public void InitializeChunk(Vector3 chunkCenter)
    {
        this.chunkCenter = chunkCenter;
        blocks = new BlockType[chunkSize, chunkSize, chunkSize];
        InitalizeAir();

        gameObject.AddComponent<ChunkData>().Initialize(chunkSize);
        StartCoroutine(CreateChunkComponents());

        GenerateMesh();
    }

    private void InitalizeAir()
    {
        for (int x = 0; x < blocks.GetLength(0); x++)
        {
            for (int y = 0; y < blocks.GetLength(1); y++)
            {
                for (int z = 0; z < blocks.GetLength(2); z++)
                {
                    blocks[x, y, z] = BlockType.Air;
                }
            }
        }
    }

    private void GenerateMesh()
    {
        var meshBuilder = new ChunkMeshBuilder(chunkSize * chunkSize);

        for (int z = 0; z < chunkSize; z++)
        {
            for (int y = 0; y < chunkSize; y++)
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
        }

        Mesh mesh = meshBuilder.BuildMesh();

        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;
    }

    private IEnumerator CreateChunkComponents()
    {
        yield return new WaitForEndOfFrame();

        GetComponent<ChunkMarker>().CreateMarker();
        GetComponent<ChunkBorder>().CreateBorder();
    }

    public void Highlight()
    {
        meshRenderer.material = highlightMaterial;
    }

    public void Unhighlight()
    {
        meshRenderer.material = defaultMaterial;
    }
}
