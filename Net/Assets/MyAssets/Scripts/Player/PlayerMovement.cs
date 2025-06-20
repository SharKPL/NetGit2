using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Mirror;

namespace MUSOAR
{
    public class PlayerMovement : NetworkBehaviour, ISaveable
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

        [Header("Параметры приседания")]
        [SerializeField] private float standingHeight = 1.8f;
        [SerializeField] private float crouchingHeight = 1.0f;
        [SerializeField] private float crouchTransitionSpeed = 10f;

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
        [SerializeField] private NetworkAnimator netAnimator;
        [SerializeField] private float animationSmoothTime = 0.1f;

        [Header("Параметры камеры")]
        [SerializeField] private float cameraStandingHeight = 0f;
        [SerializeField] private float cameraCrouchingHeight = -0.2f;
        [SerializeField] private float cameraPlayerDeadHeight = 0.3f;

        private InputManager inputManager = InputManager.Instance;
        [SerializeField]private PlayerCamera playerCamera;
        //private PlayerSuitEnergy playerSuitEnergy;
        [SerializeField] private PlayerHealth playerHealth;
        //private PlayerEnergyConsumptionConfig playerEnergyConsumptionConfig;
        [SerializeField] private CharacterController controller;
        [SerializeField] private Animator animator;
        private MoveState currentMoveState;
        [SerializeField]private Transform cameraTransform;

        private Vector3 moveDirection;
        private Vector3 lastMoveDirection;
        private Vector3 velocity;

        private Vector2 speedVelocity;
        private Vector2 movementInput;
        [SyncVar]private float smoothMouseX;
        private float mouseXVelocity;
        private float clampedMouseX;

        private float currentSpeed;
        private float targetSpeed;
        private float lastJumpTime;
        private float fallStartY;
        
        [SyncVar]private bool isFalling;
        private bool isWalkMode = true;
        [SyncVar]private bool isGrounded;
        private bool enableInput = true;

        private bool isSwitchTo = false;

        public MoveState CurrentMoveState => currentMoveState;
        public float CurrentSpeed => currentSpeed;
        public float CurrentRunSpeed => runSpeed;
        public float CurrentMoveSpeed => moveSpeed;
        public Vector2 MovementInput => movementInput;
        public bool IsGrounded => isGrounded;

        [SyncVar] bool teleport = false;

        
        /*
        public override void OnStartClient()
        {
            //Debug.Log($"{netIdentity} OnStartMove1");
            //base.OnStartClient();
            //Debug.Log($"{netIdentity} OnStartMove2");

            //playerCamera = GetComponentInChildren<PlayerCamera>();
            //playerCamera.gameObject.SetActive(false);
            //inputManager.TurnAllControl(false);
            if (!netIdentity.isLocalPlayer)
            {
                playerCamera.gameObject.SetActive(false);
                inputManager.TurnAllControl(false);
                
            }
        }

        public override void OnStartAuthority()
        {
            base.OnStartAuthority();
            if (!netIdentity.isLocalPlayer && !netIdentity.isOwned) return;
            Debug.Log("StartAuth");
            playerCamera.gameObject.SetActive(true);
            inputManager.TurnAllControl(true);
        }
        */

        public void Teleport(Vector3 pos)
        {
            if(!isServer) return;
            RpcTeleport(pos);
        }
        [Command(requiresAuthority = false)]
        public void CmdTeleport(Vector3 pos)
        {
            Teleport(pos);
        }


        [ClientRpc]
        public void RpcTeleport(Vector3 pos)
        {
            StartCoroutine(TeleportCoroutine(pos));
        }

        private IEnumerator TeleportCoroutine(Vector3 pos)
        {
            teleport = true;
            animator.applyRootMotion = false;

            // Останавливаем текущее движение
            velocity = Vector3.zero;

            controller.enabled = false;
            transform.position = pos;

            // Небольшая задержка для синхронизации
            yield return new WaitForFixedUpdate();

            controller.enabled = true;
            animator.applyRootMotion = true;
            teleport = false;

            Debug.Log($"Player teleported to {pos}");
        }




        private void Update()
        {
            if (teleport) return;
            //if (!netIdentity.isLocalPlayer && !netIdentity.isOwned) return;
            //if (!isSwitchTo) return;
            if (!isLocalPlayer) return;
            GetMovementInput();
            HandleMovement();
            HandleJump();
            HandleGravity();
            UpdateAnimator();
            CheckGround();

            HandleCrouch();
            HandleCameraPosition();
        }

