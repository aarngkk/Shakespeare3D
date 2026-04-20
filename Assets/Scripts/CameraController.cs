using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float dragSpeed = 0.1f;   // Speed for dragging
    public float rotateSpeed = 2f;   // Speed for rotating
    public float zoomSpeed = 5f;     // Speed for zooming

    public float minZoomDistance = 1f; // Minimum distance allowed from center (zoom in limit)
    public float maxZoomDistance = 50f; // Maximum distance allowed from center (zoom out limit)

    private Vector3 lastMousePosition; // Stores the last mouse position for calculating movement

    [SerializeField] private Transform cameraTargetTransform;
    private Vector3 cameraTarget => cameraTargetTransform.position;
    private Vector3 cameraOriginalPos;
    private Quaternion cameraOriginalRotation;

    private void Start()
    {
        cameraOriginalPos = transform.position;
        cameraOriginalRotation = transform.rotation;
    }

    void Update()
    {
        HandleDragging();    // Handles camera dragging (moving horizontally/vertically)
        HandleRotation();    // Handles camera rotation (spinning around)
        HandleZoom();        // Handles camera zoom in/out
    }
    // Handles camera dragging (left click drag)
    void HandleDragging()
    {   // Prevents dragging if the script selection panel is open
        if (ScriptSelectionManager.IsPanelOpen) return; 
        if (Input.GetMouseButtonDown(0))  // Left mouse button pressed down
        {   // Store initial mouse position
            lastMousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButton(0))  // While holding middle mouse button
        {   // Calculate mouse movement
            Vector3 delta = Input.mousePosition - lastMousePosition;
            // Calculate move direction
            Vector3 move = new Vector3(-delta.x * dragSpeed, 0, -delta.y * dragSpeed);

            // Move the camera in world space
            transform.position += transform.right * move.x; // Move left/right
            transform.position += transform.up * move.z;    // Move up/down (moves the scene, not camera height)

            lastMousePosition = Input.mousePosition;
        }
    }
  // Handles camera rotation (right click drag)
    void HandleRotation()
    {   // Prevents rotation if script panel is open
        if (ScriptSelectionManager.IsPanelOpen) return;
        if (Input.GetMouseButtonDown(1))  // Right mouse button pressed
        {
            lastMousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButton(1))  // While holding right mouse button
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;
            float rotationX = delta.y * rotateSpeed; // Up/down mouse movement rotates around X-axis
            float rotationY = delta.x * rotateSpeed; // Left/right mouse movement rotates around Y-axis

            transform.eulerAngles += new Vector3(-rotationX, rotationY, 0);
            lastMousePosition = Input.mousePosition;
        }
    }
    // Handles camera zoom with mouse scroll wheel
    void HandleZoom()
    {
        if (ScriptSelectionManager.IsPanelOpen) return;  // Prevents zooming if panel is open
        float scroll = Input.GetAxis("Mouse ScrollWheel"); // Get scroll input (-1 to 1)
        Vector3 direction = transform.forward * scroll * zoomSpeed;
        Vector3 newPosition = transform.position + direction;

       // Check if new position is within zoom limits
        float distance = Vector3.Distance(newPosition, cameraTarget);

        // Clamp zooming so it doesn't go too far or too close
        if (distance >= minZoomDistance && distance <= maxZoomDistance)
        {
            transform.position = newPosition;
        }
    }

    public void ResetCamera()
    {
        if (!this.gameObject.activeSelf) return;

        transform.position = cameraOriginalPos;
        transform.rotation = cameraOriginalRotation;
    }
}
