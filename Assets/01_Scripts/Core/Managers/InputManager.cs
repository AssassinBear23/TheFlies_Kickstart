using Core.Managers;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    PlayerInput playerInput;

    private void Start()
    {
        playerInput = GetComponent<PlayerInput>();
    }

    public void SetupInputManager()
    {
        if(GameManager.Instance.inputManager == null)
        {
            GameManager.Instance.inputManager = this;
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
