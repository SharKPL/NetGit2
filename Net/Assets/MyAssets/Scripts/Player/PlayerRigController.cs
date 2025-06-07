using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Animations.Rigging;
using Zenject;
using System.Collections;

namespace MUSOAR
{
    public class PlayerRigController : MonoBehaviour
    {
        [Header("IK компоненты")]
        [SerializeField] private MultiAimConstraint headIK;
        [SerializeField] private MultiAimConstraint upperChestIK;

        [Header("Кости")]
        [SerializeField] private Transform headBone;
        [SerializeField] private Transform upperChestBone;

        [Header("Настройки")]
        [SerializeField] private float disableDelay = 0.25f;

        public Transform GetHeadBone() => headBone;
        public Transform GetUpperChestBone() => upperChestBone;

        private PlayerCamera playerCamera;
        
        private RigBuilder rigBuilder;
        private Rig playerRig;

        [Inject]
        private void Construct(PlayerCamera playerCamera)
        {
            this.playerCamera = playerCamera;
        }

        private void Awake()
        {
            rigBuilder = GetComponent<RigBuilder>();
            playerRig = GetComponentInChildren<Rig>();
        }

        private void OnEnable()
        {
            GlobalEventManager.OnPlayerDie.AddListener(OnPlayerDie);
        }

        private void OnDestroy()
        {
            GlobalEventManager.OnPlayerDie.RemoveListener(OnPlayerDie);
        }

        private void OnPlayerDie()
        {
            StartCoroutine(SmoothDisableRig());
        }

        private IEnumerator SmoothDisableRig()
        {
            float elapsedTime = 0f;
            float startWeight = playerRig.weight;

            while (elapsedTime < disableDelay)
            {
                elapsedTime += Time.deltaTime;
                float normalizedTime = elapsedTime / disableDelay;
                playerRig.weight = Mathf.Lerp(startWeight, 0f, normalizedTime);
                yield return null;
            }

            playerRig.weight = 0f;
        }
    }
}
