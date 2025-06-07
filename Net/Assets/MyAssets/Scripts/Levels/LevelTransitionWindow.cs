using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Zenject;

namespace MUSOAR
{
    public class LevelTransitionWindow : MonoBehaviour
    {
        [Header("Панель перехода")]
        [SerializeField] private GameObject transitionWindow;
        [SerializeField] private GameObject transitionTimerPanel;

        [Header("Кнопки")]
        [SerializeField] private Button returnToShipButton;
        [SerializeField] private Button toNextLevelButton;

        [Header("Текст")]
        [SerializeField] private TMP_Text timeToTransitionText;

        [Header("Настройки")]
        [SerializeField] private float transitionDelay = 3f;

        private LevelsManager levelsManager;
        private LevelConfig targetLevel;

        private Coroutine transitionTimerCoroutine;

        [Inject]
        private void Construct(LevelsManager levelsManager)
        {
            this.levelsManager = levelsManager;
        }

        private void Awake()
        {
            returnToShipButton.onClick.AddListener(OnReturnToShipButtonClick);
            toNextLevelButton.onClick.AddListener(OnToNextLevelButtonClick);

            transitionWindow.SetActive(false);
            transitionTimerPanel.SetActive(false);
        }

        private void OnReturnToShipButtonClick()
        {
            HideTransitionWindow();
            levelsManager.LoadLevel(LevelData.Level_1);
        }

        private void OnToNextLevelButtonClick()
        {
            HideTransitionWindow();
            levelsManager.LoadLevel(targetLevel.LevelData);
        }

        public void SetStartTransition(LevelConfig targetLevel, bool showTransitionWindow)
        {
            this.targetLevel = targetLevel;
            StartTransitionTimer(showTransitionWindow);
        }

        public void SetEndTransition()
        {
            if (transitionTimerCoroutine != null)
            {
                StopCoroutine(transitionTimerCoroutine);
                transitionTimerCoroutine = null;
            }
            
            HideTransitionWindow();
        }

        private void StartTransitionTimer(bool showTransitionWindow)
        {
            transitionTimerPanel.SetActive(true);
            
            if (transitionTimerCoroutine != null)
            {
                StopCoroutine(transitionTimerCoroutine);
            }
            
            transitionTimerCoroutine = StartCoroutine(TransitionTimerCoroutine(showTransitionWindow));
        }

        private IEnumerator TransitionTimerCoroutine(bool showTransitionWindow)
        {
            float remainingTime = transitionDelay;
            
            while (remainingTime > 0)
            {
                timeToTransitionText.text = $"Переход через {Mathf.CeilToInt(remainingTime)} секунд";
                remainingTime -= Time.unscaledDeltaTime;
                yield return null;
            }
            
            transitionTimerPanel.SetActive(false);
            
            if (showTransitionWindow)
            {
                ShowTransitionWindow();
            }
            else
            {
                levelsManager.LoadLevel(targetLevel.LevelData);
            }
        }

        private void ShowTransitionWindow()
        {
            transitionWindow.SetActive(true);
            GlobalEventManager.OnPauseStateChanged.Invoke(true);
        }

        private void HideTransitionWindow()
        {           
            transitionWindow.SetActive(false);
            transitionTimerPanel.SetActive(false);
            GlobalEventManager.OnShowCursor.Invoke(false);
        }
    }
}
