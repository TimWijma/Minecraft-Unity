using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

public class BlockRegistry : MonoBehaviour
{
    public static BlockRegistry Instance;

    public Dictionary<string, Block> blocks = new();
    private Dictionary<string, Vector2Int> blockTextures = new();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("BlockRegistry initialized");
            InitializeBlockTextures();
            InitializeBlocks();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void InitializeBlockTextures()
    {
        string json = File.ReadAllText("Assets/Blocks/block_textures.json");
        blockTextures = JsonConvert.DeserializeObject<Dictionary<string, Vector2Int>>(json);

        foreach (var blockTexture in blockTextures)
        {
            Debug.Log($"Loaded block texture: {blockTexture.Key} {blockTexture.Value}");
        }
    }

    public void InitializeBlocks()
    {
        string json = File.ReadAllText("Assets/Blocks/blocks.json");
        Block[] blocksArray = JsonConvert.DeserializeObject<Block[]>(json);
        foreach (var block in blocksArray)
        {
            blocks[block.id] = block;
            Debug.Log($"Loaded block: {block.id}");
        }
    }

    public Vector2Int GetBlockTexture(string blockId, Direction direction)
    {
        Block block = GetBlock(blockId);

        Vector2Int texture = direction switch
        {
            Direction.Top => blockTextures[block.texture.top],
            Direction.Bottom => blockTextures[block.texture.bottom],
            Direction.Left => blockTextures[block.texture.side],
            Direction.Right => blockTextures[block.texture.side],
            Direction.Front => blockTextures[block.texture.side],
            Direction.Back => blockTextures[block.texture.side],
            _ => new Vector2Int(0, 0),
        };

        if (texture == null)
        {
            Debug.LogError($"Texture not found for block {blockId} and direction {direction}");
        }

        return texture;
    }

    public Block GetBlock(string blockId)
    {
        return blocks[blockId];
    }
}
