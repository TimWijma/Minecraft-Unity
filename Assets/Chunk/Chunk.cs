using UnityEngine;

public class Chunk : MonoBehaviour
{
    public int chunkSize = 16;

    private BlockType[,,] blocks;
    private MeshFilter meshFilter;
    private MeshCollider meshCollider;
    private Vector3Int chunkIndex;

    private const int seedX = 2000;
    private const int seedZ = 4000;

    private const int WORLD_DEPTH = -8;

    private ChunkMeshBuilder meshBuilder;

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();
        meshBuilder = new ChunkMeshBuilder(chunkSize);
    }

    public void InitializeChunk(Vector3Int chunkIndex)
    {
        this.chunkIndex = chunkIndex;
        blocks = new BlockType[chunkSize, chunkSize, chunkSize];

        GenerateBlocks();
        GenerateMesh();
    }

    private void GenerateBlocks()
    {
        for (int z = 0; z < chunkSize; z++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                for (int x = 0; x < chunkSize; x++)
                {
                    float worldX = chunkIndex.x * chunkSize + x;
                    float worldY = chunkIndex.y * chunkSize + y;
                    float worldZ = chunkIndex.z * chunkSize + z;

                    int height = CalculateHeight(Mathf.FloorToInt(worldX), Mathf.FloorToInt(worldZ), seedX, seedZ);

                    int intWorldY = Mathf.FloorToInt(worldY);

                    if (intWorldY == WORLD_DEPTH) // World depth is center of the chunk, so we need to offset by half the chunk size
                    {
                        blocks[x, y, z] = BlockType.Bedrock;
                    }
                    else if (intWorldY < height)
                    {
                        blocks[x, y, z] = BlockType.Dirt;
                    }
                    else if (intWorldY == height)
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
    }

    private void GenerateMesh()
    {
        meshBuilder.UpdateBlocks(blocks);

        for (int z = 0; z < chunkSize; z++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                for (int x = 0; x < chunkSize; x++)
                {
                    if (blocks[x, y, z] == BlockType.Air) continue;

                    meshBuilder.AddCube(x, y, z);
                }
            }
        }

        Mesh mesh = meshBuilder.BuildMesh();
        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;
    }

    private int CalculateHeight(int worldX, int worldZ, int seedX, int seedZ)
    {
        float height = 0;
        float amplitude = 1;
        float frequency = 1;
        float maxAmplitude = 0;

        float heightScale = 64f;
        float noiseScale = 0.01f;
        float octaves = 4;

        for (int i = 0; i < octaves; i++)
        {
            float sampleX = (worldX + seedX) * noiseScale * frequency;
            float sampleZ = (worldZ + seedZ) * noiseScale * frequency;

            height += Mathf.PerlinNoise(sampleX, sampleZ) * amplitude;

            maxAmplitude += amplitude;
            amplitude *= 0.5f;
            frequency *= 2;
        }

        return Mathf.FloorToInt(height / maxAmplitude * heightScale);
    }

    public void EnableChunk()
    {
        gameObject.SetActive(true);
    }

    public void DisableChunk()
    {
        gameObject.SetActive(false);
    }

    public BlockType GetBlockType(Vector3 worldPosition)
    {
        int chunkX = Mathf.FloorToInt(worldPosition.x - (chunkIndex.x * chunkSize));
        int chunkY = Mathf.FloorToInt(worldPosition.y - (chunkIndex.y * chunkSize));
        int chunkZ = Mathf.FloorToInt(worldPosition.z - (chunkIndex.z * chunkSize));

        if (chunkX < 0 || chunkX >= chunkSize || chunkY < 0 || chunkY >= chunkSize || chunkZ < 0 || chunkZ >= chunkSize)
        {
            return BlockType.Air;
        }

        return blocks[chunkX, chunkY, chunkZ];
    }

    public void SetBlockType(Vector3 worldPosition, BlockType blockType)
    {
        int chunkX = Mathf.FloorToInt(worldPosition.x - (chunkIndex.x * chunkSize));
        int chunkY = Mathf.FloorToInt(worldPosition.y - (chunkIndex.y * chunkSize));
        int chunkZ = Mathf.FloorToInt(worldPosition.z - (chunkIndex.z * chunkSize));

        if (chunkX < 0 || chunkX >= chunkSize || chunkY < 0 || chunkY >= chunkSize || chunkZ < 0 || chunkZ >= chunkSize)
        {
            return;
        }

        blocks[chunkX, chunkY, chunkZ] = blockType;
        GenerateMesh();
    }
}
