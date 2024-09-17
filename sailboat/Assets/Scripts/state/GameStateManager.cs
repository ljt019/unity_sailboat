using UnityEngine;
using static GameEnums;

/// <summary>
/// Manages the overall game state, handling transitions between different weather conditions and tracking player progress.
/// </summary>
public class GameStateManager : MonoBehaviour
{
    #region Serialized Fields

    [Header("Entity References")]
    [SerializeField] private Transform playerTransform;

    [Header("Controller References")]
    [SerializeField] private WeatherController weatherController;
    [SerializeField] private SoundController soundController;
    [SerializeField] private OceanAdvanced oceanController;
    [SerializeField] private PromptController promptController;

    [Header("Navigation Settings")]
    [SerializeField] private float stormIncomingDistanceRatio = 0.5f;
    [SerializeField] private float stormStartDistance = 10f;
    [SerializeField] private float stormEscapeDistance = 5f;
    [SerializeField] private float directionToleranceAngle = 0.75f;
    [SerializeField] private float hintTriggerThreshold = 5f;

    #endregion

    #region Private Fields

    private Vector3 targetDirection;
    private Vector3 lastPosition;

    private DistanceManager distanceManager;
    private WeatherStateManager weatherStateManager;
    private NavigationManager navigationManager;
    private GameStateTransitioner gameStateTransitioner;

    private HintState currentHintState;
    private EscapeDirection currentEscapeDirection = EscapeDirection.None;

    /// <summary>
    /// Event triggered when the game state changes.
    /// </summary>
    public event System.Action<GameState> OnGameStateChange;

    #endregion

    #region Unity Callbacks

    private void Start()
    {
        if (!ValidateReferences())
        {
            Debug.LogError("GameStateManager initialization failed due to missing references.");
            enabled = false;
            return;
        }

        InitializeManagers();
        StartCalm();
    }

    private void Update()
    {
        UpdateMovement();
        UpdateGameState();
        ProcessLogMessages();
    }

    private void OnDestroy()
    {
        CleanupManagers();
    }

    #endregion

    #region Initialization Methods

    /// <summary>
    /// Validates all required serialized references.
    /// </summary>
    /// <returns>True if all references are valid; otherwise, false.</returns>
    private bool ValidateReferences()
    {
        bool isValid = true;

        if (playerTransform == null)
        {
            Debug.LogError("Player Transform is not assigned.");
            isValid = false;
        }

        if (weatherController == null)
        {
            Debug.LogError("WeatherController is not assigned.");
            isValid = false;
        }

        if (soundController == null)
        {
            Debug.LogError("SoundController is not assigned.");
            isValid = false;
        }

        if (oceanController == null)
        {
            Debug.LogError("OceanAdvanced Controller is not assigned.");
            isValid = false;
        }

        if (promptController == null)
        {
            Debug.LogError("PromptController is not assigned.");
            isValid = false;
        }

        return isValid;
    }

    /// <summary>
    /// Initializes all necessary manager classes.
    /// </summary>
    private void InitializeManagers()
    {
        distanceManager = new DistanceManager();
        weatherStateManager = new WeatherStateManager(weatherController, oceanController, soundController);
        navigationManager = new NavigationManager(promptController, directionToleranceAngle, hintTriggerThreshold);
        gameStateTransitioner = new GameStateTransitioner(weatherStateManager, navigationManager, promptController, distanceManager);
        gameStateTransitioner.OnStateChange += HandleGameStateChange;
    }

    /// <summary>
    /// Handles the game state change event from the GameStateTransitioner.
    /// </summary>
    /// <param name="newState">The new game state.</param>
    private void HandleGameStateChange(GameState newState)
    {
        OnGameStateChange?.Invoke(newState);
        Debug.Log($"Game state changed to: {newState}");
    }

    #endregion

    #region Movement and State Updates

