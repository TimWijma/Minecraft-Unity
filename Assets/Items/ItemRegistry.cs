using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

public class ItemRegistry : MonoBehaviour
{
    public static ItemRegistry Instance;

    public Dictionary<string, Item> items = new();
    private Dictionary<string, Vector2Int> itemTextures = new();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("BlockRegistry initialized");
            // InitializeBlockTextures();
            InitializeItems();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // public void InitializeBlockTextures()
    // {
    //     string json = File.ReadAllText("Assets/Blocks/block_textures.json");
    //     itemTextures = JsonConvert.DeserializeObject<Dictionary<string, Vector2Int>>(json);

    //     foreach (var blockTexture in itemTextures)
    //     {
    //         Debug.Log($"Loaded block texture: {blockTexture.Key} {blockTexture.Value}");
    //     }
    // }

    public void InitializeItems()
    {
        string json = File.ReadAllText("Assets/Items/items.json");
        Item[] itemsArray = JsonConvert.DeserializeObject<Item[]>(json);
        foreach (var item in itemsArray)
        {
            items[item.id] = item;
            Debug.Log($"Loaded block: {item.id}");
        }
    }

    public Item GetItem(string itemId)
    {
        return items[itemId];
    }
}
