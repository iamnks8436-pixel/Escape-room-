using UnityEngine;
using TMPro;

/// <summary>
/// ClueInteraction handles inspecting clues in "VR Escape Room — The Lost Relic"
/// (Storyboard Panels 2, 3, and 4).
/// 
/// Supported Clues:
/// - Clue 1: Celestial Star Journal (Bookshelf) -> First Digit = 5
/// - Clue 2: Diary Scrap (Study Desk) -> Second Digit = 2
/// - Clue 3: Ancient Inscription (South Wall) -> Third Digit = 7
/// 
/// Interacting opens a magnified readable UI overlay in the player's view, awards +100 points,
/// and updates the HUD (Clues: X/3) without awarding duplicate points.
/// </summary>
public class ClueInteraction : MonoBehaviour, IInteractable
{
    [Header("Clue Identity")]
    [Tooltip("Unique ID used to prevent duplicate score awards (e.g. Clue_1_Book_Of_Stars)")]
    public string clueId = "Clue_1_Book_Of_Stars";
    public int clueIndex = 1;

    [Header("Display Text")]
    [Tooltip("Title shown in the inspection popup (e.g. 'Celestial Star Journal')")]
    public string clueTitle = "Mysterious Clue";

    [TextArea(3, 6)]
    [Tooltip("Main riddle or lore text")]
    public string clueText = "Look closely at the symbols...";

    [Tooltip("The explicit passcode digit reveal (e.g. 'FIRST DIGIT = 5')")]
    public string digitHint = "FIRST DIGIT = 5";

    [Header("In-World Label (Optional)")]
    public TextMeshPro inWorldText;

    [Header("Visual Feedback")]
    public Renderer objectRenderer;
    public Color normalColor = Color.white;
    public Color highlightColor = new Color(1f, 0.95f, 0.6f);

    [Header("Runtime State")]
    public bool isInspecting = false;

    public string InteractionPrompt => $"[Left Click / E] Inspect \"{clueTitle}\"";

    private void Awake()
    {
        if (objectRenderer == null) objectRenderer = GetComponent<Renderer>();
        if (inWorldText != null) inWorldText.text = clueText;
    }

    /// <summary>
    /// Implementation of IInteractable interface.
    /// </summary>
    public void Interact(GameObject user)
    {
        if (!isInspecting)
        {
            InspectClue();
        }
        else
        {
            StopInspectClue();
        }
    }

    /// <summary>
    /// Displays the magnified legible reading panel, awards clue score, and updates HUD.
    /// </summary>
    public void InspectClue()
    {
        isInspecting = true;

        // Show UI popup
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowClueInspection(clueTitle, clueText, digitHint);
        }

        // Highlight material
        if (objectRenderer != null && objectRenderer.material != null)
        {
            objectRenderer.material.color = highlightColor;
        }

        // Notify GameManager to register clue progression
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterClueDiscovered(clueId, clueTitle, digitHint);
        }

        // Play audio feedback
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayPaperRustle();
            SoundManager.Instance.PlaySecretDiscovered();
        }

        Debug.Log($"[ClueInteraction] Inspecting {clueTitle} -> {digitHint}");
    }

    /// <summary>
    /// Closes the reading overlay and restores normal material color.
    /// </summary>
    public void StopInspectClue()
    {
        isInspecting = false;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.HideClueInspection();
        }

        if (objectRenderer != null && objectRenderer.material != null)
        {
            objectRenderer.material.color = normalColor;
        }
    }

    // --- Desktop Click Fallback ---
    private void OnMouseDown()
    {
        Interact(gameObject);
    }

    // --- Unity XR Interaction Toolkit Hooks ---
    public void OnXRSelectEntered()
    {
        InspectClue();
    }

    public void OnXRSelectExited()
    {
        StopInspectClue();
    }
}
