using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private PlayerMine playerMine;
    private PlayerInventory playerInventory;
    private PlayerMovement playerMovement;

    void Awake()
    {
        playerMine = GetComponent<PlayerMine>();
        playerInventory = GetComponent<PlayerInventory>();
        playerMovement = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        if (playerMine == null) Debug.LogWarning("Playermine does not exist");
        if (playerInventory == null) Debug.LogWarning("Playerinventory does not exist");

        if (Input.GetMouseButtonDown(0))
        {
            playerMine.MineBlock();
        }
        else if (Input.GetMouseButtonDown(1))
        {
            playerMine.PlaceBlock();
        }
        
        if (Input.GetKeyDown(KeyCode.E))
        {
            playerInventory.OpenInventory();
        }
        else if (Input.GetKeyDown(KeyCode.C))
        {
            playerMovement.ToggleCreativeMode();
        }
        else
        {
            for (int i = 0; i < 10; i++)
            {
                if (Input.GetKeyDown("" + i))
                {
                    playerInventory.SelectItem(i);
                }
            }
        }
    }

}