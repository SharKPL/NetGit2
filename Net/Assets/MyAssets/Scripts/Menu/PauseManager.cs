    using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Zenject;

namespace MUSOAR
{
    public class PauseManager : UIWindow, IReturnableWindow
    {
        [Header("Кнопки")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button saveWindowButton;
        [SerializeField] private Button loadWindowButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button exitButton;

        [Header("Панель")]
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject blackBackground;

        private InputManager inputManager;
        private SettingsWindow settingsWindow;
        private SettingsScreenWindow settingsScreenWindow;
        private LevelsManager levelsManager;
        private SaveLoadWindow saveLoadWindow;
        private SaveController saveController;

        private bool isPaused = false;
        private bool isUsingWorkbench = false;
        [Inject]
        private void Construct(InputManager inputManager, 
            SettingsWindow settingsWindow, 
            SettingsScreenWindow settingsScreenWindow, 
            LevelsManager levelsManager, 
            SaveLoadWindow saveLoadWindow, 
            SaveController saveController)
        {
            this.inputManager = inputManager;
            this.settingsWindow = settingsWindow;
            this.settingsScreenWindow = settingsScreenWindow;
            this.levelsManager = levelsManager;
            this.saveLoadWindow = saveLoadWindow;
            this.saveController = saveController;
        }

        private void Awake()
        {
            resumeButton.onClick.AddListener(OnResumeButtonClick);
            saveWindowButton.onClick.AddListener(OnSaveWindowButtonClick);
            loadWindowButton.onClick.AddListener(OnLoadWindowButtonClick);
            settingsButton.onClick.AddListener(OnSettingsButtonClick);
            exitButton.onClick.AddListener(OnExitButtonClick);

            CheckSave();
        }

        private void Update()
        {
            if (inputManager.IsPause() && !isUsingWorkbench)
            {
                if (settingsScreenWindow.IsOpen)
                {
                    settingsScreenWindow.CloseWindow();
                    settingsWindow.OpenSettingsTypePanel();
                }
                else if (settingsWindow.IsOpen)
                {
                    settingsWindow.CloseWindow();
                    pausePanel.SetActive(true);
                    CheckSave();
                }
                else if (saveLoadWindow.IsOpen)
                {
                    saveLoadWindow.CloseWindow();
                    pausePanel.SetActive(true);
                    CheckSave();
                }
                else
                {
                    OnOpenPauseMenu();
                }
            }
        }

        private void OnEnable()
        {
            //GlobalEventManager.OnPlayerUseWorkbench.AddListener(OnPlayerUseWorkbench);
        }

        private void OnDisable()
        {
            //GlobalEventManager.OnPlayerUseWorkbench.RemoveListener(OnPlayerUseWorkbench);
        }

        private void OnPlayerUseWorkbench(bool isUsing)
        {
            isUsingWorkbench = isUsing;
        }

        private void OnOpenPauseMenu()
        {
            isPaused = !isPaused;
            pausePanel.SetActive(isPaused);
            blackBackground.SetActive(isPaused);
            GlobalEventManager.OnPauseStateChanged.Invoke(isPaused);
        }

        private void OnResumeButtonClick()
        {
            OnOpenPauseMenu();
        }

        private void OnSaveWindowButtonClick()
        {
            saveLoadWindow.InitializeSaveWindow(this);
            pausePanel.SetActive(false);
        }

        private void OnLoadWindowButtonClick()
        {
            saveLoadWindow.InitializeLoadWindow(this);
            pausePanel.SetActive(false);
        }

        private void OnSettingsButtonClick()
        {
            settingsWindow.Initialize(this);
            pausePanel.SetActive(false);
        }

        private void OnExitButtonClick()
        {
            levelsManager.LoadLevel(LevelData.MainMenu);
        }

        private void CheckSave()
        {
            if (!saveController.HasSaveFile())
            {
                loadWindowButton.interactable = false;
            }
            else
            {
                loadWindowButton.interactable = true;
            }
        }

        public void ReturnToPrevious()
        {
            CheckSave();
            pausePanel.SetActive(true);
        }
    }
}
