using System.Collections.Generic;
using UnityEngine;

public class InventoryItem
{
    public Item item;
    public int count;
}

public class PlayerInventory : MonoBehaviour
{
    public List<InventoryItem> items = new List<InventoryItem>();
    public int currentIndex;

    void Start()
    {
        for (int i = 0; i < 10; i++)
        {
            items.Add(null);
        }
        Debug.Log("items initialized");
    }

    public void SelectItem(int index)
    {
        Debug.Log("Index:" + index);
        currentIndex = (index - 1) % 10;
        
    }

    public void OpenInventory()
    {
        foreach (var item in items)
        {
            if (item == null) continue;
            Debug.Log(item?.item.id + " x" + item.count);
        }
    }

    public void AddItem(string itemId)
    {
        Item item = ItemRegistry.Instance.items[itemId];
        if (item != null) {
            AddItem(item);
        }
    }

    void AddItem(Item item)
    {
        var existingItem = items.FindAll(x => x?.item == item);

        bool added = false;
        for (int i = 0; i < existingItem.Count; i++)
        {
            if (existingItem[i].count < item.maxStackSize)
            {
                existingItem[i].count++;
                added = true;
                return;
            }
        }
        if (!added)
        {
            var nullItemIndex = items.FindIndex(i => i == null);
            if (nullItemIndex != -1)
            {
                items[nullItemIndex] = new InventoryItem { item = item, count = 1 };
            }
            else
            {
                items.Add(new InventoryItem { item = item, count = 1 });
            }
        }
    }

    public string PlaceItem()
    {
        InventoryItem currentItem = items[currentIndex];

        if (currentItem == null || !currentItem.item.isPlaceable) return null;

        currentItem.count--;

        if (currentItem.count == 0) items[currentIndex] = null;

        return currentItem.item.id;
    }
}