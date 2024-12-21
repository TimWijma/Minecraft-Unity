public class Item
{
    public string name;
    public int maxStackSize;
    public bool isPlaceable;


    public Item(string name, int maxStackSize, bool isPlaceable)
    {
        this.name = name;
        this.maxStackSize = maxStackSize;
        this.isPlaceable = isPlaceable;
    }
}
