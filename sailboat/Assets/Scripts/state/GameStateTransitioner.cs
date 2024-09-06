using UnityEngine;
using static GameEnums;

public class GameStateTransitioner
{
    private GameState currentState;
    private WeatherStateManager weatherStateManager;
    private NavigationManager navigationManager;
    private PromptController promptController;
    private DistanceManager distanceManager;

    public delegate void StateChangeHandler(GameState newState);
    public event StateChangeHandler OnStateChange;

    public GameStateTransitioner(WeatherStateManager wsm, NavigationManager nm, PromptController pc, DistanceManager dm)
    {
        weatherStateManager = wsm;
        navigationManager = nm;
        promptController = pc;
        distanceManager = dm;
        currentState = GameState.Calm;
    }

    public void TransitionToCalm(ref HintState currentHintState, ref EscapeDirection currentEscapeDirection, Vector3 playerPosition)
    {
        if (currentState == GameState.Calm) return;

        currentState = GameState.Calm;
        currentHintState = HintState.Direction;
        currentEscapeDirection = EscapeDirection.None;

        distanceManager.ResetDistances();
        promptController.ClearPrompt();
        weatherStateManager.SetCalmWeather();
        Debug.Log("Transitioned to Calm state.");
        OnStateChange?.Invoke(currentState);
    }

    public void TransitionToStormIncoming()
    {
        if (currentState == GameState.StormIncoming || currentState == GameState.Stormy) return;

        currentState = GameState.StormIncoming;
        promptController.StormApproachingPrompt();
        weatherStateManager.SetStormIncomingWeather();
        Debug.Log("Transitioned to Storm Incoming state.");
        OnStateChange?.Invoke(currentState);
    }

    public void TransitionToStormy(ref Vector3 targetDirection, ref EscapeDirection currentEscapeDirection, Vector3 playerPosition)
    {
        if (currentState == GameState.Stormy) return;

        currentState = GameState.Stormy;
        distanceManager.ResetDistances();
        weatherStateManager.SetStormyWeather();
        navigationManager.PromptNavigation(ref currentEscapeDirection, ref targetDirection);
        Debug.Log("Transitioned to Stormy state.");
        OnStateChange?.Invoke(currentState);
    }

    public GameState GetCurrentState()
    {
        return currentState;
    }
}