using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Zenject;

namespace MUSOAR
{
    public class LoadingScreenController : MonoBehaviour
    {
        [Header("Ссылки")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Camera gameCamera;
        [SerializeField] private TMP_Text loadingText;
        [SerializeField] private Slider loadingSlider;

        [Header("Параметры")]
        [SerializeField] private int delayBeforeLoadingNewScene = 1000; // 1 секунда
        [SerializeField] private float fadeDuration = 1f;

        private SaveController saveController;
        
        private string sceneToLoad;

        [Inject]
        private void Construct(SaveController saveController)
        {
            this.saveController = saveController;
        }

        public void InitializeLoading(string sceneName)
        {
            sceneToLoad = sceneName;
            StartLoadingProcess();
        }

        private async void StartLoadingProcess()
        {
            if (string.IsNullOrEmpty(sceneToLoad)) return;

            await PrepareLoadingScreen();
            await LoadNewScene();
            await CleanupLoadingScreen();
        }

        private async Task PrepareLoadingScreen()
        {
            EnableCamera(false);
            UpdateLoadingProgress(0);
            await FadeIn();

            Scene currentScene = SceneManager.GetActiveScene();
            if (currentScene.name != LevelData.LoadingScreen.ToString())
            {
                await SceneManager.UnloadSceneAsync(currentScene);
            }
            EnableCamera(true);
            await Task.Delay(delayBeforeLoadingNewScene);
        }

        private async Task LoadNewScene()
        {
            var loadOperation = SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive);
            loadOperation.allowSceneActivation = false;

            while (loadOperation.progress < 0.9f)
            {
                float progressPercent = loadOperation.progress * 100f / 0.9f;
                UpdateLoadingProgress(progressPercent);
                await Task.Yield();
            }

            UpdateLoadingProgress(100);
            loadOperation.allowSceneActivation = true;
            
            while (!loadOperation.isDone)
            {
                await Task.Yield();
            }

            SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneToLoad));
        }

        private async Task CleanupLoadingScreen()
        {
            EnableCamera(false);
            GlobalEventManager.OnLevelLoaded.Invoke(); // Для save/load системы

            await FadeOut();
            await SceneManager.UnloadSceneAsync(LevelData.LoadingScreen.ToString());
        }

        private async Task FadeIn()
        {
            float elapsedTime = 0;
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                canvasGroup.alpha = elapsedTime / fadeDuration;
                await Task.Yield();
            }
            canvasGroup.alpha = 1f;
        }

        private async Task FadeOut()
        {
            float elapsedTime = 0;
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                canvasGroup.alpha = 1f - (elapsedTime / fadeDuration);
                await Task.Yield();
            }
            canvasGroup.alpha = 0f;
        }


        private void EnableCamera(bool isEnable)
        {
            gameCamera.gameObject.SetActive(isEnable);
        }

        private void UpdateLoadingProgress(float progress)
        {
            float normalizedProgress = Mathf.Clamp(progress, 0f, 100f);
            loadingSlider.value = normalizedProgress;
            loadingText.text = $"{Mathf.RoundToInt(normalizedProgress)}%";
        }
    }
}
