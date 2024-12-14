using UnityEngine;

public class PlayerFollow : MonoBehaviour
{
    public Transform player;
    public Vector3 offset = new(0, 5, -7);
    public float mouseSensitivity = 2f;

    private float currentRotationX = 0f;
    private float currentRotationY = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = true;
    }

    void LateUpdate()
    {
        if (player == null) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        currentRotationX += mouseX;
        currentRotationY -= mouseY;

        Quaternion rotation = Quaternion.Euler(currentRotationY, currentRotationX, 0);
        Vector3 rotatedOffset = rotation * offset;

        transform.position = player.position + rotatedOffset;
        transform.LookAt(player);
    }
}
