using System;
using UnityEngine;

public class ChunkMeshBuilder
{
    private Vector3[] vertices;
    private int[] triangles;
    private Vector2[] uvs;

    private int currentVertIndex = 0;
    private int currentTriIndex = 0;

    private BlockType[,,] blocks;

    public ChunkMeshBuilder(BlockType[,,] blocks)
    {
        int maxCubes = blocks.GetLength(0) * blocks.GetLength(1) * blocks.GetLength(2);

        vertices = new Vector3[maxCubes * 8];
        triangles = new int[maxCubes * 36];
        uvs = new Vector2[maxCubes * 8];

        this.blocks = blocks;
    }

    public bool IsTransparent(int x, int y, int z)
    {
        if (x < 0 || x >= blocks.GetLength(0) ||
            y < 0 || y >= blocks.GetLength(1) ||
            z < 0 || z >= blocks.GetLength(2))
        {
            return true;
        }

        return blocks[x, y, z] == BlockType.Air;
    }

    private Vector3[] GetFaceVertices(Direction direction, int x, int y, int z)
    {
        Vector3[] faceVertices = new Vector3[4];

        switch (direction)
        {
            case Direction.Left:
                faceVertices[0] = new Vector3(x, y, z + 1);
                faceVertices[1] = new Vector3(x, y + 1, z + 1);
                faceVertices[2] = new Vector3(x, y + 1, z);
                faceVertices[3] = new Vector3(x, y, z);
                break;
            case Direction.Right:
                faceVertices[0] = new Vector3(x + 1, y, z);
                faceVertices[1] = new Vector3(x + 1, y + 1, z);
                faceVertices[2] = new Vector3(x + 1, y + 1, z + 1);
                faceVertices[3] = new Vector3(x + 1, y, z + 1);
                break;
            case Direction.Front:
                faceVertices[0] = new Vector3(x, y, z);
                faceVertices[1] = new Vector3(x, y + 1, z);
                faceVertices[2] = new Vector3(x + 1, y + 1, z);
                faceVertices[3] = new Vector3(x + 1, y, z);
                break;
            case Direction.Back:
                faceVertices[0] = new Vector3(x + 1, y, z + 1);
                faceVertices[1] = new Vector3(x + 1, y + 1, z + 1);
                faceVertices[2] = new Vector3(x, y + 1, z + 1);
                faceVertices[3] = new Vector3(x, y, z + 1);
                break;
            case Direction.Top:
                faceVertices[0] = new Vector3(x, y + 1, z);
                faceVertices[1] = new Vector3(x, y + 1, z + 1);
                faceVertices[2] = new Vector3(x + 1, y + 1, z + 1);
                faceVertices[3] = new Vector3(x + 1, y + 1, z);
                break;
            case Direction.Bottom:
                faceVertices[0] = new Vector3(x, y, z);
                faceVertices[1] = new Vector3(x, y, z + 1);
                faceVertices[2] = new Vector3(x + 1, y, z + 1);
                faceVertices[3] = new Vector3(x + 1, y, z);
                break;
        }

        return faceVertices;
    }

    private Vector2[] GetFaceUVs(Block block, Direction direction)
    {
        Vector2[] faceUVs = new Vector2[4];

        if (block.hasTexture)
        {
            int tilesPerAxis = 16;
            float tileSize = 1f / tilesPerAxis;
            Vector2 tileCoords = block.GetTextureCoords(direction);

            float x = tileCoords.x * tileSize;
            float y = (tilesPerAxis - tileCoords.y - 1) * tileSize;

            faceUVs[0] = new Vector2(x, y);
            faceUVs[1] = new Vector2(x, y + tileSize);
            faceUVs[2] = new Vector2(x + tileSize, y + tileSize);
            faceUVs[3] = new Vector2(x + tileSize, y);
        }
        else
        {
            faceUVs[0] = new Vector2(0, 0);
            faceUVs[1] = new Vector2(0, 1);
            faceUVs[2] = new Vector2(1, 1);
            faceUVs[3] = new Vector2(1, 0);
        }

        return faceUVs;
    }

    public void AddFace(Direction direction, int x, int y, int z)
    {
        Block block = BlockRegistry.GetBlock(blocks[x, y, z]);

        Vector3[] faceVertices = GetFaceVertices(direction, x, y, z);
        Vector2[] faceUVs = GetFaceUVs(block, direction);

        for (int i = 0; i < 4; i++)
        {
            vertices[currentVertIndex + i] = faceVertices[i];
            uvs[currentVertIndex + i] = faceUVs[i];
        }

        triangles[currentTriIndex] = currentVertIndex;
        triangles[currentTriIndex + 1] = currentVertIndex + 1;
        triangles[currentTriIndex + 2] = currentVertIndex + 2;
        triangles[currentTriIndex + 3] = currentVertIndex;
        triangles[currentTriIndex + 4] = currentVertIndex + 2;
        triangles[currentTriIndex + 5] = currentVertIndex + 3;

        currentVertIndex += 4;
        currentTriIndex += 6;
    }

    public void AddCube(int x, int y, int z)
    {
        if (IsTransparent(x, y, z)) return;

        if (IsTransparent(x - 1, y, z)) AddFace(Direction.Left, x, y, z);
        if (IsTransparent(x + 1, y, z)) AddFace(Direction.Right, x, y, z);
        if (IsTransparent(x, y, z - 1)) AddFace(Direction.Front, x, y, z);
        if (IsTransparent(x, y, z + 1)) AddFace(Direction.Back, x, y, z);
        if (IsTransparent(x, y + 1, z)) AddFace(Direction.Top, x, y, z);
        if (IsTransparent(x, y - 1, z)) AddFace(Direction.Bottom, x, y, z);
    }

    public Mesh BuildMesh()
    {
        Mesh mesh = new();
        Array.Resize(ref vertices, currentVertIndex);
        Array.Resize(ref triangles, currentTriIndex);
        Array.Resize(ref uvs, currentVertIndex);

        int offset = blocks.GetLength(0) / 2;
        Vector3 offsetVector = new(offset, offset, offset);
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] -= offsetVector;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }
}
