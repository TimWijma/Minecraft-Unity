using UnityEngine;

public class PlayerFollow : MonoBehaviour
{
    public Transform player;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    void LateUpdate()
    {
        if (player != null)
        {
            transform.position = player.position + new Vector3(0, 2, -2); // Camera position with offset
            transform.LookAt(player); // Ensure camera looks at player
        }
    }

}
