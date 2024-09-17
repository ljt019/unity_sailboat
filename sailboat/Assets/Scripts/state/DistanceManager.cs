using UnityEngine;
using static GameEnums;

/// <summary>
/// Manages and tracks various distance metrics in the game.
/// </summary>
public class DistanceManager
{
    private float distanceTraveled;
    private float correctDistanceTraveled;
    private float incorrectDistanceTraveled;

    /// <summary>
    /// Updates the distance metrics based on the current game state and direction correctness.
    /// </summary>
    /// <param name="incrementalDistance">The distance increment to add.</param>
    /// <param name="isCorrectDirection">Indicates if the current direction is correct.</param>
    /// <param name="gameState">The current state of the game.</param>
    public void UpdateDistances(float incrementalDistance, bool isCorrectDirection, GameState gameState)
    {
        switch (gameState)
        {
            case GameState.Calm:
            case GameState.StormIncoming:
                distanceTraveled += incrementalDistance;
                break;
            case GameState.Stormy:
                if (isCorrectDirection)
                {
                    correctDistanceTraveled += incrementalDistance;
                }
                else
                {
                    incorrectDistanceTraveled += incrementalDistance;
                }
                break;
            default:
                Debug.LogWarning($"Unhandled GameState: {gameState}");
                break;
        }

        // Conditional Logging to prevent excessive logs
        if (Debug.isDebugBuild)
        {
            Debug.Log($"Incremental Distance: {incrementalDistance}, Correct direction distance: {correctDistanceTraveled}, Wrong direction distance: {incorrectDistanceTraveled}");
        }
    }

    /// <summary>
    /// Resets all tracked distance metrics to zero.
    /// </summary>
    public void ResetDistances()
    {
        distanceTraveled = 0f;
        correctDistanceTraveled = 0f;
        incorrectDistanceTraveled = 0f;
    }

    /// <summary>
    /// Retrieves the current distance metrics based on the game state.
    /// </summary>
    /// <param name="gameState">The current state of the game.</param>
    /// <returns>A DistanceMetrics struct containing relevant distances.</returns>
    public DistanceMetrics GetDistance(GameState gameState)
    {
        return gameState switch
        {
            GameState.Calm or GameState.StormIncoming => new DistanceMetrics(distanceTraveled, 0f),
            GameState.Stormy => new DistanceMetrics(correctDistanceTraveled, incorrectDistanceTraveled),
            _ => new DistanceMetrics(0f, 0f),
        };
    }

    /// <summary>
    /// Represents distance metrics for the game.
    /// </summary>
    public struct DistanceMetrics
    {
        public float Traveled;
        public float IncorrectTraveled;

        public DistanceMetrics(float traveled, float incorrectTraveled)
        {
            Traveled = traveled;
            IncorrectTraveled = incorrectTraveled;
        }
    }
}
