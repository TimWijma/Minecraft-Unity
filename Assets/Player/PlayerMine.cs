using UnityEngine;

public class PlayerMine : MonoBehaviour
{
    public WorldGenerator worldGenerator;
    public Camera playerCamera;
    public float reach = 5f;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            MineBlock();
        }
        else if (Input.GetMouseButtonDown(1))
        {
            PlaceBlock();
        }
    }

    void MineBlock()
    {
        Ray ray = new(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, reach))
        {
            Vector3 hitBlock = new(
                Mathf.Floor(hit.point.x - hit.normal.x / 2),
                Mathf.Floor(hit.point.y - hit.normal.y / 2),
                Mathf.Floor(hit.point.z - hit.normal.z / 2)
            );

            Chunk chunk = worldGenerator.GetChunkAtPosition(hitBlock);
            if (chunk != null)
            {
                BlockType blockType = chunk.GetBlockType(hitBlock);
                Block block = BlockRegistry.GetBlock(blockType);
                
                Debug.Log($"Block at {hitBlock} is {blockType}");

                if (hitBlock == null) return;
                if (!block.isBreakable) return;
                if (blockType == BlockType.Air) return;

                chunk.SetBlockType(hitBlock, BlockType.Air);
            }
            else
            {
                Debug.Log($"Chunk not found at {hitBlock}");
            }
        }
    }

    void PlaceBlock()
    {
        Ray ray = new(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, reach))
        {
            Vector3 hitBlock = new(
                Mathf.Floor(hit.point.x + hit.normal.x / 2),
                Mathf.Floor(hit.point.y + hit.normal.y / 2),
                Mathf.Floor(hit.point.z + hit.normal.z / 2)
            );

            Chunk chunk = worldGenerator.GetChunkAtPosition(hitBlock);
            if (chunk != null)
            {
                BlockType blockType = chunk.GetBlockType(hitBlock);
                Block block = BlockRegistry.GetBlock(blockType);

                Debug.Log($"Block at {hitBlock} is {blockType}");

                if (hitBlock == null) return;
                // if (!block.isPlaceable) return;

                chunk.SetBlockType(hitBlock, BlockType.Dirt);
            }
            else
            {
                Debug.Log($"Chunk not found at {hitBlock}");
            }
        }
    }
}
