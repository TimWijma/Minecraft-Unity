using UnityEngine;

public class PlayerMovement: MonoBehaviour
{
    public Rigidbody playerBody;
    public float speed = 5f;
    public float jumpForce = 2f;

    void Update()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        Vector3 movement = new(moveHorizontal, 0.0f, moveVertical);
        // playerBody.AddForce(movement * speed * Time.deltaTime);
        playerBody.linearVelocity = new Vector3(movement.x * speed, playerBody.linearVelocity.y, movement.z * speed);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            playerBody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        };
    }
}
