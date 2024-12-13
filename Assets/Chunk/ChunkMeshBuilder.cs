using System;
using UnityEngine;

public class ChunkMeshBuilder
{
    private Vector3[] vertices;
    private int[] triangles;
    private int currentVertIndex = 0;
    private int currentTriIndex = 0;
    private readonly int maxCubes;

    public ChunkMeshBuilder(int maxCubes)
    {
        this.maxCubes = maxCubes;
        vertices = new Vector3[maxCubes * 8];
        triangles = new int[maxCubes * 36];
    }

    public void AddCube(Vector3 position)
    {
        for (int i = 0; i < 8; i++)
        {
            Vector3 vertex = new(
                position.x + (i & 1),
                position.y + ((i >> 1) & 1),
                position.z + ((i >> 2) & 1)
            );

            vertices[currentVertIndex + i] = vertex;
        }

        int[] triangles = new int[]
        {
            0, 2, 1, 1, 2, 3,
            5, 7, 4, 4, 7, 6,
            2, 6, 3, 3, 6, 7,
            0, 1, 4, 4, 1, 5,
            0, 4, 2, 2, 4, 6,
            1, 3, 5, 5, 3, 7
        };

        for (int i = 0; i < 36; i++)
        {
            this.triangles[currentTriIndex + i] = triangles[i] + currentVertIndex;
        }

        currentVertIndex += 8;
        currentTriIndex += 36;
    }

    public Mesh BuildMesh()
    {
        Mesh mesh = new();
        Array.Resize(ref vertices, currentVertIndex);
        Array.Resize(ref triangles, currentTriIndex);

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }
}
