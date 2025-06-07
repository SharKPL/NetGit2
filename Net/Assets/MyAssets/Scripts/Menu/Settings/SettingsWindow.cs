using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace MUSOAR
{
    public class SettingsWindow : UIWindow, IReturnableWindow
    {
        [Header("Кнопки настроек")]
        [SerializeField] private Button screenButton;
        [SerializeField] private Button graphicsButton;
        [SerializeField] private Button audioButton;
        [SerializeField] private Button controlButton;

        [Header("Кнопки действий")]
        [SerializeField] private Button backButton;

        [Header("Панели")]
        [SerializeField] private GameObject settingsTypePanel;

        private IReturnableWindow menuManager;
        private SettingsScreenWindow screenWindow;
        private SettingsGraphicsWindow graphicsWindow;
        private SettingsAudioWindow audioWindow;
        private SettingsControlWindow controlWindow;

        [Inject]
        private void Construct(
            SettingsScreenWindow screenWindow, 
            SettingsGraphicsWindow graphicsWindow, 
            SettingsAudioWindow audioWindow, 
            SettingsControlWindow controlWindow)
        {
            this.screenWindow = screenWindow;
            this.graphicsWindow = graphicsWindow;
            this.audioWindow = audioWindow;
            this.controlWindow = controlWindow;
        }


        public void Initialize(IReturnableWindow manager)
        {
            menuManager = manager;
            
            OpenWindow();
            settingsTypePanel.SetActive(true);
        }

        private void Awake()
        {
            backButton.onClick.AddListener(OnBackButtonClick);
            screenButton.onClick.AddListener(OnScreenButtonClick);
            graphicsButton.onClick.AddListener(OnGraphicsButtonClick);
            audioButton.onClick.AddListener(OnAudioButtonClick);
            controlButton.onClick.AddListener(OnControlButtonClick);
        }

        private void OnScreenButtonClick()
        {
            screenWindow.Initialize(this);
            settingsTypePanel.SetActive(false);
        }

        private void OnGraphicsButtonClick()
        {
            //graphicsWindow.OpenWindow();
        }

        private void OnAudioButtonClick()
        {
            //audioWindow.OpenWindow();
        }

        private void OnControlButtonClick()
        {
            //controlWindow.OpenWindow();
        }

        private void OnBackButtonClick()
        {
            menuManager?.ReturnToPrevious();
            CloseWindow();
            settingsTypePanel.SetActive(false);
        }

        public void OpenSettingsTypePanel()
        {
            settingsTypePanel.SetActive(true);
        }

        public void ReturnToPrevious()
        {
            settingsTypePanel.SetActive(true);
        }
    }
}
