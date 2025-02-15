using UnityEngine;

public class ChunkData
{
    public const int chunkSize = 16;

    private const int MAX_CHUNK_DEPTH = -1; // TODO: Get this from WorldGenerator

    public const int seedX = 2000;
    public const int seedZ = 4000;


    public Vector3Int chunkIndex;
    public string[,,] blocks;

    public ChunkData(Vector3Int chunkIndex)
    {
        this.chunkIndex = chunkIndex;
        blocks = new string[chunkSize, chunkSize, chunkSize];
    }

    public void Generate()
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
                        blocks[x, y, z] = "bedrock";
                    }
                    else if (intWorldY == height)
                    {
                        blocks[x, y, z] = "grass";
                    }
                    else if (intWorldY < height - 3)
                    {
                        blocks[x, y, z] = "stone";
                    }
                    else if (intWorldY < height)
                    {
                        blocks[x, y, z] = "dirt";
                    }
                    else
                    {
                        blocks[x, y, z] = "air";
                    }
                }
            }
        }
    }

}
