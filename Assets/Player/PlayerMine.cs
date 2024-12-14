using UnityEngine;

public class PlayerMine : MonoBehaviour
{
    public WorldGenerator worldGenerator;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            MineBlock();
        }
    }

    void MineBlock()
    {
        Ray ray = new(transform.position, Vector3.down);

        Debug.Log($"Mining block at {ray.origin}");

        if (Physics.Raycast(ray, out RaycastHit hit, 5))
        {
            Vector3 hitBlock = new Vector3(
                Mathf.Floor(hit.point.x - hit.normal.x / 2),
                Mathf.Floor(hit.point.y - hit.normal.y / 2),
                Mathf.Floor(hit.point.z - hit.normal.z / 2)
            );

            Chunk chunk = worldGenerator.GetChunkAtPosition(hitBlock);
            if (chunk != null)
            {
                BlockType blockType = chunk.GetBlockType(hitBlock);
                Debug.Log($"Block type at {hitBlock} is {blockType}");
                if (blockType == BlockType.Air) return;
                
                chunk.SetBlockType(hitBlock, BlockType.Air);
            }
            else
            {
                Debug.Log($"Chunk not found at {hitBlock}");
            }

        }
    }
}
