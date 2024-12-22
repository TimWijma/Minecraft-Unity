using UnityEngine;

public class PlayerMine : MonoBehaviour
{
    public WorldGenerator worldGenerator;
    public Camera playerCamera;
    public float reach = 5f;

    private PlayerInventory inventory;

    public string currentBlockId = "dirt";

    void Awake()
    {
        inventory = GetComponent<PlayerInventory>();
    }

    public void MineBlock()
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
                string blockId = chunk.GetBlockAtWorldPosition(hitBlock);
                Block block = BlockRegistry.Instance.GetBlock(blockId);

                Debug.Log($"Block at {hitBlock} is {blockId}");

                if (hitBlock == null) return;
                if (!block.isBreakable) return;
                if (blockId == "air") return;

                chunk.SetBlock(hitBlock, "air");

                if (block.dropItem != null)
                {
                    inventory.AddItem(block.dropItem);
                }
            }
            else
            {
                Debug.Log($"Chunk not found at {hitBlock}");
            }
        }
    }

    public void PlaceBlock()
    {
        if (inventory.items[inventory.currentIndex] == null || !inventory.items[inventory.currentIndex].item.isPlaceable) return;

        Ray ray = new(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, reach))
        {
            Vector3 hitBlock = new(
                Mathf.Floor(hit.point.x - hit.normal.x / 2),
                Mathf.Floor(hit.point.y - hit.normal.y / 2),
                Mathf.Floor(hit.point.z - hit.normal.z / 2)
            );

            Vector3 playerPosition = new(
                Mathf.Floor(playerCamera.transform.position.x),
                Mathf.Floor(playerCamera.transform.position.y),
                Mathf.Floor(playerCamera.transform.position.z)
            );

            Vector3 placeBlock = hitBlock + hit.normal;

            if (
                placeBlock == playerPosition ||
                placeBlock == playerPosition - new Vector3(0, 1, 0)
            ) return;

            Chunk placeChunk = worldGenerator.GetChunkAtPosition(placeBlock);
            Chunk hitChunk = worldGenerator.GetChunkAtPosition(hitBlock);
            if (placeChunk != null && hitChunk != null)
            {
                string hitBlockId = hitChunk.GetBlockAtWorldPosition(hitBlock);
                Block block = BlockRegistry.Instance.GetBlock(hitBlockId);

                Debug.Log($"Block at {hitBlock} is {hitBlockId}");

                if (hitBlock == null) return;
                if (hitBlockId == "air") return;

                string placeBlockId = inventory.PlaceItem();
                if (placeBlockId != null)
                {
                    placeChunk.SetBlock(placeBlock, placeBlockId);
                }
            }
            else
            {
                Debug.Log($"Chunk not found at {hitBlock}");
            }
        }
    }

    void PickBlock()
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
                string blockId = chunk.GetBlockAtWorldPosition(hitBlock);
                if (blockId != "air")
                {
                    currentBlockId = blockId;
                    Debug.Log($"Current block type is {currentBlockId}");
                }
            }
        }
    }
}
