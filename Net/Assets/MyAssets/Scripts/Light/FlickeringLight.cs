using System.Collections;
using UnityEngine;

public class FlickeringLight : MonoBehaviour
{
    [Header("Базовые настройки")]
    [SerializeField] private bool enableFlickering = true;
    [SerializeField] private float baseIntensity = 1.0f;

    [Header("Настройки мерцания")]
    [SerializeField] private float minFlickerInterval = 0.1f;
    [SerializeField] private float maxFlickerInterval = 0.5f;
    [SerializeField] private float minIntensityMultiplier = 0.1f;
    [SerializeField] private float maxIntensityMultiplier = 1.0f;

    [Header("Эффект полного отключения")]
    [SerializeField] private bool canTurnOff = true;
    [SerializeField] private float turnOffChance = 0.1f; // 10% вероятность
    [SerializeField] private float minOffTime = 0.5f;
    [SerializeField] private float maxOffTime = 2.0f;

    [Header("Цветовые эффекты")]
    [SerializeField] private bool changeColor = false;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color flickerColor = Color.yellow;

    private Light lightComponent;
    private Color originalColor;
    private bool isFlickering = false;

    private void Start()
    {
        lightComponent = GetComponent<Light>();
        if (lightComponent == null)
        {
            Debug.LogError("Компонент Light не найден на объекте " + gameObject.name);
            enabled = false;
            return;
        }

        originalColor = lightComponent.color;
        lightComponent.intensity = baseIntensity;

        if (enableFlickering)
        {
            StartCoroutine(FlickerRoutine());
        }
    }

    private IEnumerator FlickerRoutine()
    {
        while (enableFlickering)
        {
            yield return new WaitForSeconds(Random.Range(minFlickerInterval, maxFlickerInterval));

            if (canTurnOff && Random.value < turnOffChance)
            {
                yield return StartCoroutine(TurnOffSequence());
            }
            else
            {
                // Обычное мерцание
                yield return StartCoroutine(SingleFlicker());
            }
        }
    }

    private IEnumerator SingleFlicker()
    {
        isFlickering = true;

        int flickerCount = Random.Range(2, 5);

        for (int i = 0; i < flickerCount; i++)
        {
            float flickerIntensity = baseIntensity * Random.Range(minIntensityMultiplier, maxIntensityMultiplier);
            lightComponent.intensity = flickerIntensity;

            if (changeColor)
            {
                lightComponent.color = Color.Lerp(normalColor, flickerColor, Random.value);
            }

            yield return new WaitForSeconds(Random.Range(0.05f, 0.15f));

            lightComponent.intensity = baseIntensity;
            if (changeColor)
            {
                lightComponent.color = normalColor;
            }

            yield return new WaitForSeconds(Random.Range(0.05f, 0.1f));
        }

        isFlickering = false;
    }

    private IEnumerator TurnOffSequence()
    {
        for (int i = 0; i < 3; i++)
        {
            lightComponent.intensity = baseIntensity * 0.3f;
            yield return new WaitForSeconds(0.1f);
            lightComponent.intensity = baseIntensity;
            yield return new WaitForSeconds(0.05f);
        }

        lightComponent.intensity = 0;
        yield return new WaitForSeconds(Random.Range(minOffTime, maxOffTime));

        for (int i = 0; i < Random.Range(2, 4); i++)
        {
            lightComponent.intensity = baseIntensity * Random.Range(0.2f, 0.8f);
            yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
            lightComponent.intensity = 0;
            yield return new WaitForSeconds(Random.Range(0.2f, 0.5f));
        }

        lightComponent.intensity = baseIntensity;
        lightComponent.color = originalColor;
    }

    public void SetFlickering(bool enabled)
    {
        enableFlickering = enabled;

        if (enabled && !isFlickering)
        {
            StartCoroutine(FlickerRoutine());
        }
    }

    public void SetBaseIntensity(float intensity)
    {
        baseIntensity = intensity;

        if (!isFlickering)
        {
            lightComponent.intensity = baseIntensity;
        }
    }
}
