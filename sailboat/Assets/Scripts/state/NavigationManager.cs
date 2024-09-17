using UnityEngine;
using static GameEnums;

/// <summary>
/// Manages navigation logic, including direction validation and prompt updates.
/// </summary>
public class NavigationManager
{
    private readonly PromptController promptController;
    private readonly float directionToleranceDot;
    private readonly float hintTriggerThreshold;

    /// <summary>
    /// Indicates whether the current direction is correct.
    /// </summary>
    public bool IsCorrectDirection { get; private set; }

    /// <summary>
    /// Initializes a new instance of the NavigationManager class.
    /// </summary>
    /// <param name="pc">PromptController instance.</param>
    /// <param name="toleranceAngleDegrees">Direction tolerance angle in degrees.</param>
    /// <param name="hintThreshold">Threshold distance to trigger hints.</param>
    public NavigationManager(PromptController pc, float toleranceAngleDegrees, float hintThreshold)
    {
        promptController = pc;
        directionToleranceDot = Mathf.Cos(toleranceAngleDegrees * Mathf.Deg2Rad); // Convert angle to dot product threshold
        hintTriggerThreshold = hintThreshold;
    }

    /// <summary>
    /// Updates whether the current movement direction is correct based on the target direction.
    /// </summary>
    /// <param name="movementDirection">The current movement direction vector.</param>
    /// <param name="targetDirection">The desired target direction vector.</param>
    public void UpdateIsCorrectDirection(Vector3 movementDirection, Vector3 targetDirection)
    {
        IsCorrectDirection = CalculateIsCorrectDirection(movementDirection, targetDirection);
    }

    /// <summary>
    /// Determines if the movement direction aligns with the target direction within the tolerance.
    /// </summary>
    /// <param name="movementDirection">The current movement direction vector.</param>
    /// <param name="targetDirection">The desired target direction vector.</param>
    /// <returns>True if direction is correct; otherwise, false.</returns>
    public bool CalculateIsCorrectDirection(Vector3 movementDirection, Vector3 targetDirection)
    {
        if (movementDirection == Vector3.zero || targetDirection == Vector3.zero)
            return false;

        Vector3 normalizedMovement = movementDirection.normalized;
        Vector3 normalizedTarget = targetDirection.normalized;

        float dotProduct = Vector3.Dot(normalizedMovement, normalizedTarget);
        return dotProduct >= directionToleranceDot;
    }

    /// <summary>
    /// Updates the navigation prompt based on incorrect distance traveled and current escape direction.
    /// </summary>
    /// <param name="incorrectDistance">The distance traveled in the wrong direction.</param>
    /// <param name="direction">The current escape direction.</param>
    /// <param name="currentHintState">Reference to the current hint state.</param>
    public void UpdateNavigationPrompt(float incorrectDistance, EscapeDirection direction, ref HintState currentHintState)
    {
        if (incorrectDistance >= hintTriggerThreshold)
        {
            if (currentHintState != HintState.Hint)
            {
                promptController.DisplayHint();
                currentHintState = HintState.Hint;
                LogDebug("Hint displayed due to wrong direction travel.");
            }
        }
        else
        {
            if (currentHintState != HintState.Direction)
            {
                promptController.DirectionPrompt(GetDirectionText(direction));
                currentHintState = HintState.Direction;
                LogDebug($"Direction prompt updated to: {direction}");
            }
        }
    }

    /// <summary>
    /// Prompts the player with a navigation direction based on the escape direction.
    /// </summary>
    /// <param name="currentEscapeDirection">Reference to the current escape direction.</param>
    /// <param name="targetDirection">Reference to the target direction vector.</param>
    public void PromptNavigation(ref EscapeDirection currentEscapeDirection, ref Vector3 targetDirection)
    {
        currentEscapeDirection = Random.value > 0.5f ? EscapeDirection.North : EscapeDirection.South;
        targetDirection = currentEscapeDirection == EscapeDirection.North ? Vector3.forward : Vector3.back;
        promptController.DirectionPrompt(GetDirectionText(currentEscapeDirection));
        LogDebug($"New navigation direction: {GetDirectionText(currentEscapeDirection)}");
    }

    /// <summary>
    /// Updates the direction feedback based on whether the direction is correct.
    /// </summary>
    public void UpdateDirectionFeedback()
    {
        promptController.UpdateDirectionFeedback(IsCorrectDirection);
    }

    /// <summary>
    /// Converts an EscapeDirection enum to its corresponding text representation.
    /// </summary>
    /// <param name="direction">The escape direction.</param>
    /// <returns>String representation of the direction.</returns>
    private string GetDirectionText(EscapeDirection direction)
    {
        return direction switch
        {
            EscapeDirection.North => "North",
            EscapeDirection.South => "South",
            _ => "None",
        };
    }

    /// <summary>
    /// Logs debug messages if in debug build.
    /// </summary>
    /// <param name="message">The debug message.</param>
    private void LogDebug(string message)
    {
        if (Debug.isDebugBuild)
        {
            Debug.Log(message);
        }
    }
}