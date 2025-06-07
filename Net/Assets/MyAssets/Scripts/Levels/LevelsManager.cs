using System;
using UnityEngine;
using Zenject;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace MUSOAR
{
    public class LevelsManager
    {
        private AllLevelsConfig allLevelsConfig;
        private LevelConfig currentLevel;
        private LevelConfig levelToLoad;

        [Inject]
        private void Construct(AllLevelsConfig allLevelsConfig)
        {
            this.allLevelsConfig = allLevelsConfig;
        }

        public void LoadLevel(LevelData levelData) // Используйте для загрузки уровня
        {
            Debug.Log(levelData);
            var level = GetLevelByLevelData(levelData);
            LoadLevel(level);
        }

        private void LoadLevel(LevelConfig levelConfig)
        {
            if (levelConfig == null) return;

            currentLevel = levelConfig;
            SceneManager.LoadSceneAsync(LevelData.LoadingScreen.ToString(), LoadSceneMode.Additive).completed += operation =>
            {
                var loadingController = GameObject.FindAnyObjectByType<LoadingScreenController>(); 
                loadingController.InitializeLoading(levelConfig.LevelData.ToString());
            };
        }

        public LevelConfig GetCurrentLevel()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            LevelData currentLevelData;
            if (Enum.TryParse(activeScene.name, out currentLevelData))
            {
                Debug.Log($"GetCurrentLevel: {currentLevelData}");
                return GetLevelByLevelData(currentLevelData);
            }
            Debug.LogWarning($"Не удалось определить текущий уровень из сцены: {activeScene.name}");
            return null;
        }

        public LevelConfig GetLevelByLevelData(LevelData levelData)
        {
            return allLevelsConfig.Levels.Find(level => level.LevelData == levelData);
        }

        public List<LevelConfig> GetAllLevels()
        {
            return allLevelsConfig.Levels;
        }
    }
}

