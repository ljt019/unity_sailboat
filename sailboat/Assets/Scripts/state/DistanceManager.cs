using UnityEngine;
using static GameEnums;

public class DistanceManager
{
    private float distanceTraveled;
    private float correctDistanceTraveled;
    private float incorrectDistanceTraveled;

    public void UpdateDistances(float incrementalDistance, bool isCorrectDirection, GameState gameState)
    {
        if (gameState == GameState.Calm || gameState == GameState.StormIncoming)
        {
            distanceTraveled += incrementalDistance;
        }
        else
        {
            if (isCorrectDirection)
            {
                correctDistanceTraveled += incrementalDistance;
                incorrectDistanceTraveled = 0; // Reset incorrect distance when moving correctly
            }
            else
            {
                incorrectDistanceTraveled += incrementalDistance;
            }
        }

        Debug.Log($"Incremental Distance: {incrementalDistance}, Correct direction distance: {correctDistanceTraveled}, Wrong direction distance: {incorrectDistanceTraveled}");
    }

    public void ResetDistances()
    {
        distanceTraveled = 0f;
        correctDistanceTraveled = 0f;
        incorrectDistanceTraveled = 0f;
    }

    public (float, float) GetDistance(GameState gameState)
    {
        if (gameState == GameState.Calm || gameState == GameState.StormIncoming)
        {
            return (distanceTraveled, 0);
        }
        else
        {
            return (correctDistanceTraveled, incorrectDistanceTraveled);
        }
    }
}