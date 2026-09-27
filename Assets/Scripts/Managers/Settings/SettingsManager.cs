using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private float mouseSensitivity = 100f;
    [SerializeField] private float minSensitivity = 10f;
    [SerializeField] private float maxSensitivity = 500f;

    // Makes it so the field is read-only
    public float MouseSensitivity => mouseSensitivity;

    //[Header("Movement Settings")]

    public void SetSensitivity(float value)
    {
        mouseSensitivity = Mathf.Clamp(
            value,
            minSensitivity,
            maxSensitivity
        );
    }
}
