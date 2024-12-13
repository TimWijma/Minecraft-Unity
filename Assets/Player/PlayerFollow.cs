using UnityEngine;

public class PlayerFollow : MonoBehaviour
{
    public Transform player;
    public Vector3 offset = new(0, 5, -7);

    void LateUpdate()
    {
        if (player != null)
        {
            transform.position = player.position + offset;
            transform.LookAt(player);
        }
    }
}
