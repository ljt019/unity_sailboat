using UnityEngine;
using TMPro;

public class NavigationPrompt : MonoBehaviour
{
    public TextMeshProUGUI promptText;
    public float directionThreshold = 0.7f;

    private Color defaultColor = Color.white;
    private Color correctDirectionColor = Color.green;

    private GameStateManager gameStateManager;

    void Start()
    {
        if (promptText == null)
        {
            Debug.LogError("Prompt Text is not assigned!");
            return;
        }

        gameStateManager = GameStateManager.Instance;
        if (gameStateManager == null)
        {
            Debug.LogError("GameStateManager not found!");
            return;
        }

        SetupPromptTextPosition();
        gameStateManager.OnGameStateChanged += HandleGameStateChanged;
    }

    void OnDestroy()
    {
        if (gameStateManager != null)
        {
            gameStateManager.OnGameStateChanged -= HandleGameStateChanged;
        }
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
        UpdatePromptBasedOnGameState();
    }

    void HandleGameStateChanged(GameStateManager.GameState newState)
    {
        UpdatePromptBasedOnGameState();
    }

    void UpdatePromptBasedOnGameState()
    {
        switch (gameStateManager.CurrentState)
        {
            case GameStateManager.GameState.CalmJourney:
                UpdatePromptText("Enjoy the calm waters!");
                promptText.color = defaultColor;
                break;
            case GameStateManager.GameState.StormApproaching:
                UpdatePromptText("A storm is approaching!");
                promptText.color = defaultColor;
                break;
            case GameStateManager.GameState.InStorm:
                UpdateStormNavigation();
                break;
            case GameStateManager.GameState.EscapingStorm:
                UpdatePromptText("You're almost out of the storm!");
                promptText.color = defaultColor;
                break;
        }
    }

    void UpdateStormNavigation()
    {
        Vector3 targetDirection = gameStateManager.GetTargetDirection();
        float distanceTraveled = gameStateManager.GetDistanceTraveledInStorm();
        string directionText = (targetDirection == Vector3.forward) ? "North" : "South";

        UpdatePromptText($"Navigate {directionText} to escape the storm!\nDistance: {distanceTraveled:F1} / {gameStateManager.escapeDistance:F1}");

        float dotProduct = Vector3.Dot(gameStateManager.playerTransform.forward, targetDirection);
        promptText.color = (dotProduct >= directionThreshold) ? correctDirectionColor : defaultColor;
    }

    void UpdatePromptText(string newText)
    {
        promptText.text = newText;
        promptText.ForceMeshUpdate();
    }
}