using UnityEngine;

public class CameraController : MonoBehaviour
{
    // Reference to the camera's transform
    public Transform cameraTransform;

    // Zoom range limits
    private float maxZoom = 1000f;
    private float minZoom = 10f;

    // Movement speed settings
    public float normalSpeed = 0.4f;
    public float fastSpeed = 1f;
    public float moveSpeed;
    public float moveTime = 5f; // Smoothing factor for movement and rotation transitions
    public float rotationAmount = 0.5f; // Amount to rotate per input

    // Camera target position, rotation, and zoom level
    public Vector3 newPosition;
    public Quaternion newRotation;
    public Vector3 zoomAmount = new(1, 1, 1); // Incremental zoom vector
    public Vector3 newZoom;

    // Normalization factor for adjusting movement speed based on zoom level
    private float zoomMultiplier;

    void Start()
    {
        // Initialize target position, rotation, and zoom to current settings
        newPosition = transform.position;
        newRotation = transform.rotation;
        newZoom = cameraTransform.localPosition;

        // Initialize zoom multiplier to adjust speed based on initial zoom
        zoomMultiplier = CalculateZoomMultiplier();
    }

    void Update()
    {
        // Check for user input to handle zoom and movement each frame
        HandleMouseInput();
        HandleMovementInput();
    }

    void HandleMovementInput()
    {
        // Recalculate zoom multiplier to adapt movement speed based on current zoom level
        zoomMultiplier = CalculateZoomMultiplier();

        // Switch to fast speed if Left Shift is held down
        if (Input.GetKey(KeyCode.LeftShift))
            moveSpeed = fastSpeed * zoomMultiplier;
        else
            moveSpeed = normalSpeed * zoomMultiplier;

        // Move camera forward/backward based on W/S or Up/Down Arrow keys
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            newPosition += transform.forward * moveSpeed;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            newPosition += transform.forward * -moveSpeed;

        // Move camera right/left based on D/A or Right/Left Arrow keys
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            newPosition += transform.right * moveSpeed;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            newPosition += transform.right * -moveSpeed;

        // Rotate camera based on Q/E keys
        if (Input.GetKey(KeyCode.Q))
            newRotation *= Quaternion.Euler(Vector3.up * rotationAmount);
        if (Input.GetKey(KeyCode.E))
            newRotation *= Quaternion.Euler(Vector3.up * -rotationAmount);

        // Adjust zoom based on R/F keys
        if (Input.GetKey(KeyCode.R))
            newZoom += zoomAmount;
        if (Input.GetKey(KeyCode.F))
            newZoom -= zoomAmount;

        // Smoothly interpolate camera movement and rotation towards target values
        transform.SetPositionAndRotation(Vector3.Lerp(transform.position, newPosition, Time.deltaTime * moveTime), 
                                        Quaternion.Lerp(transform.rotation, newRotation, Time.deltaTime * moveTime));

        // Clamp zoom within set bounds and apply the updated zoom smoothly
        if (newZoom.y < minZoom)
            newZoom = new Vector3(0, minZoom, -minZoom);
        else if (newZoom.y > maxZoom)
            newZoom = new Vector3(0, maxZoom, -maxZoom);
        else
            cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, newZoom, Time.deltaTime * moveTime);
    }

    void HandleMouseInput()
    {
        // Adjust zoom based on mouse scroll wheel input
        if (Input.mouseScrollDelta.y != 0)
            newZoom += Input.mouseScrollDelta.y * zoomAmount;
    }

    // Calculates a zoom-based multiplier to maintain consistent movement speed
    float CalculateZoomMultiplier()
    {
        // Calculate a multiplier inversely related to the zoom level to adjust movement speed
        float currentZoom = Mathf.Clamp(cameraTransform.localPosition.y, minZoom, maxZoom);
        return Mathf.Log10(currentZoom + 10f); // Adding 10 to prevent issues with log(0)
    }
}
