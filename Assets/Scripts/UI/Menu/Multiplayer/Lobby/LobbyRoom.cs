using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyRoom : MonoBehaviour
{
    [SerializeField] private TMP_Text codeLabel;
    [SerializeField] private Button leaveButton;
    [SerializeField] private Button copyCodeButton;
    // [SerializeField] private Button inviteButton;
    [SerializeField] private Button startButton;
    [SerializeField] private string menuScene = "MainMenu";
    private bool leaving;

    private void Start()
    {
        if (!LobbyManager.Instance.IsLobbyHost)
        {
            startButton.gameObject.SetActive(false);
        }
        codeLabel.gameObject.SetActive(LobbyManager.Instance.IsLobbyHost);
        codeLabel.text = $"Lobby code: {LobbyManager.Instance.LobbyCode}";
        leaveButton.onClick.AddListener(Leave);
        copyCodeButton.onClick.AddListener(CopyLobbyCode);
        startButton.onClick.AddListener(StartGame);
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

    private void Invite()
    {
        // TODO: Implement invite functionality
    }

    private void CopyLobbyCode()
    {
        GUIUtility.systemCopyBuffer = LobbyManager.Instance.LobbyCode;
    }

    private void StartGame()
    {
        if (!LobbyManager.Instance.IsLobbyHost) return;
        // create a system to grab a random scene from a list of games
        //NetworkManager.Singleton.SceneManager.LoadScene("InGameLobby", LoadSceneMode.Single);
    }

    private async void Leave()
    {
        if (leaving) return;
        leaving = true;

        await LobbyManager.Instance.LeaveLobby();
        GameObject services = NetworkManager.Singleton.gameObject;
        NetworkManager.Singleton.Shutdown();
        Destroy(services);
        SceneManager.LoadScene(menuScene);
    }
}