using TMPro;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public Rigidbody playerBody;
    public float speed = 5f;
    public float jumpForce = 2f;
    public TextMeshProUGUI playerCoords;

    private Camera playerCamera;

    void Start()
    {
        playerCamera = Camera.main;
    }

    void Update()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        Vector3 forward = playerCamera.transform.forward;
        Vector3 right = playerCamera.transform.right;

        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 movement = (forward * moveVertical + right * moveHorizontal).normalized;

        playerBody.linearVelocity = new(
            movement.x * speed,
            playerBody.linearVelocity.y,
            movement.z * speed
        );

        if (Input.GetKeyDown(KeyCode.Space))
        {
            playerBody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            speed = 10f;
        }
        else if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            speed = 5f;
        }

        if (movement != Vector3.zero)
        {
            transform.forward = movement;
        }

        string xCoords = transform.position.x.ToString("F2");
        string yCoords = transform.position.y.ToString("F2");
        string zCoords = transform.position.z.ToString("F2");
        playerCoords.text = $"X: {xCoords}\nY: {yCoords}\nZ: {zCoords}";
    }
}
