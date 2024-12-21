using UnityEngine;

public class Chunk : MonoBehaviour
{
    public int chunkSize = 16;

    public BlockType[,,] blocks;
    private MeshFilter meshFilter;
    private MeshCollider meshCollider;
    public Vector3Int chunkIndex;

    public const int seedX = 2000;
    public const int seedZ = 4000;

    private const int MAX_CHUNK_DEPTH = -1; // TODO: Get this from WorldGenerator

    public bool blocksGenerated = false;
    public bool structuresGenerated = false;
    public bool meshGenerated = false;

    private ChunkMeshBuilder meshBuilder;

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();
        meshBuilder = new ChunkMeshBuilder(chunkSize);
    }

    public void Initialize(Vector3Int chunkIndex, bool isNeighbor = false)
    {
        this.chunkIndex = chunkIndex;
        blocks = new BlockType[chunkSize, chunkSize, chunkSize];

        GenerateBlocks(isNeighbor);
        blocksGenerated = true;
    }

    private void GenerateBlocks(bool isNeighbor = false)
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

                    int height = TerrainHelper.CalculateHeight(Mathf.FloorToInt(worldX), Mathf.FloorToInt(worldZ), seedX, seedZ);

                    int intWorldY = Mathf.FloorToInt(worldY);

                    if (chunkIndex.y == MAX_CHUNK_DEPTH && y == 0)
                    {
                        blocks[x, y, z] = BlockType.Bedrock;
                    }
                    else if (intWorldY == height)
                    {
                        // blocks[x, y, z] = isNeighbor ? BlockType.Bedrock : BlockType.Grass;
                        blocks[x, y, z] = BlockType.Grass;
                    }
                    else if (intWorldY < height - 3)
                    {
                        blocks[x, y, z] = BlockType.Stone;
                    }
                    else if (intWorldY < height)
                    {
                        blocks[x, y, z] = BlockType.Dirt;
                    }
                    else
                    {
                        blocks[x, y, z] = BlockType.Air;
                    }
                }
            }
        }
    }

    public void GenerateMesh()
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
        // GenerateMesh();
    }
}
