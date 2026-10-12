using UnityEngine;
using UnityEngine.InputSystem;

public class LobbyMenuNavigation : MonoBehaviour
{
    [Header("Menu References")]
    [SerializeField] private GameObject MenuParent;
    [SerializeField] private GameObject InitialChoices;
    [SerializeField] private GameObject JoinGameUI;
    [SerializeField] private GameObject SettingsUI;
    [SerializeField] private GameObject ConfirmCloseGameUI;

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        MenuParent.SetActive(true);
        InitialChoices.SetActive(true);
        JoinGameUI.SetActive(false);
        SettingsUI.SetActive(false);
        ConfirmCloseGameUI.SetActive(false);
    }

    public void OpenInitialChoices()
    {
        JoinGameUI.SetActive(false);
        SettingsUI.SetActive(false);
        ConfirmCloseGameUI.SetActive(false);
        InitialChoices.SetActive(true);
    }

    public void OpenJoinGame()
    {
        InitialChoices.SetActive(false);
        JoinGameUI.SetActive(true);
    }

    public void OpenSettings()
    {
        InitialChoices.SetActive(false);
        SettingsUI.SetActive(true);
    }

    public void OpenConfirmCloseGame()
    {
        InitialChoices.SetActive(false);
        ConfirmCloseGameUI.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
