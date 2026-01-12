using UnityEngine;

/// <summary>
/// Adds gamepad / controller look support and optional cursor locking for first-person players.
/// - Attach to the player GameObject (the same object that rotates for yaw).
/// - Assign the camera (usually a child) to `playerCamera` or it will use Camera.main.
/// - Configure axis names in __Edit > Project Settings > Input Manager__ to match your controller.
///
/// This script does not replace movement or firing (those use the project's existing Input axes).
/// It only provides right-stick -> look (pitch/yaw) so controllers can aim and look around.
/// </summary>
public class ControllerControls : MonoBehaviour
{
    [Header("Camera")]
    [Tooltip("Assign the player's camera (if null Camera.main will be used).")]
    public Camera playerCamera;

    [Header("Axis names (configure in Input Manager)")]
    [Tooltip("Right stick horizontal axis name (controller). Fallbacks to \"Mouse X\" when this axis reads ~0).")]
    public string lookHorizontalAxis = "RightStickX";
    [Tooltip("Right stick vertical axis name (controller). Fallbacks to \"Mouse Y\" when this axis reads ~0).")]
    public string lookVerticalAxis = "RightStickY";

    [Header("Sensitivity & Limits")]
    public float lookSensitivity = 3.5f;      // multiplier for controller look
    public float mouseFallbackSensitivity = 1f; // used when falling back to mouse axes
    public bool invertY = false;
    public float minPitch = -85f;
    public float maxPitch = 85f;
    [Range(0f, 0.5f)]
    public float deadzone = 0.15f;            // ignore tiny stick drift

    [Header("Cursor / Lock")]
    [Tooltip("If true the cursor will be locked while a controller is active.")]
    public bool lockCursorWhenControllerActive = true;

    float pitch = 0f;

    void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        // initialize pitch from current camera local rotation
        if (playerCamera != null)
            pitch = playerCamera.transform.localEulerAngles.x;
        else
            pitch = 0f;
    }

    void Update()
    {
        // Read controller right-stick axes (user can map these names in Input Manager)
        float rawLookX = 0f;
        float rawLookY = 0f;

        // Try reading configured axes; if they don't exist or read near zero, fall back to mouse axes
        try
        {
            rawLookX = Input.GetAxis(lookHorizontalAxis);
            rawLookY = Input.GetAxis(lookVerticalAxis);
        }
        catch
        {
            rawLookX = 0f;
            rawLookY = 0f;
        }

        // If the stick is essentially idle, use mouse as a fallback (so mouse still works)
        bool usingMouseFallback = Mathf.Abs(rawLookX) < 0.0001f && Mathf.Abs(rawLookY) < 0.0001f;

        if (usingMouseFallback)
        {
            rawLookX = Input.GetAxis("Mouse X");
            rawLookY = Input.GetAxis("Mouse Y");
        }

        // Apply deadzone for controller sticks (mouse fallback will bypass deadzone)
        if (!usingMouseFallback)
        {
            if (Mathf.Abs(rawLookX) < deadzone) rawLookX = 0f;
            if (Mathf.Abs(rawLookY) < deadzone) rawLookY = 0f;
        }

        float yawDelta = rawLookX * (usingMouseFallback ? mouseFallbackSensitivity : lookSensitivity);
        float pitchDelta = rawLookY * (usingMouseFallback ? -mouseFallbackSensitivity : -lookSensitivity); // invert sign to match typical mouse Y

        if (invertY) pitchDelta = -pitchDelta;

        // Apply yaw to the player (rotate around Y)
        transform.Rotate(0f, yawDelta, 0f);

        // Apply pitch to the camera (rotate around local X)
        if (playerCamera != null)
        {
            // Convert camera's current localEulerAngles.x into a signed -180..180 range for clamping
            float currentPitch = playerCamera.transform.localEulerAngles.x;
            if (currentPitch > 180f) currentPitch -= 360f;
            pitch = currentPitch;

            pitch += pitchDelta;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

            playerCamera.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        // Optionally lock the cursor when the player is using a controller
        if (lockCursorWhenControllerActive)
        {
            bool controllerActive = !usingMouseFallback && (Mathf.Abs(rawLookX) > 0f || Mathf.Abs(rawLookY) > 0f);
            if (controllerActive)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                // keep the existing behavior if the user unlocks with Escape / mouse (don't force unlock)
                if (Cursor.lockState == CursorLockMode.Locked && Input.GetMouseButton(0))
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
            }
        }
    }
}
