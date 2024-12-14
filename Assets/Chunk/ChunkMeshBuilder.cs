using System;
using System.Collections.Generic;
using UnityEngine;

public class ChunkMeshBuilder
{
    private Vector3[] vertices;
    private int[] triangles;

    private int currentVertIndex = 0;
    private int currentTriIndex = 0;

    private Dictionary<Vector3, int> vertexIndexMap = new();

    private BlockType[,,] blocks;

    public ChunkMeshBuilder(BlockType[,,] blocks)
    {
        int maxCubes = blocks.GetLength(0) * blocks.GetLength(1) * blocks.GetLength(2);

        vertices = new Vector3[maxCubes * 8];
        triangles = new int[maxCubes * 36];

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

    public void AddFace(Direction direction, int x, int y, int z)
    {
        Vector3[] faceVertices = GetFaceVertices(direction, x, y, z);

        for (int i = 0; i < 4; i++)
        {
            // If the vertex is not in the map, add it to the map and increment the currentVertIndex
            if (!vertexIndexMap.ContainsKey(faceVertices[i]))
            {
                vertices[currentVertIndex] = faceVertices[i];
                vertexIndexMap[faceVertices[i]] = currentVertIndex;
                currentVertIndex++;
            }
        }

        int[] faceTriangles = new int[]
        {
            vertexIndexMap[faceVertices[0]],
            vertexIndexMap[faceVertices[1]],
            vertexIndexMap[faceVertices[2]],
            vertexIndexMap[faceVertices[2]],
            vertexIndexMap[faceVertices[3]],
            vertexIndexMap[faceVertices[0]]
        };

        for (int i = 0; i < 6; i++)
        {
            triangles[currentTriIndex + i] = faceTriangles[i];
        }

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

        int offset = blocks.GetLength(0) / 2;
        Vector3 offsetVector = new(offset, offset, offset);
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] -= offsetVector;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }
}
