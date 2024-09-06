using UnityEngine;
using UnityEngine.AI;
using static GameEnums;

public class GameStateManager : MonoBehaviour
{
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

    private Vector3 targetDirection;
    private Vector3 lastPosition;

    private DistanceManager distanceManager;
    private WeatherStateManager weatherStateManager;
    private NavigationManager navigationManager;
    private GameStateTransitioner gameStateTransitioner;

    private HintState currentHintState;
    private EscapeDirection currentEscapeDirection = EscapeDirection.None;

    public delegate void GameStateChangeHandler(GameState newState);
    public event GameStateChangeHandler OnGameStateChange;

    private void Start()
    {
        InitializeManagers();
        StartCalm();
    }

    private void InitializeManagers()
    {
        distanceManager = new DistanceManager();
        weatherStateManager = new WeatherStateManager(weatherController, oceanController, soundController);
        navigationManager = new NavigationManager(promptController, directionToleranceAngle, hintTriggerThreshold);
        gameStateTransitioner = new GameStateTransitioner(weatherStateManager, navigationManager, promptController, distanceManager);
        gameStateTransitioner.OnStateChange += (newState) => OnGameStateChange?.Invoke(newState);
    }

    private void Update()
    {
        Vector3 currentPosition = GetHorizontalVector(playerTransform.position);
        Vector3 movementVector = currentPosition - GetHorizontalVector(lastPosition);
        float incrementalDistance = movementVector.magnitude;

        bool isCorrectDirection = navigationManager.IsCorrectDirection(movementVector, targetDirection);
        distanceManager.UpdateDistances(incrementalDistance, isCorrectDirection, gameStateTransitioner.GetCurrentState());

        (float distanceTraveled, float incorrectDistanceTraveled) = distanceManager.GetDistance(gameStateTransitioner.GetCurrentState());
        Debug.Log($"Distance Traveled: {distanceTraveled}");

        switch (gameStateTransitioner.GetCurrentState())
        {
            case GameState.Calm:
            case GameState.StormIncoming:
                CheckStormStart();
                break;
            case GameState.Stormy:
                CheckStormEscape();
                break;
        }

        navigationManager.updateIsCorrectDirection(GetHorizontalVector(movementVector), targetDirection);
        navigationManager.UpdateDirectionFeedback();
        lastPosition = playerTransform.position;
    }

    private void StartCalm()
    {
        gameStateTransitioner.TransitionToCalm(ref currentHintState, ref currentEscapeDirection, playerTransform.position);
        lastPosition = playerTransform.position;
    }

    private void CheckStormStart()
    {
        float distanceTraveled = distanceManager.GetDistance(gameStateTransitioner.GetCurrentState()).Item1;
        Debug.Log($"Distance Traveled: {distanceTraveled}");

        if (gameStateTransitioner.GetCurrentState() == GameState.StormIncoming)
        {
            if (distanceTraveled >= stormStartDistance)
            {
                gameStateTransitioner.TransitionToStormy(ref targetDirection, ref currentEscapeDirection, playerTransform.position);
            }
        }
        else if (gameStateTransitioner.GetCurrentState() == GameState.Calm)
        {
            if (distanceTraveled >= stormStartDistance * stormIncomingDistanceRatio)
            {
                gameStateTransitioner.TransitionToStormIncoming();
            }
        }
    }

    private void CheckStormEscape()
    {
        float correctDistanceTraveled = distanceManager.GetDistance(gameStateTransitioner.GetCurrentState()).Item1;
        float incorrectDistanceTraveled = distanceManager.GetDistance(gameStateTransitioner.GetCurrentState()).Item2;

        if (correctDistanceTraveled >= stormEscapeDistance)
        {
            StartCalm();
            Debug.Log("Storm escaped. Weather set to Calm. New journey started.");
        }
        else
        {
            navigationManager.UpdateNavigationPrompt(incorrectDistanceTraveled, currentEscapeDirection, ref currentHintState);
        }
    }

    private Vector3 GetHorizontalVector(Vector3 vector)
    {
        return new Vector3(vector.x, 0, vector.z);
    }
}