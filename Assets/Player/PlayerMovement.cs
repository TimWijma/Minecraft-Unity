using TMPro;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // public CharacterController controller;
    public Rigidbody rb;

    public float speed = 20f;
    public float gravity = -14;
    public float jumpHeight = 1.5f;

    private Vector3 moveDirection;
    private Vector3 wallNormal;
    private float verticalMovement;

    private bool isGrounded;
    private bool jumpRequest;
    private bool isSprinting;
    private bool isAgainstWall;

    public TextMeshProUGUI coordsText;

    private bool isCreativeMode = true;

    void Update()
    {
        moveDirection = Vector3.zero;
        if (Input.GetKey(KeyCode.W)) moveDirection += transform.forward;
        if (Input.GetKey(KeyCode.S)) moveDirection -= transform.forward;
        if (Input.GetKey(KeyCode.A)) moveDirection -= transform.right;
        if (Input.GetKey(KeyCode.D)) moveDirection += transform.right;
        moveDirection = moveDirection.normalized;

        if (isCreativeMode)
        {
            verticalMovement = 0;
            if (Input.GetKey(KeyCode.Space)) verticalMovement = 1.5f;
            if (Input.GetKey(KeyCode.LeftShift)) verticalMovement = -1.5f;
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Space)) jumpRequest = true;
        }

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

        // if (isAgainstWall && moveDirection.magnitude > 0)
        // {
        //     Vector3 projectedDirection = Vector3.ProjectOnPlane(targetVelocity, wallNormal).normalized;
        //     targetVelocity = projectedDirection * (isSprinting ? speed * 2 : speed);
        // }

        if (isCreativeMode)
        {
            rb.linearVelocity = new Vector3(targetVelocity.x, verticalMovement * speed, targetVelocity.z);
        }
        else
        {
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
    }

    // void OnCollisionStay(Collision collision)
    // {
    //     isAgainstWall = false;
    //     foreach (ContactPoint contact in collision.contacts)
    //     {
    //         if (Mathf.Abs(contact.normal.y) < 0.1f)
    //         {
    //             wallNormal = contact.normal;
    //             isAgainstWall = true;
    //             break;
    //         }
    //     }
    // }

    // void OnCollisionExit(Collision collision)
    // {
    //     isAgainstWall = false;
    // }

    public void ToggleCreativeMode()
    {
        isCreativeMode = !isCreativeMode;
        speed = isCreativeMode ? 10 : 5;
        // reset upward velocity
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        Debug.Log($"Creative mode is now {(isCreativeMode ? "enabled" : "disabled")}");
    }
}
