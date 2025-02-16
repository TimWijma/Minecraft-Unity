using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    public int chunkSize = 16;

    public string[,,] blocks;
    private MeshFilter meshFilter;
    private MeshCollider meshCollider;
    public Vector3Int chunkIndex;

    public bool blocksGenerated = false;
    public bool structuresGenerated = false;
    public bool meshGenerated = false;

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();
    }

    public void Initialize(Vector3Int chunkIndex, bool isNeighbor = false)
    {
        this.chunkIndex = chunkIndex;
        blocks = new string[chunkSize, chunkSize, chunkSize];

        blocksGenerated = true;
    }

    public void ApplyChunkData(ChunkData chunkData)
    {
        blocks = chunkData.blocks;
    }

    public void ApplyMeshData(MeshData meshData)
    {
        Mesh mesh = new()
        {
            vertices = meshData.vertices,
            triangles = meshData.triangles,
            uv = meshData.uvs,
            normals = meshData.normals
        };

        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;

        meshGenerated = true;
    }

    public void GenerateStructures(Dictionary<Vector3Int, Chunk> chunks)
    {
        Vector3Int structurePos = new(
            Random.Range(1, chunkSize - 2),
            Random.Range(4, chunkSize - 2),
            Random.Range(1, chunkSize - 2)
        );

        for (int x = -1; x <= 1; x++)
        {
            for (int y = 0; y <= 2; y++)
            {
                for (int z = -1; z <= 1; z++)
                {
                    Vector3Int blockPos = structurePos + new Vector3Int(x, y, z);
                    Vector3Int targetChunkCoord = new(
                        chunkIndex.x + Mathf.FloorToInt((float)blockPos.x / chunkSize),
                        chunkIndex.y + Mathf.FloorToInt((float)blockPos.y / chunkSize),
                        chunkIndex.z + Mathf.FloorToInt((float)blockPos.z / chunkSize)
                    );

                    if (chunks.TryGetValue(targetChunkCoord, out Chunk targetChunk))
                    {
                        Vector3Int localPos = new(
                            (blockPos.x + chunkSize) % chunkSize,
                            (blockPos.y + chunkSize) % chunkSize,
                            (blockPos.z + chunkSize) % chunkSize
                        );
                        targetChunk.SetBlock(localPos, "stone", false);
                    }
                }
            }
        }
    }

    public MeshData GenerateMeshData()
    {
        ChunkMeshBuilder meshBuilder = new(chunkSize, blocks);

        for (int z = 0; z < chunkSize; z++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                for (int x = 0; x < chunkSize; x++)
                {
                    if (blocks[x, y, z] == "air") continue;

                    meshBuilder.AddCube(x, y, z);
                }
            }
        }

        return meshBuilder.GetMeshData();
    }

    public void Reset()
    {
        if (meshFilter.mesh != null)
        {
            meshFilter.mesh.Clear();
            meshCollider.sharedMesh = null;
        }

        blocksGenerated = false;
        structuresGenerated = false;
        meshGenerated = false;
    }

    public string GetBlockAtWorldPosition(Vector3 worldPosition)
    {
        int chunkX = Mathf.FloorToInt(worldPosition.x - (chunkIndex.x * chunkSize));
        int chunkY = Mathf.FloorToInt(worldPosition.y - (chunkIndex.y * chunkSize));
        int chunkZ = Mathf.FloorToInt(worldPosition.z - (chunkIndex.z * chunkSize));

        if (chunkX < 0 || chunkX >= chunkSize || chunkY < 0 || chunkY >= chunkSize || chunkZ < 0 || chunkZ >= chunkSize)
        {
            return "air";
        }

        return blocks[chunkX, chunkY, chunkZ];
    }

    public void SetBlock(Vector3 worldPosition, string blockId, bool updateMesh = true)
    {
        int chunkX = Mathf.FloorToInt(worldPosition.x - (chunkIndex.x * chunkSize));
        int chunkY = Mathf.FloorToInt(worldPosition.y - (chunkIndex.y * chunkSize));
        int chunkZ = Mathf.FloorToInt(worldPosition.z - (chunkIndex.z * chunkSize));

        if (chunkX < 0 || chunkX >= chunkSize || chunkY < 0 || chunkY >= chunkSize || chunkZ < 0 || chunkZ >= chunkSize)
        {
            return;
        }

        blocks[chunkX, chunkY, chunkZ] = blockId;
        if (updateMesh)
        {
            // GenerateMesh();
        }
    }

    private string GenerateBlockHash()
    {
        StringBuilder sb = new();

        for (int z = 0; z < chunkSize; z++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                for (int x = 0; x < chunkSize; x++)
                {
                    if (blocks[x, y, z] != "air")
                        sb.Append($"{x},{y},{z}:{blocks[x, y, z]};");
                }
            }
        }

        return sb.ToString();
    }
}
