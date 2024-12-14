using System.Collections.Generic;
using UnityEngine;

public static class BlockRegistry
{
    private static readonly Dictionary<BlockType, Block> blocks = new();

    static BlockRegistry()
    {
        blocks.Add(BlockType.Air, new Block(false));
        blocks.Add(BlockType.Dirt, new Block(true));
        blocks.Add(BlockType.Grass, new Block(true));
    }

    public static Block GetBlock(BlockType blockType)
    {
        return blocks[blockType];
    }

    public static void RegisterBlocks()
    {
        Block grass = blocks[BlockType.Grass];
        Block dirt = blocks[BlockType.Dirt];
        for (int i = 0; i < 6; i++)
        {
            grass.SetTextureCoords((Direction)i, new Vector2(0, 0));
            dirt.SetTextureCoords((Direction)i, new Vector2(1, 0));
        }
    }
}
