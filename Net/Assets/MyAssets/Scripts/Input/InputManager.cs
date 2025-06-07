using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private static InputManager instance;

    private InputSystem_Actions inputActions;

    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction jumpAction;
    private InputAction sprintAction;
    private InputAction pauseAction;
    private InputAction chatAction;
    private InputAction sendMsgAction;
    private InputAction interactAction;
    private InputAction inventoryAction;

    public static InputManager Instance { get { return instance; } }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
        inputActions = new InputSystem_Actions();

        moveAction = inputActions.Player.Move;
        lookAction = inputActions.Player.Look;
        jumpAction = inputActions.Player.Jump;
        sprintAction = inputActions.Player.Sprint;
        interactAction = inputActions.Player.Interact;

        pauseAction = inputActions.UIControl.PauseControl;
        chatAction = inputActions.UIControl.OpenCloseChat;
        sendMsgAction = inputActions.UIControl.SendMessage;
        inventoryAction = inputActions.UIControl.Inventory;
    }

    private void OnEnable()
    {
        moveAction.Enable();
        lookAction.Enable();
        jumpAction.Enable();
        sprintAction.Enable();
        pauseAction.Enable();
        chatAction.Enable();
        sendMsgAction.Enable();
        interactAction.Enable();
        inventoryAction.Enable();
    }

    private void OnDisable()
    {
        moveAction.Disable();
        lookAction.Disable(); 
        jumpAction.Disable();
        sprintAction.Disable();
        pauseAction.Disable();
        chatAction.Disable();
        sendMsgAction.Disable();
        interactAction.Disable();
        inventoryAction.Disable();
    }

    public void TurnPlayerControls(bool turn){
        if (turn)
        {
            inputActions.Player.Enable();
        }
        else{
            inputActions.Player.Disable();
        }
    }

    public Vector2 GetMovementInput() => moveAction.ReadValue<Vector2>();
    public Vector2 GetLookInput()=> lookAction.ReadValue<Vector2>();

    public InputAction GetMoveAction() => moveAction;
    public InputAction GetJump() => jumpAction;
    public bool IsJump() => jumpAction.WasPressedThisFrame();
    public bool IsRun() => sprintAction.IsPressed();
    public bool IsPause() => pauseAction.WasPressedThisFrame();
    public InputAction GetPause() => pauseAction;
    public InputAction GetLookAction() => lookAction;

    public InputAction GetChatAction()=> chatAction;
    public InputAction GetSendMsgAction() => sendMsgAction;

    public InputAction GetInteractAction()=> interactAction;
    public InputAction GetInventoryAction()=> inventoryAction;


}
