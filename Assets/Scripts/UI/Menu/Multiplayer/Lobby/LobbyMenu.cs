using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyMenu : MonoBehaviour
{
    [SerializeField] private Button hostButton, joinButton;
    [SerializeField] private TMP_InputField codeInput;
    [SerializeField] private TMP_Text joinErrorLabel;
    [SerializeField] private string gameScene = "LobbyRoom";

    private void Start()
    {
        hostButton.onClick.AddListener(OnHost);
        joinButton.onClick.AddListener(OnJoin);
        if (joinErrorLabel) joinErrorLabel.text = "";
    }

    private void Update()
    {
        bool ready = LobbyManager.Instance != null && LobbyManager.Instance.IsReady;
        hostButton.interactable = joinButton.interactable = ready;
    }

    private async void OnHost()
    {
        hostButton.interactable = false;
        if (await LobbyManager.Instance.CreateLobby())
            NetworkManager.Singleton.SceneManager.LoadScene(gameScene, LoadSceneMode.Single);
    }

    private async void OnJoin()
    {
        joinButton.interactable = false;
        if (joinErrorLabel) joinErrorLabel.text = "Joining...";
        if (!await LobbyManager.Instance.JoinLobby(codeInput.text) && joinErrorLabel)
            joinErrorLabel.text = "Invalid code or lobby full";
    }
}