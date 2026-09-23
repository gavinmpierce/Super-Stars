using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCamera : MonoBehaviour
{
    public float sensX;
    public float sensY;
    public InputActionReference cameraAction;

    public Transform orientation;

    public float xRotation;
    public float yRotation;

    public float sensitivityStep = 10f;
    public float minSensitivity = 10f;
    public float maxSensitivity = 500f;

    public Camera MainCam;
    public Transform MainTransform;
    public bool CamOn;
    void Start()
    {
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
        Vector2 mouseInput = cameraAction.action.ReadValue<Vector2>();

        float mouseX = mouseInput.x * Time.deltaTime * sensX;
        float mouseY = mouseInput.y * Time.deltaTime * sensY;

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
}