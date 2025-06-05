using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;
using Mirror;


[RequireComponent(typeof(CharacterController))]
public class CharacterInput : NetworkBehaviour
{
    [Header("Camera")]
    [SerializeField] private Camera playerCamera;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;
    private Vector3 moveDirection;
    private float verticalVelocity;

    [SerializeField] private float sensitivity = 2f;
    [SerializeField] private float minVerticalAngle = -80f;
    [SerializeField] private float maxVerticalAngle = 80f;

    [Header("Jump")]
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float jumpSpeed = 8f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpCooldown = 0.5f;
    private bool isGrounded;
    private float lastJumpTime = 0f;

    [Header("Animations")]
    [SerializeField] private Animator animator;
    private int walkAnim = Animator.StringToHash("isWalking");
    private int jumpAnim = Animator.StringToHash("isJump");
    private int jumpTriggerAnim = Animator.StringToHash("JumpTrigger");

    [Header("Interact")]
    [SerializeField] float radius = 2f;
    [SerializeField] float maxDistance = 5f;
    [SerializeField] LayerMask interactMask;



    [SerializeField] float fallDamage = 5f;
    [SerializeField] float damageHeight = 5f;
    private PlayerHealth playerHealth;

    private System.Action<InputAction.CallbackContext> interactDelegate;
    private System.Action<InputAction.CallbackContext> jumpDelegate;


    //Network
    [SyncVar] private bool isMove = false;
    [SyncVar] private bool isJumping = false;
    [SerializeField] private NetworkAnimator netAnimator;



    private CharacterController playerController;


    private void Awake()
    {
        playerController = GetComponent<CharacterController>();
    }

    private void Start()
    {

        jumpDelegate = ctx => Jump();
        interactDelegate = ctx => Interact();

        InputManager.Instance.GetJump().performed += jumpDelegate;
        InputManager.Instance.GetInteractAction().performed += interactDelegate;
        Debug.Log("InteractDel");


        playerHealth =GetComponent<PlayerHealth>();
    }

    private void OnDestroy()
    {
        InputManager.Instance.GetJump().performed -= jumpDelegate;
        InputManager.Instance.GetInteractAction().performed -= interactDelegate;
    }

    private void Update()
    {
        if (!netIdentity.isLocalPlayer && !netIdentity.isOwned) return;
        Move();
        UpdateGroundedStatus();
    }

    private void Interact()
    {
        Debug.Log("Interact");
        Vector3 direction = transform.forward;

        if (Physics.SphereCast(transform.position, radius, direction, out RaycastHit hit, maxDistance, interactMask))
        {
            var item = hit.collider.gameObject.GetComponent<Item>();
            if (item == null)
            {
                Debug.Log("NoItem");
                return;
            } 
            Inventory.Instance.CmdAddItem(item);
            
            Debug.DrawLine(transform.position, hit.point, Color.red, 2f);
        }
        else
        {
            Debug.DrawRay(transform.position, direction * maxDistance, Color.green, 2f);
        }
    }

    private void Jump()
    {
        if (netIdentity.isLocalPlayer && netIdentity.isOwned)
        {

            if (Time.time - lastJumpTime < jumpCooldown)
                return;

            if (isGrounded && !isJumping)
            {
                isJumping = true;
                CmdSetJump(isJumping);
                lastJumpTime = Time.time;


                verticalVelocity = jumpSpeed;

            }
        }
    }

    [Command]
    private void CmdSetJump(bool isJump)
    {
        animator.SetTrigger(jumpTriggerAnim);
        netAnimator.SetTrigger(jumpTriggerAnim);
    }

    [Command]
    private void CmdSetMove(bool move)
    {
        animator.SetBool(walkAnim, move);
    }

    private void UpdateGroundedStatus()
    {
        isGrounded = playerController.isGrounded;


        if (!isGrounded)
        {
            verticalVelocity += gravity * Time.deltaTime;
        }
        else if (verticalVelocity < 0)
        {

            if (isJumping)
            {
                isJumping = false;
            }
            if (Mathf.Abs(verticalVelocity) > damageHeight)
            {
                playerHealth.CmdTakeDamage(Mathf.Abs(verticalVelocity) * fallDamage);
            }
            //CmdSetJump(isJumping);
            verticalVelocity = -0.5f;
        }
    }
    public void Move()
    {
        var moveInput = InputManager.Instance.GetMovementInput();

        Vector3 forward = playerCamera.transform.forward;
        Vector3 right = playerCamera.transform.right;

        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();


        moveDirection = forward * moveInput.y + right * moveInput.x;

        if (moveDirection.magnitude > 1f)
        {
            moveDirection.Normalize();
        }

        moveDirection *= moveSpeed;

        moveDirection.y = verticalVelocity;

        playerController.Move(moveDirection * Time.deltaTime);
        var velocity = new Vector3(playerController.velocity.x, 0, playerController.velocity.z);
        isMove = velocity.magnitude > 0.2f;
        CmdSetMove(isMove);
        //animator.SetBool(walkAnim, velocity.magnitude > 0.2f);

        if (moveInput == Vector2.zero) return;

        if (playerController.velocity.magnitude > 0.2f)
        {
            Vector3 targetDirection = new Vector3(moveDirection.x, 0, moveDirection.z).normalized;
            if (targetDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }
    public void ResetMovement()
    {
        verticalVelocity = 0f;
        moveDirection = Vector3.zero;
        playerController.enabled = false;
        playerController.transform.position = playerController.transform.position; // Force update
        playerController.enabled = true;
    }
}

