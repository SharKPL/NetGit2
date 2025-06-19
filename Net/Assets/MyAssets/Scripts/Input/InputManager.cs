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
    private InputAction crouchAction;

    private InputAction pauseAction;
    private InputAction chatAction;
    private InputAction sendMsgAction;
    private InputAction interactAction;
    private InputAction inventoryAction;

    private int stopControlCount = 0;

    private int cursorCount = 0;

    public static InputManager Instance { get { return instance; } }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        //DontDestroyOnLoad(gameObject);
        inputActions = new InputSystem_Actions();

        moveAction = inputActions.Player.Move;
        lookAction = inputActions.Player.Look;
        jumpAction = inputActions.Player.Jump;
        sprintAction = inputActions.Player.Sprint;
        crouchAction = inputActions.Player.Duck;

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
        crouchAction.Enable();

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
        crouchAction.Disable();

        pauseAction.Disable();
        chatAction.Disable();
        sendMsgAction.Disable();
        interactAction.Disable();
        inventoryAction.Disable();
    }

    public void TurnPlayerControls(bool turn){
        if (turn)
        {
            stopControlCount--;
            if (stopControlCount > 0) return;
            inputActions.Player.Enable();
        }
        else{
            stopControlCount++;
            GlobalEventManager.showInteract?.Invoke(false);
            inputActions.Player.Disable();
        }
    }

    public void TurnAllControl(bool turn)
    {
        if (turn)
        {
            inputActions.Player.Enable();
            inputActions.UIControl.Enable();
        }
        else
        {
            inputActions.Player.Disable();
            inputActions.UIControl.Disable();
        }
    }

    public int TurnCursor(bool turn)
    {

        if (turn)
        {
            cursorCount++;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = turn;
            return cursorCount;
        }
        else if(--cursorCount > 0)
        {
            Debug.Log($"TurnCursor2: {turn}");
            return cursorCount;
        }
        else
        {
            cursorCount = 0;
            Debug.Log($"TurnCursor: {turn}");
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = turn;
            return cursorCount;
        }

    }

    public bool GetPLayerCanMove()
    {
        return inputActions.Player.enabled;
    }

    public Vector2 GetMovementInput() => moveAction.ReadValue<Vector2>();
    public Vector2 GetLookInput()=> lookAction.ReadValue<Vector2>();

    public InputAction GetMoveAction() => moveAction;
    public InputAction GetJump() => jumpAction;
    public bool IsJump() => jumpAction.WasPressedThisFrame();
    public bool IsRun() => sprintAction.IsPressed();
    public bool IsPause() => pauseAction.WasPressedThisFrame();
    public bool GetCrouchAction() => crouchAction.IsPressed();
    public InputAction GetPause() => pauseAction;
    public InputAction GetLookAction() => lookAction;

    public InputAction GetChatAction()=> chatAction;
    public InputAction GetSendMsgAction() => sendMsgAction;

    public InputAction GetInteractAction()=> interactAction;
    public InputAction GetInventoryAction()=> inventoryAction;


}
