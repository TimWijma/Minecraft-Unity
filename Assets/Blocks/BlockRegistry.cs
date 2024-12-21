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
        blocks.Add(BlockType.Stone, new Block(true));
        blocks.Add(BlockType.Bedrock, new Block(true, false));
        blocks.Add(BlockType.Wood, new Block(true));
        blocks.Add(BlockType.Leaves, new Block(true));

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
        Block stone = blocks[BlockType.Stone];
        Block bedrock = blocks[BlockType.Bedrock];
        Block wood = blocks[BlockType.Wood];
        Block leaves = blocks[BlockType.Leaves];
        for (int i = 0; i < 6; i++)
        {
            grass.SetTextureCoords((Direction)i, new Vector2(0, 0));
            dirt.SetTextureCoords((Direction)i, new Vector2(2, 0));
            stone.SetTextureCoords((Direction)i, new Vector2(3, 0));
            bedrock.SetTextureCoords((Direction)i, new Vector2(4, 0));
            wood.SetTextureCoords((Direction)i, new Vector2(5, 0));
            leaves.SetTextureCoords((Direction)i, new Vector2(7, 0));
        }

        grass.SetTextureCoords(Direction.Top, new Vector2(1, 0));
        grass.SetTextureCoords(Direction.Bottom, new Vector2(2, 0));

        wood.SetTextureCoords(Direction.Top, new Vector2(6, 0));
    }
}
