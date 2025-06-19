using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class EndGameFade : MonoBehaviour
{
    [SerializeField] private TMP_Text text;

    [SerializeField] private Image darkenImage; 
    [SerializeField] private float fadeDuration = 1.5f; 
    [SerializeField] private Color darkenColor = new Color(0, 0, 0, 1); 

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        if (darkenImage == null)
            darkenImage = GetComponent<Image>();

        canvasGroup = GetComponentInParent<CanvasGroup>();

        GlobalEventManager.ShowEndGameText.AddListener(ShowEnd);
        text.gameObject.SetActive(false);
        darkenImage.gameObject.SetActive(false);
    }


    private void ShowEnd(string text)
    {
        StartCoroutine(FadeTo(1, fadeDuration,text));
    }

    private IEnumerator FadeTo(float targetAlpha, float duration, string text)
    {
        darkenImage.gameObject.SetActive(true);
        float startAlpha = darkenImage.color.a;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);

            SetAlpha(alpha);

            yield return null;
        }

        SetAlpha(targetAlpha);

        this.text.text = text;
        this.text.gameObject.SetActive(true);
    }


    private void SetAlpha(float alpha)
    {
        Color color = darkenImage.color;
        color.a = alpha;
        darkenImage.color = color;
    }
}
