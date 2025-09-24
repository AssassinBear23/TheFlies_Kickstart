using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }
    PlayerInput playerInput;

    private void Start()
    {
        playerInput = GetComponent<PlayerInput>();
    }

    public void SetupInstance()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Multiple instances of InputManager detected. Destroying duplicate.");
            Destroy(this);
        }
    }

    public void SwitchToGameplayInput()
    {
        playerInput.SwitchCurrentActionMap("Gameplay");
    }

    public void SwitchToUIInput()
    {
        playerInput.SwitchCurrentActionMap("UI");
    }
}
