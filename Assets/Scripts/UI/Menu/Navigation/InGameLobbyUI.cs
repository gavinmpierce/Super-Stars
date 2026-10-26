using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InGameLobbyUI : MonoBehaviour
{
    [SerializeField] private string gameScene = "LobbyRoom";
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void startGame()
    {
        
    }

    public void LeaveGame()
    {
        Application.Quit();
    }
}
