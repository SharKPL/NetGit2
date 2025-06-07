using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace MUSOAR
{
    public class UIControl : MonoBehaviour
    {
        [SerializeField] private GameObject playerUI;
        [SerializeField] private GameObject companionUI;

        [SerializeField] private GameObject currentUI;

        [SerializeField] private GameObject miniMapUI;
        [SerializeField] private GameObject dotUI;
        [SerializeField] private GameObject deathUI;

        [SerializeField] private GameObject questUI;
        [SerializeField] private GameObject inventoryQuickSlotUI;

        private bool isPlayerDead;

        private bool isPlayerUI=true;

        private void Awake()
        {
            currentUI = playerUI;
        }

        private void OnEnable()
        {
            GlobalEventManager.OnCharacterSwitch.AddListener(SwitchUI);

            GlobalEventManager.OnPauseStateChanged.AddListener(SetUIState);
            GlobalEventManager.OnPlayerDie.AddListener(OnPlayerDie);
            GlobalEventManager.OnPlayerUseWorkbench.AddListener(OnPlayerUseLevelSelector);
        }

        private void OnDisable()
        {
            GlobalEventManager.OnCharacterSwitch.RemoveListener(SwitchUI);

            GlobalEventManager.OnPauseStateChanged.RemoveListener(SetUIState);
            GlobalEventManager.OnPlayerDie.RemoveListener(OnPlayerDie);
            GlobalEventManager.OnPlayerUseWorkbench.RemoveListener(OnPlayerUseLevelSelector);
        }

        private void SwitchUI(bool turn)
        {
            playerUI.SetActive(isPlayerUI);
            companionUI.SetActive(!isPlayerUI);
            currentUI = isPlayerUI ? playerUI : companionUI;
            isPlayerUI = !isPlayerUI;
 

        }

        private void SetUIState(bool isPaused)
        {
            if (isPlayerDead) return;

            if (isPaused)
            {
                DisableUI();
            }
            else
            {
                EnableUI();
            }
        }

        private void OnPlayerDie()
        {
            isPlayerDead = true;
            DisableUI();
            StartCoroutine(ShowDeathUI());
        }

        private void OnPlayerUseLevelSelector(bool isUsing)
        {
            if (isUsing)
            {
                DisableUI();
            }
            else
            {
                EnableUI();
            }
        }

        private void EnableUI()
        {
            currentUI.SetActive(true);
            miniMapUI.SetActive(true);
            dotUI.SetActive(true);
            questUI.SetActive(true);
            inventoryQuickSlotUI.SetActive(true);
        }

        private void DisableUI()
        {
            currentUI.SetActive(false);
            miniMapUI.SetActive(false);
            dotUI.SetActive(false);
            questUI.SetActive(false);
            inventoryQuickSlotUI.SetActive(false);
        }

        private IEnumerator ShowDeathUI()
        {
            yield return new WaitForSeconds(4.5f);
            GlobalEventManager.OnShowCursor.Invoke(true);
            GlobalEventManager.OnPauseStateChanged.Invoke(true);
            deathUI.SetActive(true);
        }
    }
}
