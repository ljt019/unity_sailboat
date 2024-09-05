using UnityEngine;
using Tenkoku.Core;
using System.Collections;

public class WeatherController : MonoBehaviour
{
    [Header("Entity References")]
    public TenkokuModule tenkokuModule;

    [Header("Calm Weather Settings")]
    [SerializeField] public float calmRainAmt = 0f;
    [SerializeField] public float calmFogAmt = 0f;
    [SerializeField] public float calmLightningAmt = 0f;
    [SerializeField] public float calmOvercastAmt = 0f;

    [Header("Stormy Weather Settings")]
    [SerializeField] public float stormyRainAmt = 1.0f;
    [SerializeField] public float stormyFogAmt = 0.147f;
    [SerializeField] public float stormyLightningAmt = 0.543f;
    [SerializeField] public float stormyOvercastAmt = 0.104f;

    [Header("Misc")]
    [SerializeField] public float transitionSpeed = 0.5f;

    private float currentIntensity = 0f;
    private float targetIntensity = 0f;

    private Coroutine transitionCoroutine;

    void Start()
    {
        if (tenkokuModule == null)
        {
            tenkokuModule = FindObjectOfType<TenkokuModule>();
            if (tenkokuModule == null)
            {
                Debug.LogError("Tenkoku Module is not found in the scene!");
                return;
            }
        }

        // Start with calm weather
        ForceCalmWeather();
    }

    public void SetStormyWeather()
    {
        targetIntensity = 1f;
        StartWeatherTransition();
    }

    public void SetCalmWeather()
    {
        targetIntensity = 0f;
        StartWeatherTransition();
    }

    public void SetStormApproachingWeather()
    {
        targetIntensity = 0.1f;
        StartWeatherTransition();
    }

    public void ForceCalmWeather()
    {
        StopAllCoroutines();
        UpdateWeather(0f);
        currentIntensity = 0f;
        targetIntensity = 0f;
    }

    public void ForceStormyWeather()
    {
        StopAllCoroutines();
        UpdateWeather(1f);
        currentIntensity = 1f;
        targetIntensity = 1f;
    }

    public void UpdateWeatherIntensity(float intensity)
    {
        targetIntensity = Mathf.Clamp01(intensity);
        StartWeatherTransition();
    }

    private void StartWeatherTransition()
    {
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
        }
        transitionCoroutine = StartCoroutine(TransitionWeather());
    }

    private IEnumerator TransitionWeather()
    {
        while (!Mathf.Approximately(currentIntensity, targetIntensity))
        {
            currentIntensity = Mathf.MoveTowards(currentIntensity, targetIntensity, transitionSpeed * Time.deltaTime);
            UpdateWeather(currentIntensity);
            yield return null;
        }
    }

    private void UpdateWeather(float intensity)
    {
        tenkokuModule.weather_RainAmt = Mathf.Lerp(calmRainAmt, stormyRainAmt, intensity);
        tenkokuModule.weather_FogAmt = Mathf.Lerp(calmFogAmt, stormyFogAmt, intensity);
        tenkokuModule.weather_lightning = Mathf.Lerp(calmLightningAmt, stormyLightningAmt, intensity);
        tenkokuModule.weather_OvercastAmt = Mathf.Lerp(calmOvercastAmt, stormyOvercastAmt, intensity);

        tenkokuModule.weather_forceUpdate = true;
    }

    public float GetCurrentIntensity()
    {
        return currentIntensity;
    }
}