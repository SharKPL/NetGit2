using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Zenject;
using System;
using System.Collections.Generic;

namespace MUSOAR
{
    public class SaveLoadWindow : UIWindow
    {
        [Header("Кнопки внутри окна")]
        [SerializeField] private Button saveButton;
        [SerializeField] private Button deleteSaveButton;
        [SerializeField] private Button loadButton;
        [SerializeField] private Button backButton;

        [Header("Префабы")]
        [SerializeField] private Transform saveContent;
        [SerializeField] private GameObject saveSlotPanel;
        
        [Header("Элементы UI")]
        [SerializeField] private Image saveIcon;
        [SerializeField] private TMP_Text saveNameText;
        [SerializeField] private TMP_Text saveDateText;
        [SerializeField] private TMP_Text savePlayTimeText;

        private IReturnableWindow menuManager;
        private SaveController saveController;
        private SaveData currentSelectedSave;
        private List<GameObject> saveSlots = new List<GameObject>();

        private const string DATE_FORMAT = "dd.MM.yyyy HH:mm";

        private bool isSaveMode;
        
        [Inject]
        private void Construct(SaveController saveController)
        {
            this.saveController = saveController;
        }

        private void Awake()
        {
            saveButton.onClick.AddListener(OnSaveButtonClick);
            deleteSaveButton.onClick.AddListener(OnDeleteSaveButtonClick);
            loadButton.onClick.AddListener(OnLoadButtonClick);
            backButton.onClick.AddListener(OnBackButtonClick);
        }

        public void InitializeSaveWindow(IReturnableWindow manager)
        {
            menuManager = manager;
            isSaveMode = true;
            InitializeWindow();
            saveButton.gameObject.SetActive(true);
        }

        public void InitializeLoadWindow(IReturnableWindow manager)
        {
            menuManager = manager;
            isSaveMode = false;
            InitializeWindow();
        }

        private void InitializeWindow()
        {
            OpenWindow();
            DisableAllButtons();
            saveIcon.enabled = false;
            LoadSaveSlots();
        }

        private void OnSaveButtonClick()
        {
            saveController.SaveGame();
            LoadSaveSlots();
        }

        private void OnDeleteSaveButtonClick()
        {
            if (currentSelectedSave != null)
            {
                saveController.DeleteSave(currentSelectedSave);
                LoadSaveSlots();
                ClearSaveInfoPanel();
            }
        }

        private void OnLoadButtonClick()
        {
            if (currentSelectedSave != null)
            {
                CurrentSelectedSave.SelectedSave = currentSelectedSave;
                saveController.LoadSaveLevel();
            }      
        }

        private void OnBackButtonClick()
        {
            menuManager?.ReturnToPrevious();
            CloseWindow();
        }

        private void LoadSaveSlots()
        {
            ClearSaveSlots();
            List<SaveData> saves = saveController.GetAllSaves();
            
            foreach (var saveData in saves)
            {
                CreateSaveSlot(saveData);
            }
        }

        private void ClearSaveSlots()
        {
            foreach (var slot in saveSlots)
            {
                Destroy(slot);
            }
            saveSlots.Clear();

            currentSelectedSave = null;
            ClearSaveInfoPanel();
        }

        private void ClearSaveInfoPanel()
        {
            saveNameText.text = " ";
            saveDateText.text = " ";
            savePlayTimeText.text = " ";
            saveIcon.enabled = false;
        }

        private void CreateSaveSlot(SaveData saveData)
        {
            GameObject slotObject = Instantiate(saveSlotPanel, saveContent);
            slotObject.SetActive(true);
            saveSlots.Add(slotObject);
            
            SaveSlotInfo slotInfo = slotObject.GetComponent<SaveSlotInfo>();
            if (slotInfo != null)
            {
                slotInfo.SaveNameText.text = saveData.SaveName;
                slotInfo.SaveDateText.text = saveData.SaveTimestamp.ToString(DATE_FORMAT);
                slotInfo.SaveTimeText.text = FormatPlayTime(saveData.TotalTimePlayed);
            }
            
            SaveData save = saveData;
            slotObject.GetComponent<Button>().onClick.AddListener(() => OnSaveSlotSelected(save));
        }

        private string FormatPlayTime(float seconds)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(seconds);
            return $"{timeSpan.Hours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
        }

        private void OnSaveSlotSelected(SaveData saveData)
        {
            currentSelectedSave = saveData;
            
            if (saveData != null)
            {
                UpdateSaveInfoPanel(saveData);
                UpdateButtonsVisibility();
            }
        }

        private void UpdateSaveInfoPanel(SaveData saveData)
        {
            saveNameText.text = $"Имя сохранения: {saveData.SaveName}";
            saveDateText.text = $"Дата создания: {saveData.SaveTimestamp.ToString(DATE_FORMAT)}";
            savePlayTimeText.text = $"Общее время: {FormatPlayTime(saveData.TotalTimePlayed)}";
            
            UpdateSaveScreenshot(saveData);
        }

        private void UpdateSaveScreenshot(SaveData saveData)
        {
            if (!string.IsNullOrEmpty(saveData.ScreenshotBase64))
            {
                try
                {
                    byte[] imageBytes = Convert.FromBase64String(saveData.ScreenshotBase64);
                    Texture2D tex = new Texture2D(2, 2);
                    if (tex.LoadImage(imageBytes))
                    {
                        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                        saveIcon.sprite = sprite;
                        saveIcon.enabled = true;
                    }
                    else
                    {
                        saveIcon.enabled = false;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Ошибка при загрузке скриншота: {e.Message}");
                    saveIcon.enabled = false;
                }
            }
            else
            {
                saveIcon.enabled = false;
            }
        }

        private void UpdateButtonsVisibility()
        {
            if (isSaveMode)
            {
                deleteSaveButton.gameObject.SetActive(true);
            }
            else
            {
                loadButton.gameObject.SetActive(true);
                deleteSaveButton.gameObject.SetActive(true);
            }
        }

        private void DisableAllButtons()
        {
            saveButton.gameObject.SetActive(false);
            deleteSaveButton.gameObject.SetActive(false);
            loadButton.gameObject.SetActive(false);
        }
    }
}
