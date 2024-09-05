using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    [Header("Entity References")]
    [SerializeField] private Transform playerTransform;

    [Header("Controller References")]
    [SerializeField] private WeatherController weatherController;
    [SerializeField] private SoundController soundController;
    [SerializeField] private OceanAdvanced ocean;
    [SerializeField] private PromptController promptController;

    [Header("Navigation Settings")]
    [SerializeField] private float stormStartDistance = 100f;
    [SerializeField] private float escapeDistance = 20f;
    [SerializeField] private float directionThreshold = 0.7f;
    [SerializeField] private float hintTriggerDistance = 10f;

    private Vector3 targetDirection;
    private Vector3 journeyStartPosition;
    private Vector3 stormStartPosition;
    private bool isInStorm = false;
    private float distanceTraveled = 0f;
    private float distanceTraveledInCorrectDirection = 0f;
    private float cumulativeWrongDirectionDistance = 0f;

    private void Start()
    {
        ValidateReferences();
        StartJourney();
    }

    private void Update()
    {
        if (!isInStorm)
        {
            CheckStormStart();
        }
        else
        {
            CheckNavigation();
        }
        UpdateDirectionFeedback();
    }

    private void ValidateReferences()
    {
        if (playerTransform == null || promptController == null || weatherController == null ||
            soundController == null || ocean == null)
        {
            Debug.LogError("One or more required references are not assigned!");
            enabled = false;
        }
    }

    private void StartJourney()
    {
        journeyStartPosition = playerTransform.position;
        ResetNavigationValues();
        promptController.ClearPrompt();
        SetWeatherState(WeatherState.Calm);
    }

    private void CheckStormStart()
    {
        distanceTraveled = Vector3.Distance(playerTransform.position, journeyStartPosition);

        if (distanceTraveled >= stormStartDistance)
        {
            StartStorm();
        }
        else if (distanceTraveled >= stormStartDistance * 0.65f)
        {
            StormIncoming();
        }
    }

    private void StormIncoming()
    {
        promptController.StormApproachingPrompt();
        SetWeatherState(WeatherState.StormApproaching);
    }

    private void StartStorm()
    {
        isInStorm = true;
        stormStartPosition = playerTransform.position;
        ResetNavigationValues();
        SetWeatherState(WeatherState.Stormy);
        PromptNavigation();
    }

    private void CheckNavigation()
    {
        Vector3 movementVector = playerTransform.position - stormStartPosition;
        float dotProduct = Vector3.Dot(movementVector.normalized, targetDirection);

        UpdateNavigationDistances(movementVector, dotProduct);

        if (distanceTraveledInCorrectDirection >= escapeDistance)
        {
            EscapeStorm();
        }
        else
        {
            UpdateNavigationPrompt();
        }

        UpdateWeather();
    }

    private void UpdateNavigationDistances(Vector3 movementVector, float dotProduct)
    {
        if (dotProduct >= directionThreshold)
        {
            float newDistanceInCorrectDirection = movementVector.magnitude;
            distanceTraveledInCorrectDirection = Mathf.Max(distanceTraveledInCorrectDirection, newDistanceInCorrectDirection);
            cumulativeWrongDirectionDistance = 0f;
        }
        else
        {
            cumulativeWrongDirectionDistance += CalculateWrongDirectionDistance();
        }
    }

    private float CalculateWrongDirectionDistance()
    {
        return Vector3.ProjectOnPlane(playerTransform.GetComponent<Rigidbody>().velocity, targetDirection).magnitude * Time.deltaTime;
    }

    private void UpdateNavigationPrompt()
    {
        string direction = GetDirectionText(targetDirection);
        if (cumulativeWrongDirectionDistance >= hintTriggerDistance)
        {
            promptController.DirectionWithHintPrompt(direction);
        }
        else
        {
            promptController.DirectionPrompt(direction);
        }
    }

    private void PromptNavigation()
    {
        targetDirection = Random.value > 0.5f ? Vector3.forward : Vector3.back;
        promptController.DirectionPrompt(GetDirectionText(targetDirection));
    }

    private void UpdateDirectionFeedback()
    {
        Vector3 playerDirection = new Vector3(playerTransform.forward.x, 0, playerTransform.forward.z).normalized;
        float dotProduct = Vector3.Dot(playerDirection, targetDirection);
        promptController.UpdateDirectionFeedback(dotProduct >= directionThreshold);
    }

    private void UpdateWeather()
    {
        float weatherIntensity = Mathf.Clamp01(1f - (distanceTraveledInCorrectDirection / escapeDistance));
        weatherController.UpdateWeatherIntensity(weatherIntensity);
    }

    private void EscapeStorm()
    {
        isInStorm = false;
        promptController.SuccessfulNavigationPrompt();
        SetWeatherState(WeatherState.Calm);
        StartJourney(); // Reset for the next storm
    }

    private void SetWeatherState(WeatherState state)
    {
        switch (state)
        {
            case WeatherState.Calm:
                weatherController.SetCalmWeather();
                soundController.SetWeatherState(SoundController.WeatherState.Calm);
                ocean.SetWaterState(OceanAdvanced.WaterState.Calm);
                break;
            case WeatherState.StormApproaching:
                weatherController.SetStormApproachingWeather();
                soundController.SetWeatherState(SoundController.WeatherState.Stormy);
                ocean.SetWaterState(OceanAdvanced.WaterState.Choppy);
                break;
            case WeatherState.Stormy:
                weatherController.SetStormyWeather();
                ocean.SetWaterState(OceanAdvanced.WaterState.Stormy);
                break;
        }
    }

    private string GetDirectionText(Vector3 direction)
    {
        return direction == Vector3.forward ? "North" : "South";
    }

    private void ResetNavigationValues()
    {
        distanceTraveled = 0f;
        distanceTraveledInCorrectDirection = 0f;
        cumulativeWrongDirectionDistance = 0f;
    }

    private enum WeatherState
    {
        Calm,
        StormApproaching,
        Stormy
    }
}