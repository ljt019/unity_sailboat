using UnityEngine;
using static GameEnums;

/// <summary>
/// Manages transitions between different game states, handling related actions and notifying listeners.
/// </summary>
public class GameStateTransitioner
{
    private GameState currentState;
    private readonly WeatherStateManager weatherStateManager;
    private readonly NavigationManager navigationManager;
    private readonly PromptController promptController;
    private readonly DistanceManager distanceManager;

    /// <summary>
    /// Event triggered when the game state changes.
    /// </summary>
    public event System.Action<GameState> OnStateChange;

    /// <summary>
    /// Initializes a new instance of the GameStateTransitioner class.
    /// </summary>
    /// <param name="wsm">WeatherStateManager instance.</param>
    /// <param name="nm">NavigationManager instance.</param>
    /// <param name="pc">PromptController instance.</param>
    /// <param name="dm">DistanceManager instance.</param>
    public GameStateTransitioner(WeatherStateManager wsm, NavigationManager nm, PromptController pc, DistanceManager dm)
    {
        weatherStateManager = wsm;
        navigationManager = nm;
        promptController = pc;
        distanceManager = dm;
        currentState = GameState.Calm;
    }

    /// <summary>
    /// Transitions the game state to Calm.
    /// </summary>
    /// <param name="currentHintState">Reference to the current hint state.</param>
    /// <param name="currentEscapeDirection">Reference to the current escape direction.</param>
    public void TransitionToCalm(ref HintState currentHintState, ref EscapeDirection currentEscapeDirection)
    {
        if (currentState == GameState.Calm) return;

        currentState = GameState.Calm;
        currentHintState = HintState.Direction;
        currentEscapeDirection = EscapeDirection.None;

        distanceManager.ResetDistances();
        promptController.ClearPrompt();
        weatherStateManager.SetCalmWeather();

        LogStateTransition("Calm");
        OnStateChange?.Invoke(currentState);
    }

    /// <summary>
    /// Transitions the game state to Storm Incoming.
    /// </summary>
    public void TransitionToStormIncoming()
    {
        if (currentState == GameState.StormIncoming || currentState == GameState.Stormy) return;

        currentState = GameState.StormIncoming;

        promptController.StormApproachingPrompt();
        weatherStateManager.SetStormIncomingWeather();

        LogStateTransition("Storm Incoming");
        OnStateChange?.Invoke(currentState);
    }

    /// <summary>
    /// Transitions the game state to Stormy.
    /// </summary>
    /// <param name="currentEscapeDirection">Reference to the current escape direction.</param>
    /// <param name="targetDirection">Reference to the target direction.</param>
    public void TransitionToStormy(ref EscapeDirection currentEscapeDirection, ref Vector3 targetDirection)
    {
        if (currentState == GameState.Stormy) return;

        currentState = GameState.Stormy;

        distanceManager.ResetDistances();
        weatherStateManager.SetStormyWeather();
        navigationManager.PromptNavigation(ref currentEscapeDirection, ref targetDirection);

        LogStateTransition("Stormy");
        OnStateChange?.Invoke(currentState);
    }

    /// <summary>
    /// Retrieves the current game state.
    /// </summary>
    /// <returns>The current GameState.</returns>
    public GameState GetCurrentState()
    {
        return currentState;
    }

    /// <summary>
    /// Logs the state transition if in debug mode.
    /// </summary>
    /// <param name="stateName">The name of the state transitioned to.</param>
    private void LogStateTransition(string stateName)
    {
        if (Debug.isDebugBuild)
        {
            Debug.Log($"Transitioned to {stateName} state.");
        }
    }
}
