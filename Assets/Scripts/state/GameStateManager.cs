using UnityEngine;
using System;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    public enum GameState
    {
        CalmJourney,
        StormApproaching,
        InStorm,
        EscapingStorm
    }

    public GameState CurrentState { get; private set; }

    public event Action<GameState> OnGameStateChanged;

    public float stormStartDistance = 50f;
    public float escapeDistance = 100f;

    private float stormStartDistanceSqr;
    private float escapeDistanceSqr;

    private Vector3 journeyStartPosition;
    private Vector3 stormStartPosition;
    private Vector3 targetDirection;

    public WeatherController weatherController;
    public SoundController soundController;
    public OceanController oceanController;
    public Transform playerTransform;

    private float stateCheckInterval = 0.5f;
    private float lastStateCheckTime = 0f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        stormStartDistanceSqr = stormStartDistance * stormStartDistance;
        escapeDistanceSqr = escapeDistance * escapeDistance;
        StartJourney();
    }

    private void FixedUpdate()
    {
        if (Time.time - lastStateCheckTime >= stateCheckInterval)
        {
            CheckGameState();
            lastStateCheckTime = Time.time;
        }
    }

    private void CheckGameState()
    {
        switch (CurrentState)
        {
            case GameState.CalmJourney:
                CheckStormStart();
                break;
            case GameState.InStorm:
                CheckStormNavigation();
                break;
        }
    }

    public void StartJourney()
    {
        journeyStartPosition = playerTransform.position;
        ChangeState(GameState.CalmJourney);
        weatherController.ForceCalmWeather();
        soundController.SetWeatherState(SoundController.WeatherState.Calm);
        oceanController.SetOceanState(OceanController.OceanState.Calm);
    }

    private void CheckStormStart()
    {
        if ((playerTransform.position - journeyStartPosition).sqrMagnitude >= stormStartDistanceSqr)
        {
            StartStorm();
        }
    }

    private void StartStorm()
    {
        stormStartPosition = playerTransform.position;
        targetDirection = UnityEngine.Random.value > 0.5f ? Vector3.forward : Vector3.back;
        ChangeState(GameState.StormApproaching);
        weatherController.SetStormyWeather();
        soundController.SetWeatherState(SoundController.WeatherState.Stormy);
        oceanController.SetOceanState(OceanController.OceanState.Stormy);
    }

    private void CheckStormNavigation()
    {
        Vector3 movementVector = playerTransform.position - stormStartPosition;
        float distanceTraveledSqr = Vector3.Project(movementVector, targetDirection).sqrMagnitude;

        if (distanceTraveledSqr >= escapeDistanceSqr)
        {
            EscapeStorm();
        }
        else
        {
            float weatherIntensity = 1f - (Mathf.Sqrt(distanceTraveledSqr) / escapeDistance);
            weatherController.UpdateWeatherIntensity(weatherIntensity);
            oceanController.UpdateOceanIntensity(weatherIntensity);
        }
    }

    private void EscapeStorm()
    {
        ChangeState(GameState.CalmJourney);
        weatherController.SetCalmWeather();
        soundController.SetWeatherState(SoundController.WeatherState.Calm);
        oceanController.SetOceanState(OceanController.OceanState.Calm);
        journeyStartPosition = playerTransform.position;
    }

    private void ChangeState(GameState newState)
    {
        CurrentState = newState;
        OnGameStateChanged?.Invoke(newState);
    }

    public Vector3 GetTargetDirection()
    {
        return targetDirection;
    }

    public float GetDistanceTraveledInStorm()
    {
        if (CurrentState != GameState.InStorm) return 0f;
        Vector3 movementVector = playerTransform.position - stormStartPosition;
        return Mathf.Sqrt(Vector3.Project(movementVector, targetDirection).sqrMagnitude);
    }
}