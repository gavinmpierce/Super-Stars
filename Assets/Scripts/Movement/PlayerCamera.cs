using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCamera : MonoBehaviour
{
    public InputActionReference cameraAction;

    [Header("Rotation Settings")]
    public Transform orientation;
    public float xRotation;
    public float yRotation;

    [Header("Camera Settings")]
    public Camera MainCam;
    public Transform MainTransform;
    public bool CamOn;

    private SettingsManager settingsManager;
    private bool cameraLocked;

    void Start()
    {
        settingsManager = Object.FindAnyObjectByType<SettingsManager>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnEnable()
    {
        cameraAction.action.Enable();
    }

    void OnDisable()
    {
        cameraAction.action.Disable();
    }

    void LateUpdate()
    {
        if (cameraLocked)
            return;

        Vector2 mouseInput = cameraAction.action.ReadValue<Vector2>();

        float mouseX = mouseInput.x * Time.deltaTime * settingsManager.MouseSensitivity;
        float mouseY = mouseInput.y * Time.deltaTime * settingsManager.MouseSensitivity;

        yRotation += mouseX;
        xRotation -= mouseY;

        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // rotate cam and orientation
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);


        if (CamOn)
        {
            Vector3 camForward = MainCam.transform.forward;
            camForward.y = 0f;

            if (camForward.sqrMagnitude > 0.001f)
            {
                camForward.Normalize();
                MainTransform.forward = camForward;
            }
        }
    }

    // Referenced in MenuNavigation.cs
    public void SetCameraLocked(bool locked)
    {
        cameraLocked = locked;
    }

}