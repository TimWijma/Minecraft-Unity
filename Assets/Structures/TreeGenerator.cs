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

                    if (Random.Range(0, 100) < 1 && chunk.blocks[x, localY, z] == "grass")
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
        for (int y = 3; y < 6; y++)
        {
            for (int x = -2; x <= 2; x++)
            {
                for (int z = -2; z <= 2; z++)
                {
                    var blockPosition = position + new Vector3Int(x, y, z);
                    var blockChunkIndex = worldGenerator.GetChunkIndexForPosition(blockPosition);

                    // if (blockChunkIndex != chunk.chunkIndex && !worldGenerator.chunks.ContainsKey(blockChunkIndex))
                    // {
                    //     worldGenerator.CreateChunk(blockChunkIndex, true);
                    // }

                    worldGenerator.PlaceBlockGlobal(blockPosition, "leaves", chunk.chunkIndex);
                    worldGenerator.loadedChunks[blockChunkIndex].meshGenerated = false;
                }
            }
        }
        

        // Trunk
        for (int y = 0; y < 5; y++)
        {
            var blockPosition = position + new Vector3Int(0, y, 0);
            var blockChunkIndex = worldGenerator.GetChunkIndexForPosition(blockPosition);

            // if (blockChunkIndex != chunk.chunkIndex && !worldGenerator.chunks.ContainsKey(blockChunkIndex))
            // {
            //     worldGenerator.CreateChunk(blockChunkIndex, true);
            // }

            worldGenerator.PlaceBlockGlobal(blockPosition, "wood", chunk.chunkIndex);
            worldGenerator.loadedChunks[blockChunkIndex].meshGenerated = false;
        }
    }
}
