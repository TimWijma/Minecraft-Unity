using System.Collections;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    public int chunkSize = 16;
    public float heightScale = 10;
    public float noiseScale = 0.5f;
    public float densityThreshold = 0.5f;


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

        GenerateBlocks();
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

    private void GenerateBlocks()
    {
        for (int z = 0; z < chunkSize; z++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                for (int x = 0; x < chunkSize; x++)
                {
                    float localX = x - chunkSize / 2f;
                    float localY = y - chunkSize / 2f;
                    float localZ = z - chunkSize / 2f;

                    float worldX = chunkCenter.x + localX;
                    float worldY = chunkCenter.y + localY;
                    float worldZ = chunkCenter.z + localZ;

                    int height = Mathf.FloorToInt(
                        Mathf.PerlinNoise(
                            worldX * noiseScale,
                            worldZ * noiseScale
                        ) * heightScale
                    );

                    if (worldY < height)
                    {
                        blocks[x, y, z] = BlockType.Dirt;
                    }
                    else if (worldY == height)
                    {
                        blocks[x, y, z] = BlockType.Grass;
                    }
                    else
                    {
                        blocks[x, y, z] = BlockType.Air;
                    }
                }
            }
        }

        Debug.Log("Blocks generated");
    }

    private void GenerateMesh()
    {
        var meshBuilder = new ChunkMeshBuilder(blocks);

        for (int z = 0; z < chunkSize; z++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                for (int x = 0; x < chunkSize; x++)
                {
                    BlockType blockType = blocks[x, y, z];
                    if (blockType == BlockType.Air) continue;

                    // Block block = BlockRegistry.GetBlock(blockType);

                    meshBuilder.AddCube(x, y, z);
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
