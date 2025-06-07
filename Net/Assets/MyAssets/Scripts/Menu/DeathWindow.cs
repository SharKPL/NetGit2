using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace MUSOAR
{
    public class DeathWindow : UIWindow, IReturnableWindow
    {
        [Header("Панель")]
        [SerializeField] private GameObject deathPanel;

        [Header("Кнопки")]
        [SerializeField] private Button loadingButton;
        [SerializeField] private Button backMenuButton;

        private LevelsManager levelsManager;
        private SaveLoadWindow saveLoadWindow;
        private SaveController saveController;
        
        [Inject]
        private void Construct(LevelsManager levelsManager, SaveLoadWindow saveLoadWindow, SaveController saveController)
        {
            this.levelsManager = levelsManager;
            this.saveLoadWindow = saveLoadWindow;
            this.saveController = saveController;
        }

        private void Awake()
        {
            loadingButton.onClick.AddListener(OnLoadingButtonClick);
            backMenuButton.onClick.AddListener(OnBackMenuButtonClick);

            SetLoadingButtonState();
        }

        private void OnLoadingButtonClick()
        {
            saveLoadWindow.InitializeLoadWindow(this);
            deathPanel.SetActive(false);
        }

        private void OnBackMenuButtonClick()
        {
            levelsManager.LoadLevel(LevelData.MainMenu);
        }

        private void SetLoadingButtonState()
        {
            if (!saveController.HasSaveFile())
            {
                loadingButton.interactable = false;
            }
        }

        public void ReturnToPrevious()
        {
            deathPanel.SetActive(true);
        }
    }
}

