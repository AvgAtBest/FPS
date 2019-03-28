using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraLook : MonoBehaviour
{
    [Tooltip("Is the cursor hidden?")]
    public bool showCursor = false;
    public bool isInverted = false;
    public Vector2 speed = new Vector2(120f, 120f);
    public float yMinLimit = -80f, yMaxLimit = 80f;

    private float x, y; // Current X and Y degrees of rotation

    // Use this for initialization
    void Start()
    {
        // Hide the cursor
        Cursor.visible = showCursor;
        // Lock the cursor Ternary Operator
        Cursor.lockState = showCursor ? CursorLockMode.None : CursorLockMode.Locked;
        // Get current camera Euler rotation
        Vector3 angles = transform.eulerAngles;
        // Set X and Y degrees to current camera rotation
        x = angles.y; // Pitch (X) Yaw (Y) Roll (Z)
        y = angles.x; 
    }

    // Update is called once per frame
    void Update()
    {
        // Get mouse offsets
        float mouseX = Input.GetAxis("Mouse X") * speed.x * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * speed.y * Time.deltaTime;
        // Check for inversion
        mouseY = isInverted ? -mouseY : mouseY;
        // Rotate camera based on Mouse X and Y
        x += mouseX;
        y -= mouseY;
        // Clamp the angle of the pitch
        y = Mathf.Clamp(y, yMinLimit, yMaxLimit);
        // Rotate parent on Y Axis (Yaw)
        transform.parent.rotation = Quaternion.Euler(0, x, 0);
        // Rotate local on X Axis (Pitch
        transform.localRotation = Quaternion.Euler(y, 0, 0);
    }
}
