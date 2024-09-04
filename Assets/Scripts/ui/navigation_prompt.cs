using UnityEngine;
using TMPro;

public class NavigationPrompt : MonoBehaviour
{
    [Header("Entity References")]
    public TextMeshProUGUI promptText;
    public Transform playerTransform;

    [Header("Controller References")]
    public WeatherController weatherController;
    public SoundController soundController;
    public OceanAdvanced ocean;

    [Header("Navigation Settings")]
    public float stormStartDistance = 100f;
    public float escapeDistance = 20f;
    public float directionThreshold = 0.7f;

    private Vector3 targetDirection;
    private Vector3 journeyStartPosition;
    private Vector3 stormStartPosition;
    private bool isInStorm = false;
    private float distanceTraveled = 0f;
    private float distanceTraveledInCorrectDirection = 0f;

    private Color defaultColor = Color.white;
    private Color correctDirectionColor = Color.green;

    void Start()
    {
        if (promptText == null || playerTransform == null)
        {
            Debug.LogError("Prompt Text or Player Transform is not assigned!");
            return;
        }

        SetupPromptTextPosition();
        StartJourney();
    }

    void SetupPromptTextPosition()
    {
        RectTransform rectTransform = promptText.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0, 1);
        rectTransform.anchorMax = new Vector2(1, 1);
        rectTransform.pivot = new Vector2(0.5f, 1);
        rectTransform.anchoredPosition = new Vector2(0, -20);
        rectTransform.sizeDelta = new Vector2(0, rectTransform.sizeDelta.y);
        promptText.alignment = TextAlignmentOptions.Top;
        promptText.enableWordWrapping = true;
    }

    void Update()
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

    void StartJourney()
    {
        journeyStartPosition = playerTransform.position;
        distanceTraveled = 0f;
        UpdatePromptText("");
        promptText.color = defaultColor;
        SetCalmWeather();
    }

    void CheckStormStart()
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

    void StormIncoming()
    {
        UpdatePromptText("A storm is approaching! Prepare to navigate!");
        SetStormApproachingWeather();
    }

    void StartStorm()
    {
        isInStorm = true;
        stormStartPosition = playerTransform.position;
        distanceTraveledInCorrectDirection = 0f;
        SetStormyWeather();
        PromptNavigation();
    }

    void CheckNavigation()
    {
        Vector3 movementVector = playerTransform.position - stormStartPosition;
        float dotProduct = Vector3.Dot(movementVector.normalized, targetDirection);

        if (dotProduct >= directionThreshold)
        {
            distanceTraveledInCorrectDirection = movementVector.magnitude;
        }

        if (distanceTraveledInCorrectDirection >= escapeDistance)
        {
            EscapeStorm();
        }
        else
        {
            UpdatePromptText($"Navigate {GetDirectionText(targetDirection)} to escape the storm!\n");
        }

        UpdateWeather();
    }

    void PromptNavigation()
    {
        targetDirection = Random.value > 0.5f ? Vector3.forward : Vector3.back;
        UpdatePromptText($"Storm has started! Navigate {GetDirectionText(targetDirection)} to escape!");
    }

    void UpdateDirectionFeedback()
    {
        Vector3 playerDirection = new Vector3(playerTransform.forward.x, 0, playerTransform.forward.z).normalized;
        float dotProduct = Vector3.Dot(playerDirection, targetDirection);
        promptText.color = (dotProduct >= directionThreshold) ? correctDirectionColor : defaultColor;
    }

    void UpdateWeather()
    {
        float weatherIntensity = Mathf.Clamp01(1f - (distanceTraveledInCorrectDirection / escapeDistance));
        weatherController.UpdateWeatherIntensity(weatherIntensity);
    }

    void EscapeStorm()
    {
        isInStorm = false;
        UpdatePromptText("You've successfully navigated out of the storm! Enjoy the calm seas.");
        promptText.color = defaultColor;
        SetCalmWeather();
        StartJourney(); // Reset for the next storm
    }

    void SetCalmWeather()
    {
        weatherController.SetCalmWeather();
        soundController.SetWeatherState(SoundController.WeatherState.Calm);
        ocean.SetWaterState(OceanAdvanced.WaterState.Calm);
    }

    void SetStormApproachingWeather()
    {
        weatherController.SetStormApproachingWeather();
        soundController.SetWeatherState(SoundController.WeatherState.Stormy);
        ocean.SetWaterState(OceanAdvanced.WaterState.Choppy);
    }

    void SetStormyWeather()
    {
        weatherController.SetStormyWeather();
        ocean.SetWaterState(OceanAdvanced.WaterState.Stormy);
    }

    void UpdatePromptText(string newText)
    {
        promptText.text = newText;
        promptText.ForceMeshUpdate();
    }

    string GetDirectionText(Vector3 direction)
    {
        return direction == Vector3.forward ? "North" : "South";
    }
}