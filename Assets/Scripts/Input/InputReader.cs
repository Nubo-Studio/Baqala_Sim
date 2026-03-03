using System;
using UnityEngine;
using UnityEngine.InputSystem;
using static PlayerControls;

[CreateAssetMenu(fileName = "InputReader", menuName = "Input/InputReader")]
public class InputReader : ScriptableObject, IPlayerActions
{
    public event Action OnInteractPressed;
    public event Action OnThrowPressed;
    public event Action<float> OnRotatePressed;

    public Vector2 MoveInput { get; private set; }
    public Vector2 LookInput { get; private set; }
    public bool IsSprinting { get; private set; }
    public float RotateInput { get; private set; }

    private PlayerControls playerControls;

    private void OnEnable()
    {
        if (playerControls == null)
        {
            playerControls = new PlayerControls();
            playerControls.Player.SetCallbacks(this);
        }

        playerControls.Enable();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        MoveInput = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        LookInput = context.ReadValue<Vector2>();
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        IsSprinting = context.ReadValueAsButton();
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed) OnInteractPressed?.Invoke();
    }

    public void OnThrow(InputAction.CallbackContext context)
    {
        if (context.performed) OnThrowPressed?.Invoke();
    }

    public void OnRotate(InputAction.CallbackContext context)
    {
        if (context.performed) OnRotatePressed?.Invoke(context.ReadValue<float>());
    }

    private void OnDisable()
    {
        playerControls.Disable();
    }
}
