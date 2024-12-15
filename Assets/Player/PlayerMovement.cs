using TMPro;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;
    
    public float speed = 5f;
    public float gravity = -9.81f;
    public float jumpHeight = 1.5f;

    private bool isGrounded;
    private float groundDistance = 0.4f;

    private Vector3 velocity;

    public TextMeshProUGUI coordsText;

    void Update()
    {
        // isGrounded = Physics.Raycast(transform.position, Vector3.down, groundDistance);

        // if (isGrounded && velocity.y < 0)
        // {
        //     velocity.y = -2f;
        // }

        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        Vector3 movement = transform.right * moveHorizontal + transform.forward * moveVertical;

        controller.Move(speed * Time.deltaTime * movement);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            speed = 10f;
        }
        else if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            speed = 5f;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        string xCoords = transform.position.x.ToString("F2");
        string yCoords = transform.position.y.ToString("F2");
        string zCoords = transform.position.z.ToString("F2");
        coordsText.text = $"X: {xCoords}\nY: {yCoords}\nZ: {zCoords}";
    }
}
