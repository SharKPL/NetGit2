using UnityEngine;
using Mirror;

namespace MUSOAR
{
    public class PlayerMovement : NetworkBehaviour //ISaveable
    {
        public enum MoveState
        {
            Idle,
            Walk,
            Move,
            Run,
            Jump,
            Fall
        }

        [Header("Движение")]
        [SerializeField] private float walkSpeed = 2f;
        [SerializeField] private float moveSpeed = 4f;
        [SerializeField] private float runSpeed = 6f;

        [Header("Инерция")]
        [SerializeField] private float acceleration = 10f;
        [SerializeField] private float deceleration = 8f;
        [SerializeField] private float turnSpeed = 15f;
        [SerializeField] private float airControl = 0.5f;

        [Header("Прыжок")]
        [SerializeField] private float jumpForce = 5f;
        [SerializeField] private float jumpCooldown = 0.5f;
        [SerializeField] private float groundCheckRadius = 0.2f;
        [SerializeField] private float groundCheckOffset = 0.5f;
        [SerializeField] private LayerMask groundLayer;

        [Header("Урон от падения")]
        [SerializeField] private float minFallDamageHeight = 5f;
        [SerializeField] private float fallDamageMultiplier = 10f;

        [Header("Анимация")]
        [SerializeField] private float animationSmoothTime = 0.1f;

        private InputManager inputManager;
        private PlayerCamera playerCamera;
        //private PlayerSuitEnergy playerSuitEnergy;
        private PlayerHealth playerHealth;
        //private PlayerEnergyConsumptionConfig playerEnergyConsumptionConfig;
        private CharacterController controller;
        private Animator animator;
        private MoveState currentMoveState;
        private Transform cameraTransform;

        private Vector3 moveDirection;
        private Vector3 lastMoveDirection;
        private Vector3 velocity;

        private Vector2 speedVelocity;
        private Vector2 movementInput;
        private float smoothMouseX;
        private float mouseXVelocity;
        private float clampedMouseX;

        private float currentSpeed;
        private float targetSpeed;
        private float lastJumpTime;
        private float fallStartY;
        
        private bool isFalling;
        private bool isWalkMode = true;
        private bool isGrounded;
        private bool enableInput = true;

        private bool isSwitchTo = false;

        public MoveState CurrentMoveState => currentMoveState;
        public float CurrentSpeed => currentSpeed;
        public float CurrentRunSpeed => runSpeed;
        public float CurrentMoveSpeed => moveSpeed;
        public Vector2 MovementInput => movementInput;
        public bool IsGrounded => isGrounded;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            animator = GetComponent<Animator>();
            inputManager = InputManager.Instance;
            playerCamera = GetComponentInChildren<PlayerCamera>();
            cameraTransform = playerCamera.GetCamera().transform;
            playerHealth = GetComponent<PlayerHealth>();
        }

        private void Update()
        {
            if (!netIdentity.isLocalPlayer && !netIdentity.isOwned) return;
            //if (!isSwitchTo) return;
            GetMovementInput();
            HandleMovement();
            HandleJump();
            HandleGravity();
            UpdateAnimator();
            CheckGround();
        }

        private void DisableMovement(bool turn)
        {
            isSwitchTo = turn;
        }

        private void OnEnable()
        {
            GlobalEventManager.OnCharacterSwitch.AddListener(DisableMovement);
            GlobalEventManager.OnPauseStateChanged.AddListener(OnPause);
            GlobalEventManager.OnPlayerDie.AddListener(OnPlayerDie);

            //RegisterSaveable();
        }

        private void OnDestroy()
        {
            GlobalEventManager.OnCharacterSwitch.RemoveListener(DisableMovement);
            GlobalEventManager.OnPauseStateChanged.RemoveListener(OnPause);
            GlobalEventManager.OnPlayerDie.RemoveListener(OnPlayerDie);

            //UnregisterSaveable();
        }

        private void OnPause(bool isPaused)
        {
            enableInput = !isPaused;
        }

        private void OnPlayerDie()
        {
            enableInput = false;
            PlayDeathAnimation();
        }

        private void OnPlayerUseWorkbench(bool isUsing)
        {
            enableInput = !isUsing;
        }

        private void GetMovementInput()
        {
            if (enableInput)
            {
                movementInput = inputManager.GetMovementInput();
            }
            else
            {
                movementInput = Vector2.zero;
            }
        }

