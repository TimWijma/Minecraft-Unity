using System;

[Serializable]
public class Texture
{
    public string top;
    public string bottom;
    public string side;
}

[Serializable]
public class Block
{
    public string id;
    public bool hasTexture;
    public bool isBreakable = true;
    public string dropItem;
    public Texture texture;
}
