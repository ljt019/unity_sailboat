using UnityEngine;
using TMPro;

public class PromptController : MonoBehaviour
{
    [Header("Entity References")]
    [SerializeField] private TextMeshProUGUI promptText;

    [Header("Prompt Color Settings")]
    [SerializeField] private Color defaultColor = Color.white;
    [SerializeField] private Color correctDirectionColor = Color.white;

    [Header("Hint Settings")]
    [SerializeField] private float hintDisplayDuration = 5f;

    private string currentDirection;
    private bool isHintDisplayed;
    private float hintDisplayTimer;
    private bool isCurrentlyCorrect = false;

    private void Start()
    {
        ValidateReferences();
        SetupPromptTextPosition();
    }

    private void Update()
    {
        if (isHintDisplayed)
        {
            hintDisplayTimer -= Time.deltaTime;
            if (hintDisplayTimer <= 0)
            {
                HideHint();
            }
        }
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
        if (isCorrectDirection)
        {
            isCurrentlyCorrect = true;
        }
        else
        {
            SetDefaultColor();
            isCurrentlyCorrect = false;
        }
    }

    public bool IsCurrentlyCorrect()
    {
        return isCurrentlyCorrect;
    }

    public void ClearPrompt()
    {
        UpdatePromptText(string.Empty);
        SetDefaultColor();
        isHintDisplayed = false;
        hintDisplayTimer = 0f;
    }

    public void SetDefaultColor()
    {
        promptText.color = defaultColor;
    }

    public void DirectionPrompt(string direction)
    {
        currentDirection = direction;
        UpdatePromptText($"Navigate {direction} to escape the storm!");
        isHintDisplayed = false;
    }

    public void DisplayHint()
    {
        if (!isHintDisplayed)
        {
            string hintText = currentDirection == "North" ? "How can you find Polaris?" : "Where does Orion's sword point?";
            UpdatePromptText($"Navigate {currentDirection} to escape the storm!\nHint: {hintText}");
            isHintDisplayed = true;
            hintDisplayTimer = hintDisplayDuration;
        }
    }

    private void HideHint()
    {
        UpdatePromptText($"Navigate {currentDirection} to escape the storm!");
        isHintDisplayed = false;
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