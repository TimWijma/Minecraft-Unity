using UnityEngine;

public class TreeGenerator
{
    private WorldGenerator worldGenerator;
    private Chunk chunk;

    public TreeGenerator(WorldGenerator worldGenerator, Chunk chunk)
    {
        this.worldGenerator = worldGenerator;
        this.chunk = chunk;
    }

    public void GenerateTrees()
    {
        Vector3Int chunkIndex = chunk.chunkIndex;
        int chunkSize = chunk.chunkSize;

        for (int z = 0; z < chunk.chunkSize; z++)
        {
            for (int x = 0; x < chunk.chunkSize; x++)
            {
                float worldX = chunkIndex.x * chunkSize + x;
                float worldZ = chunkIndex.z * chunkSize + z;

                // Get surface height in world coordinates
                int surfaceHeight = TerrainHelper.CalculateHeight(
                    Mathf.FloorToInt(worldX),
                    Mathf.FloorToInt(worldZ),
                    Chunk.seedX,
                    Chunk.seedZ
                );

                // Convert to chunk-local coordinates
                // int localSurfaceY = surfaceHeight - (chunkIndex.y * chunkSize);
                int surfaceChunkY = Mathf.FloorToInt(surfaceHeight / chunkSize);

                // Check if surface is in this chunk
                if (chunkIndex.y == surfaceChunkY)
                {
                    int localY = surfaceHeight % chunkSize;

                    if (Random.Range(0, 100) < 5 && chunk.blocks[x, localY, z] == BlockType.Grass)
                    {
                        CreateTree(new Vector3Int(
                            Mathf.FloorToInt(worldX),
                            surfaceHeight + 1, // One block above surface
                            Mathf.FloorToInt(worldZ)
                        ));
                    }
                }
            }
        }
    }

    void CreateTree(Vector3Int position)
    {
        for (int z = 0; z < 2; z++)
        {
            for (int y = 0; y < 2; y++)
            {
                for (int x = 0; x < 2; x++)
                {
                    Vector3Int blockPosition = position + new Vector3Int(x, y, z);
                    // Vector3Int targetChunkIndex = worldGenerator.GetChunkIndexForPosition(blockPosition);

                    worldGenerator.PlaceBlockGlobal(position + new Vector3Int(x, y, z), BlockType.Stone);

                    // if (targetChunkIndex != chunk.chunkIndex)
                    // {
                    //     if (!worldGenerator.chunks.ContainsKey(targetChunkIndex))
                    //     {
                    //         worldGenerator.
                    //     }
                    // }
                    // else
                    // {
                    //     chunk.PlaceBlock(blockPosition, BlockType.Wood);
                    // }
                }
            }
        }
    }
}
