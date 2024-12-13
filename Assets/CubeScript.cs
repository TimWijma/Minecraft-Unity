using UnityEngine;

public class CubeScript : MonoBehaviour
{
    public Rigidbody cubeBody;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(new Vector3(15, 30, 45) * Time.deltaTime);
        // cubeBody.AddTorque(new Vector3(15, 30, 45) * Time.deltaTime);
    }
}
