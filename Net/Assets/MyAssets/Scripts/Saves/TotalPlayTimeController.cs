using UnityEngine;
using Zenject;
using System;

namespace MUSOAR
{
    public class TotalPlayTimeController : IInitializable, ITickable, IDisposable, ISaveable
    {
        private float totalPlayTime;

        public void Initialize()
        {
            RegisterSaveable();
            Debug.Log("TotalPlayTimeController инициализирован");
        }

        public void Tick()
        {         
            if (Time.timeScale > 0) 
            {
                UpdatePlayTime();
            }
        }

        public void Dispose()
        {
            UnregisterSaveable();
        }

        private void UpdatePlayTime()
        {
            totalPlayTime += Time.deltaTime;
        }

        public string GetFormattedPlayTime()
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(totalPlayTime);
            return $"{timeSpan.Hours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
        }
 
        // Save/Load
        public void GetSaveData(SaveData saveData)
        {
            saveData.TotalTimePlayed = totalPlayTime;
        }

        public void SetSaveData(SaveData saveData)
        {
            totalPlayTime = saveData.TotalTimePlayed;
            Debug.Log($"Загружено общее время игры: {GetFormattedPlayTime()}");
        }

        public void RegisterSaveable()
        {
            SaveableRegistry.Register(this);
        }

        public void UnregisterSaveable()
        {
            SaveableRegistry.Unregister(this);
        }
    }
}
