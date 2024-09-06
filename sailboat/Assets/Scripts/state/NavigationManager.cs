using Unity.VisualScripting;
using UnityEngine;
using static GameEnums;

public class NavigationManager
{
    private PromptController promptController;
    private float directionToleranceAngle;
    private float hintTriggerThreshold;

    public bool isCorrectDirection;

    public NavigationManager(PromptController pc, float toleranceAngle, float hintThreshold)
    {
        promptController = pc;
        directionToleranceAngle = toleranceAngle;
        hintTriggerThreshold = hintThreshold;
    }

    public void updateIsCorrectDirection(Vector3 movementDirection, Vector3 targetDirection)
    {
        isCorrectDirection = IsCorrectDirection(movementDirection, targetDirection);
    }

    public bool IsCorrectDirection(Vector3 movementDirection, Vector3 targetDirection)
    {
        // Normalize the movement direction vector
        Vector3 normalizedMovementDirection = movementDirection.normalized;

        // Calculate the dot product between the normalized movement direction and the target direction
        float dotProduct = Vector3.Dot(normalizedMovementDirection, targetDirection);

        // Return true if the dot product is greater than or equal to the direction tolerance angle, otherwise false
        return dotProduct >= directionToleranceAngle;
    }

    public void UpdateNavigationPrompt(float incorrectDistance, EscapeDirection direction, ref HintState currentHintState)
    {
        if (incorrectDistance >= hintTriggerThreshold)
        {
            if (currentHintState == HintState.Hint) return;
            promptController.DisplayHint();
            currentHintState = HintState.Hint;
            Debug.Log("Hint displayed due to wrong direction travel.");
        }
        else
        {
            if (currentHintState == HintState.Direction) return;
            promptController.DirectionPrompt(GetDirectionText(direction));
            currentHintState = HintState.Direction;
            Debug.Log($"Direction prompt updated to: {direction}");
        }
    }

    public void PromptNavigation(ref EscapeDirection currentEscapeDirection, ref Vector3 targetDirection)
    {
        currentEscapeDirection = Random.value > 0.5f ? EscapeDirection.North : EscapeDirection.South;
        targetDirection = currentEscapeDirection == EscapeDirection.North ? Vector3.forward : Vector3.back;
        promptController.DirectionPrompt(GetDirectionText(currentEscapeDirection));
        Debug.Log($"New navigation direction: {GetDirectionText(currentEscapeDirection)}");
    }

    public void UpdateDirectionFeedback()
    {
        promptController.UpdateDirectionFeedback(isCorrectDirection);
    }

    private string GetDirectionText(EscapeDirection direction)
    {
        switch (direction)
        {
            case EscapeDirection.North:
                return "North";
            case EscapeDirection.South:
                return "South";
            default:
                return "None";
        }
    }
}