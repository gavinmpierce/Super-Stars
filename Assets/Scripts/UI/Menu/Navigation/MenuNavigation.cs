using UnityEngine;
using UnityEngine.InputSystem;

public class MenuNavigation : MonoBehaviour
{
    [Header("Menu References")]
    [SerializeField] private GameObject MenuParent;
    [SerializeField] private GameObject MainButtons;
    [SerializeField] private GameObject SettingsUI;
    [SerializeField] private GameObject ConfirmCloseGameUI;

    [Header("Input References")]
    [SerializeField] private InputActionReference MenuButton;

    private PlayerCamera playerCamera;
    private PlayerMovement playerMovement;


    void Start()
    {
        playerCamera = Object.FindAnyObjectByType<PlayerCamera>();
        playerMovement = Object.FindAnyObjectByType<PlayerMovement>();
    }
    private void OnEnable()
    {
        MenuButton.action.Enable();
    }

    private void OnDisable()
    {
        MenuButton.action.Disable();
    }

    void Update()
    {
        if (MenuButton.action.triggered)
        {
            if (!MenuParent.activeSelf)
            {
                OpenMenu();
            }
            else
            {
                CloseMenu();
            }
        }
    }

    public void OpenMenu()
    {
        MenuParent.SetActive(true);
        MainButtons.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        playerCamera.SetCameraLocked(true);
        playerMovement.SetMovementLocked(true);
    }
    public void CloseMenu() 
    { 
        MenuParent.SetActive(false);
        MainButtons.SetActive(false);
        SettingsUI.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        playerCamera.SetCameraLocked(false);
        playerMovement.SetMovementLocked(false);
    }

    public void OpenSettings()
    { 
        MainButtons.SetActive(false);
        SettingsUI.SetActive(true);
    }

    public void BackToMainMenu() 
    { 
        MainButtons.SetActive(true);
        SettingsUI.SetActive(false);
    }

    public void OpenConfirmCloseGame()   
    { 
        MainButtons.SetActive(false);
        ConfirmCloseGameUI.SetActive(true);
    }

    public void BackToMainMenuFromConfirmCloseGame() 
    { 
        MainButtons.SetActive(true);
        SettingsUI.SetActive(false);
        ConfirmCloseGameUI.SetActive(false);
    }

    public void CloseGame() 
    {
        Application.Quit();
    }
}
