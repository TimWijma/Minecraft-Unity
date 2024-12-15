using UnityEngine;

public class Block
{
    public bool hasTexture;
    public bool isBreakable = true;
    private Vector2[] textureCoords;

    public Block(bool hasTexture, bool isBreakable = true)
    {
        this.hasTexture = hasTexture;
        this.isBreakable = isBreakable;
        textureCoords = new Vector2[6];
    }

    public void SetTextureCoords(Direction direction, Vector2 coord)
    {
        textureCoords[(int)direction] = coord;
    }

    public Vector2 GetTextureCoords(Direction direction)
    {
        return textureCoords[(int)direction];
    }
}
