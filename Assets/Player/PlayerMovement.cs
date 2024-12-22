using TMPro;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // public CharacterController controller;
    public Rigidbody rb;

    public float speed = 5f;
    public float gravity = -9.81f;
    public float jumpHeight = 1.5f;

    private Vector3 moveDirection;
    private Vector3 wallNormal;

    private bool isGrounded;
    private bool jumpRequest;
    private bool isSprinting;
    private bool isAgainstWall;



    public TextMeshProUGUI coordsText;

    void Update()
    {
        moveDirection = Vector3.zero;
        if (Input.GetKey(KeyCode.W)) moveDirection += transform.forward;
        if (Input.GetKey(KeyCode.S)) moveDirection -= transform.forward;
        if (Input.GetKey(KeyCode.A)) moveDirection -= transform.right;
        if (Input.GetKey(KeyCode.D)) moveDirection += transform.right;
        moveDirection = moveDirection.normalized;

        if (Input.GetKeyDown(KeyCode.Space)) jumpRequest = true;
        isSprinting = Input.GetKey(KeyCode.LeftControl);

        Vector3 pos = transform.position;
        coordsText.text = $"X: {pos.x:F2}\nY: {pos.y:F2}\nZ: {pos.z:F2}";
    }

    void FixedUpdate()
    {
        isGrounded = Physics.CheckBox(
            transform.position + new Vector3(0, -0.9f, 0),
            new Vector3(0.4f, 0.1f, 0.4f),
            Quaternion.identity,
            LayerMask.GetMask("Ground")
        );

        Vector3 targetVelocity = moveDirection * (isSprinting ? speed * 2 : speed);

        if (isAgainstWall && moveDirection.magnitude > 0)
        {
            Vector3 projectedDirection = Vector3.ProjectOnPlane(targetVelocity, wallNormal).normalized;
            targetVelocity = projectedDirection * (isSprinting ? speed * 2 : speed);
        }

        rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);

        if (jumpRequest && isGrounded)
        {
            rb.AddForce(Vector3.up * Mathf.Sqrt(jumpHeight * -2f * gravity), ForceMode.VelocityChange);
            jumpRequest = false;
        }

        if (!isGrounded)
        {
            rb.AddForce(Vector3.up * gravity, ForceMode.Acceleration);
        }
    }

    void OnCollisionStay(Collision collision)
    {
        isAgainstWall = false;
        foreach (ContactPoint contact in collision.contacts)
        {
            if (Mathf.Abs(contact.normal.y) < 0.1f)
            {
                wallNormal = contact.normal;
                isAgainstWall = true;
                break;
            }
        }
    }

    void OnCollisionExit(Collision collision)
    {
        isAgainstWall = false;
    }
}
