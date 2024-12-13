using UnityEngine;

public class PlayerFollow : MonoBehaviour
{
    public Transform player;
    public float height = 10;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    void LateUpdate()
    {
        if (player != null)
        {
            transform.position = player.position + new Vector3(0, height, 0);
            transform.LookAt(player);
        }
    }

}
