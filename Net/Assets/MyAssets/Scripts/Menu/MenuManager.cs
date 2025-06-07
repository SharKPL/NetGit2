using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace MUSOAR
{
    public class MenuManager : UIWindow, IReturnableWindow
    {
        [Header("Кнопки")]
        [SerializeField] private Button playSingleButton;
        [SerializeField] private Button playMultiButton;
        [SerializeField] private Button creditsButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button exitButton;

        private SingleplayerWindow singleplayerWindow;
        private MultiplayerWindow multiplayerWindow;
        private CreditsWindow creditsWindow;
        private SettingsWindow settingsWindow;

        [Inject]
        private void Construct(SingleplayerWindow singleplayerWindow, MultiplayerWindow multiplayerWindow, CreditsWindow creditsWindow, SettingsWindow settingsWindow)
        {
            this.singleplayerWindow = singleplayerWindow;
            this.multiplayerWindow = multiplayerWindow;
            this.creditsWindow = creditsWindow;
            this.settingsWindow = settingsWindow;
        }

        private void Awake()
        {
            playSingleButton.onClick.AddListener(OnPlaySingleButtonClick);
            playMultiButton.onClick.AddListener(OnPlayMultiButtonClick);
            creditsButton.onClick.AddListener(OnCreditsButtonClick);
            settingsButton.onClick.AddListener(OnSettingsButtonClick);
            exitButton.onClick.AddListener(OnExitButtonClick);
        }

        private void OnPlaySingleButtonClick()
        {
            singleplayerWindow.Initialize(this);
            CloseWindow();
        }

        private void OnPlayMultiButtonClick()
        {
            multiplayerWindow.Initialize(this);
            CloseWindow();
        }

        private void OnCreditsButtonClick()
        {
            creditsWindow.Initialize(this);
            CloseWindow();
        }

        private void OnSettingsButtonClick()
        {
            settingsWindow.Initialize(this);
            CloseWindow();
        }

        private void OnExitButtonClick()
        {
            Application.Quit();
        }

        public void ReturnToPrevious()
        {
            OpenWindow();
        }
    }
}

