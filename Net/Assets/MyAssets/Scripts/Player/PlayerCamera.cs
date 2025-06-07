using UnityEngine;
using Zenject;

namespace MUSOAR
{
    public class PlayerCamera : MonoBehaviour
    {
        [Header("Ссылки")]
        [SerializeField] private Camera playerCamera;

        [Header("Настройки вращения")]
        [SerializeField] private float rotationSpeed = 2f;
        [SerializeField] private float minVerticalAngle = -80f;
        [SerializeField] private float maxVerticalAngle = 80f;
        [SerializeField] private Vector3 cameraOffset = new Vector3(0f, 0f, 0f);
        
        private PlayerMovement playerMovement;
        private PlayerRigController playerRigController;
        private InputManager inputManager;

        private Vector2 cameraInput;

        private float currentRotationX;
        private float currentRotationY;

        private bool enableInput = true;
        private bool isDead = false;

        public Camera GetCamera() => playerCamera;

        private void Awake()
        {
            inputManager = InputManager.Instance;
            playerMovement = GetComponentInParent<PlayerMovement>();
            playerRigController = GetComponentInParent<PlayerRigController>();
        }

        private void Start()
        {
            GlobalEventManager.OnShowCursor.Invoke(false);
        }

        private void LateUpdate()
        {
            GetCameraInput();
            ApplyRotation();
            ApplyDeathRotation();
        }

        private void DisableCamera(bool turn)
        {
            enableInput = turn;
            GetCamera().enabled = turn;
        }

        private void OnEnable()
        {
            GlobalEventManager.OnCharacterSwitch.AddListener(DisableCamera);
            GlobalEventManager.OnPauseStateChanged.AddListener(OnPause);
            GlobalEventManager.OnPlayerDie.AddListener(OnPlayerDie);

            //RegisterSaveable();
        }

        private void OnDestroy()
        {
            GlobalEventManager.OnCharacterSwitch.RemoveListener(DisableCamera);
            GlobalEventManager.OnPauseStateChanged.RemoveListener(OnPause);
            GlobalEventManager.OnPlayerDie.AddListener(OnPlayerDie);

            //UnregisterSaveable();
        }

        private void OnPause(bool isPaused)
        {
            enableInput = !isPaused;
        }

        private void OnPlayerDie()
        {
            enableInput = false;
            isDead = true;
        }

        private void GetCameraInput()
        {
            if (enableInput)
            {
                cameraInput = inputManager.GetLookInput();
            }
            else
            {
                cameraInput = Vector2.zero;
            }
        }

        private void ApplyRotation()
        {
            if (isDead) return;

            float mouseX = cameraInput.x * rotationSpeed;
            float mouseY = cameraInput.y * rotationSpeed;

            currentRotationY += mouseX;
            currentRotationX -= mouseY;
            currentRotationX = Mathf.Clamp(currentRotationX, minVerticalAngle, maxVerticalAngle);

            Vector3 headPosition = playerRigController.GetHeadBone().position;
            transform.localPosition = transform.parent.InverseTransformPoint(headPosition);
            transform.localRotation = Quaternion.Euler(currentRotationX, 0f, 0f);
            transform.localPosition += transform.localRotation * cameraOffset;

            playerMovement.transform.rotation = Quaternion.Euler(0f, currentRotationY, 0f);
        }

        private void ApplyDeathRotation()
        {
            if (!isDead) return;

            Transform headBone = playerRigController.GetHeadBone();
            Vector3 headPosition = headBone.position;
            transform.position = headPosition;
            transform.rotation = headBone.rotation * Quaternion.Euler(cameraOffset);
        }
        
        /*
        public void GetSaveData(SaveData saveData)
        {
            saveData.PlayerRotationX = currentRotationX;
            saveData.PlayerRotationY = currentRotationY;
        }

        public void SetSaveData(SaveData saveData)
        {
            currentRotationX = saveData.PlayerRotationX;
            currentRotationY = saveData.PlayerRotationY;
            
            transform.localRotation = Quaternion.Euler(currentRotationX, 0f, 0f);
            playerMovement.transform.rotation = Quaternion.Euler(0f, currentRotationY, 0f);
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
