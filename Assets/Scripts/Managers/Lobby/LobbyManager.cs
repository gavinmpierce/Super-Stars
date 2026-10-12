using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instance { get; private set; }

    public Lobby CurrentLobby { get; private set; }
    public string LobbyCode => CurrentLobby?.LobbyCode;
    public bool IsReady { get; private set; }
    public bool IsLobbyHost => CurrentLobby != null &&
        CurrentLobby.HostId == AuthenticationService.Instance.PlayerId;

    private const int MaxPlayers = 4;
    private const int HeartbeatSeconds = 15;
    private const string RelayCodeKey = "RelayJoinCode";
    private CancellationTokenSource heartbeatCts;

    private async void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        try
        {
            if (UnityServices.State == ServicesInitializationState.Uninitialized)
                await UnityServices.InitializeAsync();
            if (!AuthenticationService.Instance.IsSignedIn)
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            IsReady = true;
        }
        catch (Exception e) { Debug.LogError($"Unity Services init failed: {e}"); }
    }

    public async Task<bool> CreateLobby(string lobbyName = "Game Lobby")
    {
        if (!IsReady || CurrentLobby != null) return false;
        try
        {
            // Relay first, so the join code is in the lobby from the start
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(MaxPlayers - 1);
            string relayCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            var options = new CreateLobbyOptions
            {
                IsPrivate = true,
                Data = new Dictionary<string, DataObject>
                {
                    { RelayCodeKey, new DataObject(DataObject.VisibilityOptions.Member, relayCode) }
                }
            };
            CurrentLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, MaxPlayers, options);
            StartHeartbeat(CurrentLobby.Id);

            NetworkManager.Singleton.GetComponent<UnityTransport>()
                .SetRelayServerData(allocation.ToRelayServerData("dtls"));

            if (!NetworkManager.Singleton.StartHost())
            {
                await LeaveLobby();
                return false;
            }
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to host: {e}");
            await LeaveLobby();
            return false;
        }
    }

    public async Task<bool> JoinLobby(string lobbyCode)
    {
        if (!IsReady || CurrentLobby != null || string.IsNullOrWhiteSpace(lobbyCode)) return false;
        try
        {
            CurrentLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode.Trim().ToUpper());

            string relayCode = CurrentLobby.Data[RelayCodeKey].Value;
            JoinAllocation join = await RelayService.Instance.JoinAllocationAsync(relayCode);

            NetworkManager.Singleton.GetComponent<UnityTransport>()
                .SetRelayServerData(join.ToRelayServerData("dtls"));

            if (!NetworkManager.Singleton.StartClient())
            {
                await LeaveLobby();
                return false;
            }
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to join: {e}");
            await LeaveLobby();
            return false;
        }
    }

    public async Task LeaveLobby()
    {
        if (CurrentLobby == null) return;
        heartbeatCts?.Cancel();
        var lobby = CurrentLobby;
        bool wasHost = IsLobbyHost;
        CurrentLobby = null;

        try
        {
            if (wasHost) await LobbyService.Instance.DeleteLobbyAsync(lobby.Id);
            else await LobbyService.Instance.RemovePlayerAsync(lobby.Id, AuthenticationService.Instance.PlayerId);
        }
        catch (Exception e) { Debug.LogWarning($"Leave lobby failed: {e}"); }
    }

    private void StartHeartbeat(string lobbyId)
    {
        heartbeatCts?.Cancel();
        heartbeatCts = new CancellationTokenSource();
        _ = HeartbeatLoop(lobbyId, heartbeatCts.Token);
    }

    private async Task HeartbeatLoop(string lobbyId, CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                await LobbyService.Instance.SendHeartbeatPingAsync(lobbyId);
                await Task.Delay(HeartbeatSeconds * 1000, token);
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception e) { Debug.LogError($"Heartbeat failed: {e}"); }
    }

    private void OnDestroy() => heartbeatCts?.Cancel();
}