        private void HandleCrouch()
        {

            float targetHeight = InputManager.Instance.GetCrouchAction() ? crouchingHeight : standingHeight;
            float currentHeight = controller.height;

            if (Mathf.Abs(currentHeight - targetHeight) > 0.01f)
            {
                controller.height = Mathf.Lerp(currentHeight, targetHeight, crouchTransitionSpeed * Time.deltaTime);
                controller.center = new Vector3(0, controller.height / 2, 0);

                float targetCameraHeight = InputManager.Instance.GetCrouchAction() ? cameraCrouchingHeight : cameraStandingHeight;
                Vector3 currentCameraPos = cameraTransform.localPosition;
                Vector3 targetCameraPos = new Vector3(currentCameraPos.x, targetCameraHeight, currentCameraPos.z);

                cameraTransform.localPosition = Vector3.Lerp(currentCameraPos, targetCameraPos, crouchTransitionSpeed * Time.deltaTime);
            }
        }

        private void HandleCameraPosition()
        {
            float targetHeight;

            if (InputManager.Instance.GetCrouchAction())
            {
                targetHeight = cameraCrouchingHeight;
            }
            else
            {
                targetHeight = cameraStandingHeight;
            }

            Vector3 currentCameraPos = cameraTransform.localPosition;
            Vector3 targetCameraPos = new Vector3(currentCameraPos.x, targetHeight, currentCameraPos.z);

            cameraTransform.localPosition = Vector3.Lerp(currentCameraPos, targetCameraPos, crouchTransitionSpeed * Time.deltaTime);
        }

        [Command]
        private void CmdSetCrouchAnimation(bool isCrouching)
        {
            animator.SetBool("Crouch", isCrouching);
            RpcSetCrouchAnimation(isCrouching);
        }

        [ClientRpc]
        private void RpcSetCrouchAnimation(bool isCrouching)
        {
            if (!isLocalPlayer)
            {
                animator.SetBool("Crouch", isCrouching);
            }
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

            RegisterSaveable();
        }

        private void OnDestroy()
        {
            GlobalEventManager.OnCharacterSwitch.RemoveListener(DisableMovement);
            GlobalEventManager.OnPauseStateChanged.RemoveListener(OnPause);
            GlobalEventManager.OnPlayerDie.RemoveListener(OnPlayerDie);

            UnregisterSaveable();
        }

        private void OnPause(bool isPaused)
        {
            enableInput = !isPaused;
        }

        private void OnPlayerDie()
        {
            enableInput = false;
            CmdPlayDeathAnimation();
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
                CmdJumpAnim();
            }
        }
        [ClientRpc]
        private void RpcJumpAnim()
        {
            animator.SetTrigger("Jump");
            netAnimator.SetTrigger("Jump");
        }

        [Command]
        private void CmdJumpAnim()
        {
            RpcJumpAnim();
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
                CmdHardLandAnim(true,false);
            }
            else
            {
                CmdLandingAnim();
            }
            isFalling = false;
        }
        [ClientRpc]
        private void RpcHardLandAnim(bool b_anim, bool b_input)
        {
            animator.SetBool("HardLanding", b_anim);
            enableInput = b_input;
        }
        [Command]
        private void CmdHardLandAnim(bool b_anim, bool b_input)
        {
            RpcHardLandAnim(b_anim,b_input);
        }

        [ClientRpc]
        private void RpcLandingAnim()
        {
            netAnimator.SetTrigger("Landing");
            animator.SetTrigger("Landing");
        }
        [Command]
        private void CmdLandingAnim()
        {
            RpcLandingAnim();
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

            CmdUpdateSpeedAnim(localVelocity, diagonal);
            CmdSetCrouchAnimation(InputManager.Instance.GetCrouchAction());

            if (enableInput)
                clampedMouseX = Mathf.Clamp(inputManager.GetLookInput().x, -1f, 1f);
            else
                clampedMouseX = 0;

            smoothMouseX = Mathf.SmoothDamp(smoothMouseX, clampedMouseX, ref mouseXVelocity, animationSmoothTime);
            animator.SetFloat("MouseX", smoothMouseX);
            
            animator.SetBool("IsGrounded", isGrounded);
        }

        [ClientRpc]
        private void RpcUpdateSpeedAnim(Vector3 localVelocity, float diagonal)
        {
            animator.SetFloat("FB_Speed", Mathf.SmoothDamp(animator.GetFloat("FB_Speed"), localVelocity.z * diagonal, ref speedVelocity.x, animationSmoothTime));
            animator.SetFloat("RL_Speed", Mathf.SmoothDamp(animator.GetFloat("RL_Speed"), localVelocity.x * diagonal, ref speedVelocity.y, animationSmoothTime));
        }

        [Command]
        private void CmdUpdateSpeedAnim(Vector3 localVelocity, float diagonal)
        {
            RpcUpdateSpeedAnim(localVelocity, diagonal);
        }

        [ClientRpc]
        private void RpcPlayDeathAnimation()
        {
            animator.SetBool("Dead", true);
        }

        [Command]
        private void CmdPlayDeathAnimation()
        {
            RpcPlayDeathAnimation();
        }



        public void HardLandingEnd()
        {
            animator.SetBool("HardLanding", false);
            enableInput = true;
        }

     

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
    }
}
