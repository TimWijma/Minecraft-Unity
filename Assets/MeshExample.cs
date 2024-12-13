using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MeshExample : MonoBehaviour
{
    public int xSize = 20;
    public int zSize = 20;

    void Start()
    {
        GenerateChunk();
    }

    void GenerateChunk()
    {
        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        // Vertices and triangles
        Vector3[] vertices = new Vector3[(xSize + 1) * (zSize + 1)];
        int[] triangles = new int[xSize * zSize * 6];

        for (int z = 0, i = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++, i++)
            {
                vertices[i] = new Vector3(x, x+z, z);
            }
        }

        int vert = 0;
        int tris = 0;
        for (int z = 0; z < zSize; z++)
        {
            for (int x = 0; x < xSize; x++)
            {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + xSize + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + xSize + 1;
                triangles[tris + 5] = vert + xSize + 2;

                vert++;
                tris += 6;
            }
            vert++;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
    }

    private void OnDrawGizmos()
    {
        // Draw the edges of the chunk for visualization
        if (!Application.isPlaying) return;

        Gizmos.color = Color.green;
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;

        foreach (Vector3 vertex in vertices)
        {
            Gizmos.DrawSphere(transform.TransformPoint(vertex), 0.1f);
        }
    }
}
