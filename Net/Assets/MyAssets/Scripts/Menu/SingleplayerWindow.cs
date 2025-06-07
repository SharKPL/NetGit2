using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace MUSOAR
{
    public class SingleplayerWindow : UIWindow, IReturnableWindow
    {
        [Header("Кнопки")]
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button loadingButton;
        [SerializeField] private Button backButton;

        private IReturnableWindow menuManager;
        private LevelsManager levelsManager;
        private SaveController saveController;
        private SaveLoadWindow saveLoadWindow;

        [Inject]
        private void Construct(LevelsManager levelsManager, SaveController saveController, SaveLoadWindow saveLoadWindow)
        {
            this.levelsManager = levelsManager;
            this.saveController = saveController;
            this.saveLoadWindow = saveLoadWindow;
        }

        public void Initialize(IReturnableWindow manager)
        {
            menuManager = manager;
            CheckSave();
            OpenWindow();
        }

        private void Awake()
        {
            newGameButton.onClick.AddListener(OnNewGameButtonClick);
            continueButton.onClick.AddListener(OnContinueButtonClick);
            loadingButton.onClick.AddListener(OnLoadingButtonClick);
            backButton.onClick.AddListener(OnBackButtonClick);
        }

        private void CheckSave()
        {
            if (!saveController.HasSaveFile())
            {
                continueButton.gameObject.SetActive(false);
                loadingButton.gameObject.SetActive(false);
            }
            else
            {
                continueButton.gameObject.SetActive(true);
                loadingButton.gameObject.SetActive(true);
            }
        }

        private void OnNewGameButtonClick()
        {
            levelsManager.LoadLevel(LevelData.Level_1);
        }

        private void OnContinueButtonClick()
        {
            saveController.LoadSaveLevel();
        }

        private void OnLoadingButtonClick()
        {
            saveLoadWindow.InitializeLoadWindow(this);
            CloseWindow();
        }

        private void OnBackButtonClick()
        {
            menuManager?.ReturnToPrevious();
            CloseWindow();
        }

        public void ReturnToPrevious()
        {
            CheckSave();
            OpenWindow();
        }
    }
}
