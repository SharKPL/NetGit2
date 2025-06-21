using UnityEngine;
using Zenject;

namespace MUSOAR
{
    public class HelmetBobController : MonoBehaviour
    {
        [Header("Настройки покачивания")]
        [SerializeField] private float idleBobAmount = 0.01f;
        [SerializeField] private float walkBobAmount = 0.05f;
        [SerializeField] private float runBobAmount = 0.1f;
        [SerializeField] private float idleBobFrequency = 1f;
        [SerializeField] private float walkBobFrequency = 10f;
        [SerializeField] private float runBobFrequency = 14f;
        
        [Header("Настройки наклона")]
        [SerializeField] private float tiltAmount = 3f;
        [SerializeField] private float tiltSpeed = 10f;
        [SerializeField] private float rotationTiltFactor = 1.5f;

        [Header("Эффекты падения/приземления")]
        [SerializeField] private float landingBobAmount = 0.15f;
        [SerializeField] private float fallingOffset = 0.1f;
        [SerializeField] private float fallRecoverySpeed = 15f;
        
        [Header("Сглаживание")]
        [SerializeField] private float transitionSpeed = 10f;

        [Header("Смещения")]
        [SerializeField] private Vector3 helmetOffset = Vector3.zero;

        [Header("Ссылки")]
        [SerializeField] private PlayerMovement playerMovement;

        private float bobTimer;
        private Vector3 initialPosition;
        private Quaternion initialRotation;
        private float currentBobAmount;
        private float currentBobFrequency;
        private float currentTiltAmount;
        private bool isBobbing;
        
        private bool wasGrounded = true;
        private bool isFalling = false;
        private float landingEffect = 0f;
        private float fallingEffect = 0f;
        private float previousYRotation = 0f;
        private float rotationSpeed = 0f;
        
        
        private void Awake()
        {
            initialPosition = transform.localPosition;
            initialRotation = transform.localRotation;
        }

        private void Update()
        {
            if (playerMovement == null) return;

            isBobbing = playerMovement.CurrentMoveState == PlayerMovement.MoveState.Walk ||
                        playerMovement.CurrentMoveState == PlayerMovement.MoveState.Move ||
                        playerMovement.CurrentMoveState == PlayerMovement.MoveState.Run;

            CheckLandingAndFalling();
            CalculateRotationSpeed();

            Vector3 targetPosition = initialPosition + helmetOffset;
            Quaternion targetRotation = initialRotation;

            if (isBobbing)
            {
                bool isRunning = playerMovement.CurrentMoveState == PlayerMovement.MoveState.Run;

                currentBobAmount = Mathf.Lerp(currentBobAmount, isRunning ? runBobAmount : walkBobAmount, Time.deltaTime * transitionSpeed);
                currentBobFrequency = Mathf.Lerp(currentBobFrequency, isRunning ? runBobFrequency : walkBobFrequency, Time.deltaTime * transitionSpeed);
                currentTiltAmount = Mathf.Lerp(currentTiltAmount, isRunning ? tiltAmount : tiltAmount * 0.5f, Time.deltaTime * transitionSpeed);

                float speedRatio = playerMovement.MovementInput.magnitude;
                bobTimer += Time.deltaTime * currentBobFrequency * speedRatio;

                float verticalBob = Mathf.Sin(bobTimer) * currentBobAmount;
                float horizontalBob = Mathf.Cos(bobTimer / 2) * currentBobAmount * 0.5f;

                targetPosition += new Vector3(horizontalBob, verticalBob, 0);

                float tiltX = -playerMovement.MovementInput.y * currentTiltAmount * 0.2f;
                float tiltZ = -playerMovement.MovementInput.x * currentTiltAmount - rotationSpeed * rotationTiltFactor;

                targetRotation *= Quaternion.Euler(tiltX, 0, tiltZ);
            }
            else
            {
                bobTimer += Time.deltaTime * idleBobFrequency;

                currentBobAmount = Mathf.Lerp(currentBobAmount, idleBobAmount, Time.deltaTime * transitionSpeed);
                currentBobFrequency = Mathf.Lerp(currentBobFrequency, idleBobFrequency, Time.deltaTime * transitionSpeed);

                float verticalBob = Mathf.Sin(bobTimer) * currentBobAmount;
                targetPosition += new Vector3(0, verticalBob, 0);

                float rotationTilt = -rotationSpeed * rotationTiltFactor;
                targetRotation *= Quaternion.Euler(0, 0, rotationTilt);
            }

            ApplyFallingAndLandingEffects(ref targetPosition, ref targetRotation);

            if (float.IsNaN(targetPosition.y))
                targetPosition.y = initialPosition.y + helmetOffset.y;

            transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * transitionSpeed);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * tiltSpeed);
        }

        private void CheckLandingAndFalling()
        {
            bool isGrounded = playerMovement.IsGrounded;
            
            if (isGrounded && !wasGrounded)
            {
                landingEffect = landingBobAmount;
            }
            
            if (!isGrounded && wasGrounded)
            {
                isFalling = true;
            }
            
            fallingEffect = Mathf.Lerp(fallingEffect, 
                !isGrounded ? fallingOffset : 0f, 
                Time.deltaTime * fallRecoverySpeed * (!isGrounded ? 0.5f : 1f));
            
            if (isGrounded)
            {
                isFalling = false;
            }
            
            landingEffect = Mathf.Lerp(landingEffect, 0f, Time.deltaTime * fallRecoverySpeed);
            wasGrounded = isGrounded;
        }

        private void CalculateRotationSpeed()
        {
            float currentYRotation = playerMovement.transform.eulerAngles.y;
            float rotationDelta = Mathf.DeltaAngle(previousYRotation, currentYRotation);
            
            rotationSpeed = Mathf.Lerp(rotationSpeed, rotationDelta * 0.2f, Time.deltaTime * 5f);
            previousYRotation = currentYRotation;
        }

        private void ApplyFallingAndLandingEffects(ref Vector3 position, ref Quaternion rotation)
        {
            position.y += fallingEffect;
            
            if (landingEffect > 0.01f)
            {
                position.y -= landingEffect;
                position.z -= landingEffect * 0.3f;
                rotation *= Quaternion.Euler(landingEffect * 15f, 0, 0);
            }
        }
    }
}
