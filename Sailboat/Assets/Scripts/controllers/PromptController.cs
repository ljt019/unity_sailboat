using UnityEngine;
using TMPro;

public class PromptController : MonoBehaviour
{
    [Header("Entity References")]
    [SerializeField] private TextMeshProUGUI promptText;

    [Header("Prompt Color Settings")]
    [SerializeField] private readonly Color defaultColor = Color.white;
    [SerializeField] private readonly Color correctDirectionColor = Color.green;

    private void Start()
    {
        ValidateReferences();
        SetupPromptTextPosition();
    }

    private void ValidateReferences()
    {
        if (promptText == null)
        {
            Debug.LogError("Prompt Text is not assigned!");
            enabled = false;
        }
    }

    private void SetupPromptTextPosition()
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

    private void UpdatePromptText(string newText)
    {
        promptText.text = newText;
        promptText.ForceMeshUpdate();
    }

    public void UpdateDirectionFeedback(bool isCorrectDirection)
    {
        promptText.color = isCorrectDirection ? correctDirectionColor : defaultColor;
    }

    public void ClearPrompt()
    {
        UpdatePromptText(string.Empty);
        SetDefaultColor();
    }

    public void SetDefaultColor()
    {
        promptText.color = defaultColor;
    }

    // Prompt Presets
    public void DirectionPrompt(string direction)
    {
        UpdatePromptText($"Navigate {direction} to escape the storm!");
    }

    public void DirectionWithHintPrompt(string direction)
    {
        string hintText = direction == "North" ? "How can you find Polaris?" : "Where does Orion's sword point?";
        UpdatePromptText($"Navigate {direction} to escape the storm!\nHint: {hintText}");
    }

    public void StormApproachingPrompt()
    {
        UpdatePromptText("A storm is approaching! Prepare to navigate!");
    }

    public void SuccessfulNavigationPrompt()
    {
        UpdatePromptText("You've successfully navigated out of the storm! Enjoy the calm seas.");
    }
}