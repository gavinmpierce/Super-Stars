using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyRoom : MonoBehaviour
{
    [SerializeField] private TMP_Text codeLabel;
    [SerializeField] private Button leaveButton;
    [SerializeField] private string menuScene = "MainMenu";
    private bool leaving;

    private void Start()
    {
        codeLabel.gameObject.SetActive(LobbyManager.Instance.IsLobbyHost);
        codeLabel.text = $"Lobby code: {LobbyManager.Instance.LobbyCode}";
        leaveButton.onClick.AddListener(Leave);
        NetworkManager.Singleton.OnClientDisconnectCallback += OnDisconnected;
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnDisconnected;
    }

    private void OnDisconnected(ulong clientId)
    {
        // A client lands here when the host shuts down
        if (!NetworkManager.Singleton.IsServer) Leave();
    }

    private async void Leave()
    {
        if (leaving) return;
        leaving = true;

        await LobbyManager.Instance.LeaveLobby();
        GameObject services = NetworkManager.Singleton.gameObject;
        NetworkManager.Singleton.Shutdown();
        Destroy(services); // a fresh LobbyManager + NetworkManager arrive with the menu scene
        SceneManager.LoadScene(menuScene);
    }
}