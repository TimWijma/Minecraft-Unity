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
        blocks.Add(BlockType.Bedrock, new Block(true, false));

        RegisterBlocks();
    }

    public static Block GetBlock(BlockType blockType)
    {
        return blocks[blockType];
    }

    public static void RegisterBlocks()
    {
        Block grass = blocks[BlockType.Grass];
        Block dirt = blocks[BlockType.Dirt];
        Block bedrock = blocks[BlockType.Bedrock];
        for (int i = 0; i < 6; i++)
        {
            grass.SetTextureCoords((Direction)i, new Vector2(0, 0));
            dirt.SetTextureCoords((Direction)i, new Vector2(2, 0));
            bedrock.SetTextureCoords((Direction)i, new Vector2(3, 0));
        }

        grass.SetTextureCoords(Direction.Top, new Vector2(1, 0));
    }
}
