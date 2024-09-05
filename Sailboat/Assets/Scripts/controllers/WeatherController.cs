using UnityEngine;
using Tenkoku.Core;
using System.Collections;

public class WeatherController : MonoBehaviour
{
    [Header("Entity References")]
    public TenkokuModule tenkokuModule;

    [Header("Calm Weather Settings")]
    [SerializeField] private float calmRainAmt = 0f;
    [SerializeField] private float calmFogAmt = 0f;
    [SerializeField] private float calmLightningAmt = 0f;
    [SerializeField] private float calmOvercastAmt = 0f;

    [Header("Choppy Weather Settings")]
    [SerializeField] private float choppyRainAmt = 0.5f;
    [SerializeField] private float choppyFogAmt = 0.073f;
    [SerializeField] private float choppyLightningAmt = 0.273f;
    [SerializeField] private float choppyOvercastAmt = 0.052f;

    [Header("Stormy Weather Settings")]
    [SerializeField] private float stormyRainAmt = 1.0f;
    [SerializeField] private float stormyFogAmt = 0.147f;
    [SerializeField] private float stormyLightningAmt = 0.543f;
    [SerializeField] private float stormyOvercastAmt = 0.104f;

    [Header("Misc")]
    [SerializeField] private float transitionSpeed = 0.5f;

    private float currentIntensity = 0f;
    private float targetIntensity = 0f;

    private Coroutine transitionCoroutine;

    private enum WeatherState
    {
        Calm,
        Choppy,
        Stormy
    }

    private WeatherState currentState = WeatherState.Calm;
    private WeatherState targetState = WeatherState.Calm;

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
        SetWeatherState(WeatherState.Calm);
        Debug.Log("WeatherController initialized with calm weather.");
    }

    public void SetStormyWeather()
    {
        SetWeatherState(WeatherState.Stormy);
    }

    public void SetCalmWeather()
    {
        SetWeatherState(WeatherState.Calm);
    }

    public void SetStormApproachingWeather()
    {
        SetWeatherState(WeatherState.Choppy);
    }

    private void SetWeatherState(WeatherState newState)
    {
        targetState = newState;
        targetIntensity = GetTargetIntensity(newState);
        StartWeatherTransition();
        Debug.Log($"Setting weather state to: {newState}");
    }

    public void UpdateWeatherIntensity(float intensity)
    {
        targetIntensity = Mathf.Clamp01(intensity);
        StartWeatherTransition();
        Debug.Log($"Updating weather intensity to: {targetIntensity}");
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
        Debug.Log($"Starting weather transition from {currentState} to {targetState}");
        while (currentState != targetState || !Mathf.Approximately(currentIntensity, targetIntensity))
        {
            currentIntensity = Mathf.MoveTowards(currentIntensity, targetIntensity, transitionSpeed * Time.deltaTime);
            UpdateWeather(currentIntensity);

            if (Mathf.Approximately(currentIntensity, targetIntensity))
            {
                currentState = targetState;
            }

            yield return null;
        }
        Debug.Log("Weather transition complete.");

        // Ensure final weather state is set
        UpdateWeather(targetIntensity);
    }

    private void UpdateWeather(float intensity)
    {
        float rainAmt, fogAmt, lightningAmt, overcastAmt;

        switch (currentState)
        {
            case WeatherState.Calm:
                rainAmt = Mathf.Lerp(calmRainAmt, choppyRainAmt, intensity);
                fogAmt = Mathf.Lerp(calmFogAmt, choppyFogAmt, intensity);
                lightningAmt = Mathf.Lerp(calmLightningAmt, choppyLightningAmt, intensity);
                overcastAmt = Mathf.Lerp(calmOvercastAmt, choppyOvercastAmt, intensity);
                break;
            case WeatherState.Choppy:
                rainAmt = Mathf.Lerp(choppyRainAmt, stormyRainAmt, intensity);
                fogAmt = Mathf.Lerp(choppyFogAmt, stormyFogAmt, intensity);
                lightningAmt = Mathf.Lerp(choppyLightningAmt, stormyLightningAmt, intensity);
                overcastAmt = Mathf.Lerp(choppyOvercastAmt, stormyOvercastAmt, intensity);
                break;
            case WeatherState.Stormy:
                rainAmt = stormyRainAmt;
                fogAmt = stormyFogAmt;
                lightningAmt = stormyLightningAmt;
                overcastAmt = stormyOvercastAmt;
                break;
            default:
                rainAmt = calmRainAmt;
                fogAmt = calmFogAmt;
                lightningAmt = calmLightningAmt;
                overcastAmt = calmOvercastAmt;
                break;
        }

        // Ensure rain amount is zero when in calm state
        if (currentState == WeatherState.Calm)
        {
            rainAmt = 0f;
        }

        tenkokuModule.weather_RainAmt = rainAmt;
        tenkokuModule.weather_FogAmt = fogAmt;
        tenkokuModule.weather_lightning = lightningAmt;
        tenkokuModule.weather_OvercastAmt = overcastAmt;

        tenkokuModule.weather_forceUpdate = true;

        Debug.Log($"Weather updated - Rain: {rainAmt}, Fog: {fogAmt}, Lightning: {lightningAmt}, Overcast: {overcastAmt}");
    }

    private float GetTargetIntensity(WeatherState state)
    {
        switch (state)
        {
            case WeatherState.Calm:
                return 0f;
            case WeatherState.Choppy:
                return 0.5f;
            case WeatherState.Stormy:
                return 1f;
            default:
                return 0f;
        }
    }

    public float GetCurrentIntensity()
    {
        return currentIntensity;
    }

    void Update()
    {
        // Continuously update weather to ensure changes are applied
        UpdateWeather(currentIntensity);
    }
}