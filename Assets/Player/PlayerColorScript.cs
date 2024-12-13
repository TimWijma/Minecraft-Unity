using UnityEngine;

public class PlayerColorScript : MonoBehaviour
{
    public Material playerMaterial;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Object"))
        {
            playerMaterial.color = Color.red;
        } else if (collision.gameObject.CompareTag("Ground"))
        {
            playerMaterial.color = Color.green;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        playerMaterial.color = Color.white;
    }
}
