using System;
using UnityEngine;

public class ChunkMeshBuilder
{
    private Vector3[] vertices;
    private int[] triangles;
    private Vector2[] uvs;
    private Vector3[] normals;

    private readonly int maxVertices;
    private readonly int maxTriangles;


    private int currentVertIndex = 0;
    private int currentTriIndex = 0;

    private string[,,] blocks;

    public ChunkMeshBuilder(int chunkSize)
    {
        int maxCubes = chunkSize * chunkSize * chunkSize;

        maxVertices = maxCubes * 8;
        maxTriangles = maxCubes * 36;

        vertices = new Vector3[maxVertices];
        triangles = new int[maxTriangles];
        uvs = new Vector2[maxVertices];
        normals = new Vector3[maxVertices];
    }

    public void UpdateBlocks(string[,,] blocks)
    {
        this.blocks = blocks;
        currentTriIndex = 0;
        currentVertIndex = 0;

        vertices = new Vector3[maxVertices];
        triangles = new int[maxTriangles];
        uvs = new Vector2[maxVertices];
        normals = new Vector3[maxVertices];
    }

    public bool IsTransparent(int x, int y, int z)
    {
        if (x < 0 || x >= blocks.GetLength(0) ||
            y < 0 || y >= blocks.GetLength(1) ||
            z < 0 || z >= blocks.GetLength(2))
        {
            return true;
        }

        return blocks[x, y, z] == "air";
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
                faceVertices[1] = new Vector3(x + 1, y, z);
                faceVertices[2] = new Vector3(x + 1, y, z + 1);
                faceVertices[3] = new Vector3(x, y, z + 1);
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
            Vector2 tileCoords = BlockRegistry.Instance.GetBlockTexture(block.id, direction);

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

        Block block = BlockRegistry.Instance.GetBlock(blocks[x, y, z]);

        Vector3[] faceVertices = GetFaceVertices(direction, x, y, z);
        Vector2[] faceUVs = GetFaceUVs(block, direction);

        Vector3 normal = direction switch
        {
            Direction.Left => Vector3.left,
            Direction.Right => Vector3.right,
            Direction.Front => Vector3.forward,
            Direction.Back => Vector3.back,
            Direction.Top => Vector3.up,
            Direction.Bottom => Vector3.down,
            _ => Vector3.zero
        };

        for (int i = 0; i < 4; i++)
        {
            vertices[currentVertIndex + i] = faceVertices[i];
            uvs[currentVertIndex + i] = faceUVs[i];
            normals[currentVertIndex + i] = normal;
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
        Array.Resize(ref normals, currentVertIndex);

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.normals = normals;

        mesh.RecalculateBounds();

        return mesh;
    }
}
