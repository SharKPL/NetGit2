using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class EndGameFade : MonoBehaviour
{
    public static EndGameFade Instance { get; private set; }

    [SerializeField] private TMP_Text text;
    [SerializeField] private Image darkenImage;
    [SerializeField] private float fadeDuration = 1.5f;
    [SerializeField] private float transitionDuration = 3f;
    [SerializeField] private Color darkenColor = new Color(0, 0, 0, 1);
    [SerializeField] private bool showText;

    private void Awake()
    {
        Instance = this;

        if (darkenImage == null)
            darkenImage = GetComponent<Image>();

        SetAlpha(1); // Начать с полного затемнения
        darkenImage.gameObject.SetActive(true);

        if (showText)
            GlobalEventManager.ShowEndGameText.AddListener(ShowEnd);

        text.gameObject.SetActive(false);
    }

    private void ShowEnd(string text)
    {
        StartCoroutine(FadeTo(1, fadeDuration, text));
    }

    public void StartTransition()
    {
        StartCoroutine(DelayTransition(0, transitionDuration));
    }

    public void EndTransition()
    {
        StartCoroutine(DelayTransition(1, transitionDuration));
    }

    private IEnumerator DelayTransition(float targetAlpha, float duration)
    {
        Debug.Log("DelayTransition1");
        yield return new WaitForSeconds(1);
        yield return StartCoroutine(FadeTo(targetAlpha, duration));
        Debug.Log("DelayTransition2");
    }

    private IEnumerator FadeTo(float targetAlpha, float duration, string text = "")
    {
        darkenImage.gameObject.SetActive (true);
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

        if (showText && !string.IsNullOrEmpty(text))
        {
            this.text.text = text;
            this.text.gameObject.SetActive(true);
        }
        else
        {
            darkenImage.gameObject.SetActive(false);
        }
    }

    private void SetAlpha(float alpha)
    {
        Color color = darkenImage.color;
        color.a = alpha;
        darkenImage.color = color;
    }
}
