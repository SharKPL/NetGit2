using UnityEngine;
using System.Threading.Tasks;

namespace MUSOAR
{
    public class BlackScreen : MonoBehaviour
    {
        [SerializeField] private CanvasGroup blackScreenCanvasGroup;
        [SerializeField] private float fadeDuration = 0.5f;

        public async Task FadeIn()
        {
            float elapsedTime = 0;
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                blackScreenCanvasGroup.alpha = elapsedTime / fadeDuration;
                await Task.Yield();
            }
            blackScreenCanvasGroup.alpha = 1;
        }

        public async Task FadeOut()
        {
            float elapsedTime = 0;
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                blackScreenCanvasGroup.alpha = 1 - (elapsedTime / fadeDuration);
                await Task.Yield();
            }
            blackScreenCanvasGroup.alpha = 0;
        }
    }
}
