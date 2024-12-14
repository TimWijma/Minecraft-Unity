using System.Collections.Generic;
using UnityEngine;

public static class BlockRegistry
{
    private static readonly Dictionary<BlockType, Block> blocks = new();

    static BlockRegistry()
    {
        blocks.Add(BlockType.Air, new Block("textures/air"));
        blocks.Add(BlockType.Dirt, new Block("textures/dirt"));
        blocks.Add(BlockType.Grass, new Block("textures/grass"));
    }

    public static Block GetBlock(BlockType blockType)
    {
        return blocks[blockType];
    }
}
