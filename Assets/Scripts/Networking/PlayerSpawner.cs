using Unity.Netcode;
using UnityEngine;

public class PlayerSpawner : NetworkBehaviour
{
    [SerializeField] private NetworkObject playerPrefab;
    [SerializeField] private Transform[] spawnPoints;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        foreach (ulong id in NetworkManager.ConnectedClientsIds) SpawnPlayer(id);
        NetworkManager.OnClientConnectedCallback += SpawnPlayer;
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer && NetworkManager != null)
            NetworkManager.OnClientConnectedCallback -= SpawnPlayer;
    }

    private void SpawnPlayer(ulong clientId)
    {
        if (NetworkManager.ConnectedClients[clientId].PlayerObject != null) return;

        int index = (int)(NetworkManager.ConnectedClientsIds.Count - 1) % spawnPoints.Length;
        NetworkObject player = Instantiate(playerPrefab, spawnPoints[index].position, Quaternion.identity);
        player.SpawnAsPlayerObject(clientId);
    }
}