        private void HandleMovement()
        {        
            if (!enableInput) return;

            Vector3 forward = cameraTransform.forward;
            Vector3 right = cameraTransform.right;
            forward.y = right.y = 0;
            forward.Normalize();
            right.Normalize();

            moveDirection = (forward * movementInput.y + right * movementInput.x).normalized;

            UpdateMoveState(movementInput);
            targetSpeed = DetermineTargetSpeed(movementInput);
            
            Vector3 targetVelocity = moveDirection * targetSpeed;
            Vector3 horizontalVelocity = new Vector3(velocity.x, 0, velocity.z);
            horizontalVelocity = Vector3.Lerp(horizontalVelocity, targetVelocity, (movementInput.magnitude > 0.1f ? acceleration : deceleration) * (isGrounded ? 1f : airControl) * Time.deltaTime);
            
            velocity = new Vector3(horizontalVelocity.x, velocity.y, horizontalVelocity.z);
            controller.Move(velocity * Time.deltaTime);

            if (movementInput.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up));
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
            }

            currentSpeed = new Vector3(controller.velocity.x, 0, controller.velocity.z).magnitude;
            lastMoveDirection = moveDirection;
        }

        private void HandleJump()
        {
            if (!enableInput) return;

            if (inputManager.IsJump() && isGrounded && Time.time >= lastJumpTime + jumpCooldown)
            {
                /*   
                if (playerSuitEnergy.TrySpendEnergyInstant(playerEnergyConsumptionConfig.JumpEnergyCost))
                {
                    velocity.y = Mathf.Sqrt(jumpForce * -2f * Physics.gravity.y);
                    lastJumpTime = Time.time;
                    animator.SetTrigger("Jump");
                }
                */

                velocity.y = Mathf.Sqrt(jumpForce * -2f * Physics.gravity.y);
                lastJumpTime = Time.time;
                animator.SetTrigger("Jump");
            }
        }

        private void HandleGravity()
        {
            if (!isGrounded)
            {
                velocity.y += Physics.gravity.y * Time.deltaTime;
            }
        }

        private void CheckGround()
        {
            bool wasGrounded = isGrounded;
            Vector3 spherePosition = transform.position + Vector3.down * groundCheckOffset;
            isGrounded = Physics.CheckSphere(spherePosition, groundCheckRadius, groundLayer);

            if (wasGrounded && !isGrounded)
            {
                fallStartY = transform.position.y;
                isFalling = true;
            }
            
            if (!wasGrounded && isGrounded && isFalling)
            {
                ProcessFallDamage();
            }

            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f;
            }
        }

        private void ProcessFallDamage()
        {
            float fallDistance = fallStartY - transform.position.y;
            if (fallDistance > minFallDamageHeight)
            {
                float damage = (fallDistance - minFallDamageHeight) * fallDamageMultiplier;
                //playerHealth.TakeDamage(damage);
                animator.SetBool("HardLanding", true);
                enableInput = false;
            }
            else
            {
                animator.SetTrigger("Landing");
            }
            isFalling = false;
        }

        private void UpdateMoveState(Vector2 input)
        {
            if (!isGrounded)
                currentMoveState = velocity.y > 0 ? MoveState.Jump : MoveState.Fall;
            else if (input.magnitude < 0.1f)
                currentMoveState = MoveState.Idle;
            else if (inputManager.IsRun() /*&& playerSuitEnergy.TrySpendEnergy(playerEnergyConsumptionConfig.SprintEnergyCostPerSecond)*/)
                currentMoveState = MoveState.Run;
            else
                currentMoveState = isWalkMode ? MoveState.Walk : MoveState.Move;

            //if (inputManager.IsChangeMoveMode()) isWalkMode = !isWalkMode;
        }

        private float DetermineTargetSpeed(Vector2 input)
        {
            if (input.magnitude < 0.1f)
                return 0f;

            return currentMoveState switch
            {
                MoveState.Walk => walkSpeed,
                MoveState.Run => runSpeed,
                MoveState.Move => moveSpeed,
                _ => moveSpeed
            };
        }

        private void UpdateAnimator()
        {
            Vector3 localVelocity = transform.InverseTransformDirection(controller.velocity);
            float diagonal = (Mathf.Abs(inputManager.GetMovementInput().x) > 0.1f && Mathf.Abs(inputManager.GetMovementInput().y) > 0.1f) ? 1.41f : 1f;
            
            animator.SetFloat("FB_Speed", Mathf.SmoothDamp(animator.GetFloat("FB_Speed"), localVelocity.z * diagonal, ref speedVelocity.x, animationSmoothTime));
            animator.SetFloat("RL_Speed", Mathf.SmoothDamp(animator.GetFloat("RL_Speed"), localVelocity.x * diagonal, ref speedVelocity.y, animationSmoothTime));

            if (enableInput)
                clampedMouseX = Mathf.Clamp(inputManager.GetLookInput().x, -1f, 1f);
            else
                clampedMouseX = 0;

            smoothMouseX = Mathf.SmoothDamp(smoothMouseX, clampedMouseX, ref mouseXVelocity, animationSmoothTime);
            animator.SetFloat("MouseX", smoothMouseX);
            
            animator.SetBool("IsGrounded", isGrounded);
        }

        private void PlayDeathAnimation()
        {
            animator.SetBool("Dead", true);
        }

        public void HardLandingEnd()
        {
            animator.SetBool("HardLanding", false);
            enableInput = true;
        }

        /*
        // Save/Load
        public void GetSaveData(SaveData saveData)
        {
            saveData.PlayerPosition = transform.position;
        }

        public void SetSaveData(SaveData saveData)
        {
            bool wasControllerEnabled = controller.enabled;
            controller.enabled = false;
            
            transform.position = saveData.PlayerPosition;
            
            controller.enabled = wasControllerEnabled;
        }

        public void RegisterSaveable()
        {
            SaveableRegistry.Register(this);
        }

        public void UnregisterSaveable()
        {
            SaveableRegistry.Unregister(this);
        }
        */
    }
}
