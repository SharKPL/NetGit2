using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

namespace MUSOAR
{
    public class SettingsScreenWindow : UIWindow
    {
        [System.Serializable]
        private struct SettingControl
        {
            public Button leftButton;
            public Button rightButton;
            public TMP_Text valueText;
        }

        [Header("Настройки")]
        [SerializeField] private SettingControl resolutionSetting;
        [SerializeField] private SettingControl displayModeSetting;
        [SerializeField] private SettingControl fpsLimitSetting;
        [SerializeField] private SettingControl vSyncSetting;
        [SerializeField] private SettingControl brightnessSetting;
        [SerializeField] private SettingControl fieldOfViewSetting;

        [Header("Кнопки")]
        [SerializeField] private Button backButton;

        private SettingOption<Resolution> resolutionOption;
        private SettingOption<FullScreenMode> displayModeOption;
        private SettingOption<int> fpsLimitOption;
        private SettingOption<bool> vSyncOption;
        private SettingOption<int> brightnessOption;
        private SettingOption<int> fieldOfViewOption;

        private IReturnableWindow menuManager;

        public void Initialize(IReturnableWindow manager)
        {
            menuManager = manager;
            OpenWindow();
        }

        private void Awake()
        {
            backButton.onClick.AddListener(OnBackButtonClick);

            InitializeSettings();
            SetupButtons();
        }

        private void OnBackButtonClick()
        {
            menuManager?.ReturnToPrevious();
            CloseWindow();
        }

        private void InitializeSettings()
        {
            InitializeResolutionSettings();
            InitializeDisplayModeSettings();
            InitializeFpsLimitSettings();
            InitializeVSyncSettings();
            InitializeBrightnessSettings();
            InitializeFieldOfViewSettings();
            
            UpdateUI();
        }

        private void InitializeResolutionSettings()
        {
            Resolution[] resolutions = Screen.resolutions;
            Resolution currentResolution = Screen.currentResolution;
            resolutionOption = new SettingOption<Resolution>(resolutions, currentResolution);
        }

        private void InitializeDisplayModeSettings()
        {
            var displayModes = new[] 
            {
                FullScreenMode.ExclusiveFullScreen,
                FullScreenMode.FullScreenWindow,
                FullScreenMode.Windowed
            };
            displayModeOption = new SettingOption<FullScreenMode>(displayModes, FullScreenMode.ExclusiveFullScreen);
        }

        private void InitializeFpsLimitSettings()
        {
            var fpsLimits = new[] { -1, 15, 30, 60, 75, 90, 120, 144, 165, 240 };
            int currentFps = Application.targetFrameRate;
            fpsLimitOption = new SettingOption<int>(fpsLimits, currentFps != -1 ? currentFps : -1);
        }

        private void InitializeVSyncSettings()
        {
            vSyncOption = new SettingOption<bool>(new[] { false, true }, false);
        }

        private void InitializeBrightnessSettings()
        {
            var brightnessValues = Enumerable.Range(0, 21).Select(x => x * 5).ToArray();
            brightnessOption = new SettingOption<int>(brightnessValues, 50);
        }

        private void InitializeFieldOfViewSettings()
        {
            var fovValues = Enumerable.Range(60, 21).ToArray();
            fieldOfViewOption = new SettingOption<int>(fovValues, 65);
        }

        private void SetupButtons()
        {
            SetupSettingButtons(resolutionSetting, resolutionOption);
            SetupSettingButtons(displayModeSetting, displayModeOption);
            SetupSettingButtons(fpsLimitSetting, fpsLimitOption);
            SetupSettingButtons(vSyncSetting, vSyncOption);
            SetupSettingButtons(brightnessSetting, brightnessOption);
            SetupSettingButtons(fieldOfViewSetting, fieldOfViewOption);
        }

        private void SetupSettingButtons<T>(SettingControl control, SettingOption<T> option)
        {
            control.leftButton.onClick.AddListener(() => 
            {
                option.Previous();
                UpdateUI();
            });
            
            control.rightButton.onClick.AddListener(() => 
            {
                option.Next();
                UpdateUI();
            });
        }

        private void UpdateUI()
        {
            UpdateResolutionUI();
            UpdateDisplayModeUI();
            UpdateFpsLimitUI();
            UpdateVSyncUI();
            UpdateBrightnessUI();
            UpdateFieldOfViewUI();
            
            ForceUpdateLayout();
        }

        private void ForceUpdateLayout()
        {
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
        }

        private void UpdateResolutionUI()
        {
            var resolution = resolutionOption.CurrentValue;
            resolutionSetting.valueText.text = $"{resolution.width}x{resolution.height}";
        }

        private void UpdateDisplayModeUI()
        {
            displayModeSetting.valueText.text = GetDisplayModeName(displayModeOption.CurrentValue);
        }

        private void UpdateFpsLimitUI()
        {
            fpsLimitSetting.valueText.text = fpsLimitOption.CurrentValue == -1 ? "Выкл" : $"{fpsLimitOption.CurrentValue} FPS";
        }

        private void UpdateVSyncUI()
        {
            vSyncSetting.valueText.text = vSyncOption.CurrentValue ? "Включена" : "Выключена";
        }

        private void UpdateBrightnessUI()
        {
            brightnessSetting.valueText.text = $"{brightnessOption.CurrentValue}%";
        }

        private void UpdateFieldOfViewUI()
        {
            fieldOfViewSetting.valueText.text = $"{fieldOfViewOption.CurrentValue}°";
        }

        private string GetDisplayModeName(FullScreenMode mode) => mode switch
        {
            FullScreenMode.ExclusiveFullScreen => "Полноэкранный",
            FullScreenMode.FullScreenWindow => "Полноэкранный в окне",
            FullScreenMode.Windowed => "Оконный",
            _ => "Неизвестно"
        };

        public void ApplySettings()
        {
            var resolution = resolutionOption.CurrentValue;
            Screen.SetResolution(resolution.width, resolution.height, displayModeOption.CurrentValue, resolution.refreshRateRatio);
            
            QualitySettings.vSyncCount = vSyncOption.CurrentValue ? 1 : 0;
            
            if (fpsLimitOption.CurrentValue > 0)
            {
                Application.targetFrameRate = fpsLimitOption.CurrentValue;
            }
            else
            {
                Application.targetFrameRate = -1;
            }

            //TODO: Добавить яркость

            //TODO: Добавить поле зрения
        }
    }
}
