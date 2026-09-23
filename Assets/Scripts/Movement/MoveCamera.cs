using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    public Transform cameraPosition;

    void Start()
    {

    }

    void LateUpdate()
    {
        transform.position = cameraPosition.position;
    }
}