    /// <summary>
    /// Updates the player's movement and distance metrics.
    /// </summary>
    private void UpdateMovement()
    {
        Vector3 currentPosition = GetHorizontalVector(playerTransform.position);
        Vector3 movementVector = currentPosition - GetHorizontalVector(lastPosition);
        float incrementalDistance = movementVector.magnitude;

        bool isCorrectDirection = navigationManager.CalculateIsCorrectDirection(movementVector, targetDirection);
        GameState currentState = gameStateTransitioner.GetCurrentState();
        distanceManager.UpdateDistances(incrementalDistance, isCorrectDirection, currentState);

        // Conditional Logging to prevent excessive logs
        if (Debug.isDebugBuild)
        {
            var distances = distanceManager.GetDistance(currentState);
            Debug.Log($"Distance Traveled: {distances.Traveled}, Incorrect Distance Traveled: {distances.IncorrectTraveled}");
        }

        lastPosition = playerTransform.position;
    }

    /// <summary>
    /// Updates the game state based on distance traveled and current state.
    /// </summary>
    private void UpdateGameState()
    {
        GameState currentState = gameStateTransitioner.GetCurrentState();

        switch (currentState)
        {
            case GameState.Calm:
            case GameState.StormIncoming:
                CheckStormStart();
                break;
            case GameState.Stormy:
                CheckStormEscape();
                break;
        }

        // Update Navigation Direction Feedback
        Vector3 movementDirection = GetHorizontalVector(playerTransform.position) - lastPosition;
        navigationManager.UpdateDirectionFeedback();
    }

    #endregion

    #region Game State Checks

    /// <summary>
    /// Checks and handles the transition to storm states based on distance traveled.
    /// </summary>
    private void CheckStormStart()
    {
        GameState currentState = gameStateTransitioner.GetCurrentState();
        float distanceTraveled = distanceManager.GetDistance(currentState).Traveled;

        if (currentState == GameState.StormIncoming)
        {
            if (distanceTraveled >= stormStartDistance)
            {
                gameStateTransitioner.TransitionToStormy(ref currentEscapeDirection, ref targetDirection);
            }
        }
        else if (currentState == GameState.Calm)
        {
            if (distanceTraveled >= stormStartDistance * stormIncomingDistanceRatio)
            {
                gameStateTransitioner.TransitionToStormIncoming();
            }
        }
    }

    /// <summary>
    /// Checks and handles the transition out of stormy state based on distance traveled.
    /// </summary>
    private void CheckStormEscape()
    {
        GameState currentState = gameStateTransitioner.GetCurrentState();
        var distances = distanceManager.GetDistance(currentState);

        if (distances.Traveled >= stormEscapeDistance)
        {
            StartCalm();
            Debug.Log("Storm escaped. Weather set to Calm. New journey started.");
        }
        else
        {
            navigationManager.UpdateNavigationPrompt(distances.IncorrectTraveled, currentEscapeDirection, ref currentHintState);
        }
    }

    #endregion

    #region State Transition Methods

    /// <summary>
    /// Initiates the Calm state.
    /// </summary>
    private void StartCalm()
    {
        gameStateTransitioner.TransitionToCalm(ref currentHintState, ref currentEscapeDirection);
        lastPosition = playerTransform.position;
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Extracts the horizontal components of a vector, setting Y to zero.
    /// </summary>
    /// <param name="vector">The original vector.</param>
    /// <returns>The horizontal vector.</returns>
    private Vector3 GetHorizontalVector(Vector3 vector)
    {
        return new Vector3(vector.x, 0f, vector.z);
    }

    #endregion

    #region Logging Methods

    /// <summary>
    /// Processes and logs queued log messages from other managers.
    /// </summary>
    private void ProcessLogMessages()
    {
        // Implement if there are any log queues from other managers
        // Example:
        // var logs = someManager.GetLogMessages();
        // foreach (var log in logs) { Debug.Log(log); }
    }

    #endregion

    #region Cleanup Methods

    /// <summary>
    /// Cleans up all managers and unsubscribes from events.
    /// </summary>
    private void CleanupManagers()
    {
        if (gameStateTransitioner != null)
        {
            gameStateTransitioner.OnStateChange -= HandleGameStateChange;
            gameStateTransitioner = null;
        }

        // Dispose or cleanup other managers if necessary
        // Example:
        // weatherStateManager?.Dispose();
        // navigationManager?.Dispose();
    }

    #endregion
}
