using System.Globalization;
using Unity.Netcode;
using UnityEngine;

public class MoveCamera : NetworkBehaviour
{
    public Transform cameraPosition;

    void Start()
    {

    }

    void LateUpdate()
    {
        transform.position = cameraPosition.position;
    }

    public override void OnNetworkSpawn()
    {
        enabled = IsOwner; // only your own copy reads input
    }
}