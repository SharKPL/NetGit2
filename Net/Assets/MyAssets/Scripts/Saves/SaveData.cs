using System;
using UnityEngine;

namespace MUSOAR
{
    [Serializable]
    public class SaveData
    {
        // ===== Данные уровня =====
        public LevelData CurrentLevel;

        // ===== Данные игрока =====
        public Vector3 PlayerPosition;
        public float PlayerRotationX;
        public float PlayerRotationY;
        public float CurrentSuitEnergy;
        public float CurrentHeat;
        public float CurrentOxygen;
        public float CurrentHealth;

        // ===== Данные сохранения =====
        public int SaveNumber;
        public string SaveName;
        public DateTime SaveTimestamp;
        public float TotalTimePlayed;
        public string ScreenshotBase64;
        
        public SaveData()
        {
            // ===== Данные уровня =====
            CurrentLevel = LevelData.MainMenu;

            // ===== Данные игрока =====
            PlayerPosition = Vector3.zero;
            PlayerRotationX = 0f;
            PlayerRotationY = 0f;
            CurrentSuitEnergy = 0f;
            CurrentHeat = 0f;
            CurrentOxygen = 0f;
            CurrentHealth = 0f;

            // ===== Данные сохранения =====
            SaveNumber = 0;
            SaveName = string.Empty;
            SaveTimestamp = DateTime.Now;
            TotalTimePlayed = 0f;
            ScreenshotBase64 = string.Empty;
        }
    }

    public static class CurrentSelectedSave
    {
        public static SaveData SelectedSave;
        
        public static void Clear()
        {
            SelectedSave = null;
            Debug.Log("Текущее выбранное сохранение очищено!");
        }
        
        public static bool HasSelectedSave()
        {
            return SelectedSave != null;
        }
    }
}
