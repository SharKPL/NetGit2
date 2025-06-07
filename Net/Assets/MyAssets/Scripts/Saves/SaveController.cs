using System.IO;
using UnityEngine;
using Zenject;
using System;
using System.Collections.Generic;

namespace MUSOAR
{
    public class SaveController : IInitializable
    {
        private const string SAVES_FOLDER = "Saves";
        
        private LevelsManager levelsManager;
        private string saveFolderPath;

        [Inject]
        private void Construct(LevelsManager levelsManager)
        {
            this.levelsManager = levelsManager;
            saveFolderPath = Path.Combine(Application.persistentDataPath, SAVES_FOLDER);
        }
        
        public void Initialize()
        {
            if (!Directory.Exists(saveFolderPath))
                Directory.CreateDirectory(saveFolderPath);

            GlobalEventManager.OnLevelLoaded.AddListener(LoadSaveData); // Чтобы после загрузки уровня применялись данные сохранения
            Debug.Log("SaveController initialized");
        }
        
        public void SaveGame() // Сохраняем игру
        {
            int saveNumber = GetAllSaves().Count + 1;
            
            SaveData saveData = new SaveData
            {
                SaveNumber = saveNumber,
                SaveTimestamp = DateTime.Now,
                ScreenshotBase64 = CaptureScreenshot(),
                SaveName = $"Сохранение #{saveNumber}"
            };
            
            var currentLevel = levelsManager.GetCurrentLevel();
            if (currentLevel != null)
                saveData.CurrentLevel = currentLevel.LevelData;
            
            foreach (var saveable in SaveableRegistry.GetAllSaveables())
            {
                saveable.GetSaveData(saveData);
            }
            
            string fileName = $"save_{saveNumber}_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.json";
            string path = Path.Combine(saveFolderPath, fileName);
            File.WriteAllText(path, JsonUtility.ToJson(saveData, true));
            
            Debug.Log($"Игра сохранена в: {path}");
        }

        public void DeleteSave(SaveData saveData)
        {
            if (saveData == null) return;
        
            string[] saveFiles = Directory.GetFiles(saveFolderPath, $"save_{saveData.SaveNumber}_*.json");
        
            if (saveFiles.Length > 0)
            {
                File.Delete(saveFiles[0]);
                Debug.Log($"Сохранение удалено: {saveFiles[0]}");
            }
            else
            {
                Debug.LogWarning($"Файл сохранения для удаления не найден: #{saveData.SaveNumber}");
            }
        }

        public void LoadSaveLevel() // Грузим уровень из сохранения
        {
            SaveData saveData = LoadSaveDataFromDisk();
            
            if (saveData != null)
            {
                levelsManager.LoadLevel(saveData.CurrentLevel);
            }
        }
        
        private void LoadSaveData() // Применяем данные сохранения к игре после загрузки уровня
        {
            SaveData saveData = LoadSaveDataFromDisk();
            
            if (saveData != null)
            {
                foreach (var saveable in SaveableRegistry.GetAllSaveables())
                {
                    saveable.SetSaveData(saveData);
                }
                
                GlobalEventManager.OnLevelLoaded.RemoveListener(LoadSaveData);
                CurrentSelectedSave.Clear();
                Debug.Log($"Игра загружена успешно (сохранение от {saveData.SaveTimestamp})");
            }
        }

        private SaveData LoadSaveDataFromDisk() // Получаем сохранение с диска
        {
            if (CurrentSelectedSave.HasSelectedSave()) // Если есть выбранное сохранение
            {
                Debug.Log($"Загружен выбранный файл сохранения (от {CurrentSelectedSave.SelectedSave.SaveTimestamp})");
                return CurrentSelectedSave.SelectedSave;
            }
        
            List<SaveData> saves = GetAllSaves(); // Если нет выбранного сохранения, то загружаем последнее сохранение
            
            if (saves.Count == 0)
            {
                Debug.LogWarning("Сохранения не найдены!");
                return null;
            }
            
            SaveData saveData = saves[0];
            Debug.Log($"Загружен файл сохранения по умолчанию (от {saveData.SaveTimestamp})");
            return saveData;     
        }

        public List<SaveData> GetAllSaves() // Получаем все сохранения
        {
            List<SaveData> saves = new List<SaveData>();
            
            if (!HasSaveFile()) return saves;
            
            string[] saveFiles = Directory.GetFiles(saveFolderPath, "save_*.json");
            System.Array.Sort(saveFiles, (a, b) => File.GetLastWriteTime(b).CompareTo(File.GetLastWriteTime(a)));
            
            foreach (string filePath in saveFiles)
            {
                try
                {
                    SaveData saveData = JsonUtility.FromJson<SaveData>(File.ReadAllText(filePath));
                    if (saveData != null)
                    {
                        saves.Add(saveData);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Ошибка при загрузке файла сохранения {filePath}: {e.Message}");
                }
            }
            
            return saves;
        }
        
        private string CaptureScreenshot()
        {
            int width = 320, height = 320;
            RenderTexture rt = new RenderTexture(width, height, 24);
            
            GameObject tempCameraObj = new GameObject("ScreenshotCamera");
            Camera screenshotCamera = tempCameraObj.AddComponent<Camera>();
            
            if (Camera.main != null)
            {
                screenshotCamera.CopyFrom(Camera.main);
                screenshotCamera.transform.position = Camera.main.transform.position;
                screenshotCamera.transform.rotation = Camera.main.transform.rotation;
            }
            
            screenshotCamera.targetTexture = rt;
            Texture2D screenshot = new Texture2D(width, height, TextureFormat.RGB24, false);
            screenshotCamera.Render();
            RenderTexture.active = rt;
            screenshot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            
            screenshotCamera.targetTexture = null;
            RenderTexture.active = null;
            UnityEngine.Object.Destroy(rt);
            UnityEngine.Object.Destroy(tempCameraObj);
            
            byte[] bytes = screenshot.EncodeToJPG(50);
            UnityEngine.Object.Destroy(screenshot);
            
            return Convert.ToBase64String(bytes);
        }
        
        public bool HasSaveFile() => Directory.Exists(saveFolderPath) && Directory.GetFiles(saveFolderPath, "save_*.json").Length > 0;
    }
}
