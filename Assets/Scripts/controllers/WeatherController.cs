using UnityEngine;
using Tenkoku.Core;
using System.Collections;
using JetBrains.Annotations;

public class WeatherController : MonoBehaviour
{
    public TenkokuModule tenkokuModule;

    private float initialRainAmt;
    private float initialFogAmt;
    private float initialLightningAmt;
    private float initialOvercastAmt;

    public float transitionSpeed = 0.5f;

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

        StoreInitialWeatherConditions();
    }

    void StoreInitialWeatherConditions()
    {
        initialRainAmt = tenkokuModule.weather_RainAmt;
        initialFogAmt = tenkokuModule.weather_FogAmt;
        initialLightningAmt = tenkokuModule.weather_lightning;
        initialOvercastAmt = tenkokuModule.weather_OvercastAmt;
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

    public void ForceCalmWeather()
    {
        tenkokuModule.weather_RainAmt = 0f;
        tenkokuModule.weather_FogAmt = 0f;
        tenkokuModule.weather_lightning = 0f;
        tenkokuModule.weather_OvercastAmt = 0f;
        tenkokuModule.weather_forceUpdate = true;
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
        tenkokuModule.weather_RainAmt = Mathf.Lerp(0f, initialRainAmt, intensity);
        tenkokuModule.weather_FogAmt = Mathf.Lerp(0f, initialFogAmt, intensity);
        tenkokuModule.weather_lightning = Mathf.Lerp(0f, initialLightningAmt, intensity);
        tenkokuModule.weather_OvercastAmt = Mathf.Lerp(0f, initialOvercastAmt, intensity);

        tenkokuModule.weather_forceUpdate = true;
    }

    public float GetCurrentIntensity()
    {
        return currentIntensity;
    }
}