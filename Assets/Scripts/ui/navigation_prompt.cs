using UnityEngine;
using TMPro;

public class NavigationPrompt : MonoBehaviour
{
    public TextMeshProUGUI promptText;
    public Transform playerTransform;
    public float stormStartDistance = 50f;
    public float escapeDistance = 100f;
    public float directionThreshold = 0.7f;

    private string[] directions = { "North", "South" };
    private Vector3 targetDirection;
    private Vector3 journeyStartPosition;
    private Vector3 stormStartPosition;
    private bool isInStorm = false;
    private float distanceTraveledInCorrectDirection = 0f;

    // Colors for feedback
    private Color defaultColor = Color.white;
    private Color correctDirectionColor = Color.green;

    // Controllers
    public WeatherController weatherController;
    public SoundController soundController;

    public OceanAdvanced ocean;

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
        if (promptText != null)
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
            UpdateDirectionFeedback();
            UpdateWeather();
        }
    }

    void StartJourney()
    {
        journeyStartPosition = playerTransform.position;
        UpdatePromptText("Enjoy the calm waters!");
        promptText.color = defaultColor;
        weatherController.ForceCalmWeather();
        soundController.SetWeatherState(SoundController.WeatherState.Calm);
    }

    void CheckStormStart()
    {
        if (Vector3.Distance(playerTransform.position, journeyStartPosition) >= stormStartDistance)
        {
            StartStorm();
        }
    }

    void StartStorm()
    {
        isInStorm = true;
        stormStartPosition = playerTransform.position;
        distanceTraveledInCorrectDirection = 0f;
        weatherController.SetStormyWeather();
        soundController.SetWeatherState(SoundController.WeatherState.Stormy);
        ocean.SetWaterState(OceanAdvanced.WaterState.Stormy);
        UpdatePromptText("A storm is approaching!");
        PromptNavigation();

    }

    void CheckNavigation()
    {
        Vector3 movementVector = playerTransform.position - stormStartPosition;
        float dotProduct = Vector3.Dot(movementVector.normalized, targetDirection);

        if (dotProduct >= directionThreshold)
        {
            float movementInCorrectDirection = Vector3.Project(movementVector, targetDirection).magnitude;
            distanceTraveledInCorrectDirection = movementInCorrectDirection;
        }

        if (distanceTraveledInCorrectDirection >= escapeDistance)
        {
            EscapeStorm();
        }

        UpdatePromptText($"Navigate {(targetDirection == Vector3.forward ? "North" : "South")} to escape the storm!\nDistance: {distanceTraveledInCorrectDirection:F1} / {escapeDistance:F1}");
    }

    void PromptNavigation()
    {
        targetDirection = (Random.value > 0.5f) ? Vector3.forward : Vector3.back;
        string directionText = (targetDirection == Vector3.forward) ? "North" : "South";
        UpdatePromptText($"Navigate {directionText} to escape the storm!");
    }

    void UpdateDirectionFeedback()
    {
        float dotProduct = Vector3.Dot(playerTransform.forward, targetDirection);
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
        UpdatePromptText("You've successfully navigated out of the storm!");
        promptText.color = defaultColor;
        weatherController.SetCalmWeather();
        soundController.SetWeatherState(SoundController.WeatherState.Calm);

        // Reset for the next potential storm
        journeyStartPosition = playerTransform.position;
    }

    void UpdatePromptText(string newText)
    {
        if (promptText != null)
        {
            promptText.text = newText;
            promptText.ForceMeshUpdate();
        }
    }
}