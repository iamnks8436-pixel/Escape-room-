using UnityEngine;
using TMPro;

/// <summary>
/// ScoreManager tracks and computes player score in "VR Escape Room — The Lost Relic".
/// 
/// Scoring Rules (Phase 15):
/// - Base Starting Score: 1000 pts
/// - Clue Discovered: +100 pts each (awarded once per unique clue)
/// - Correct Keypad Code: +200 pts
/// - Time Bonus: Remaining Seconds × Configurable Multiplier (e.g. 2 pts/sec)
/// - Incorrect Code Penalty: -50 pts
/// </summary>
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("Score Configuration")]
    [Tooltip("Initial base score awarded at the start of the challenge")]
    [SerializeField] private int baseScore = 1000;

    [Tooltip("Bonus awarded when a new clue is discovered")]
    [SerializeField] private int clueBonus = 100;

    [Tooltip("Bonus awarded when correct passcode (527) is entered")]
    [SerializeField] private int correctCodeBonus = 200;

    [Tooltip("Multiplier for remaining seconds upon winning (e.g. 2 points per remaining second)")]
    [SerializeField] private int timeBonusMultiplier = 2;

    [Tooltip("Points deducted for each incorrect keypad submission")]
    [SerializeField] private int penaltyPerWrongAttempt = 50;

    [Header("UI References (Optional)")]
    public TextMeshProUGUI scoreTextUGUI;
    public TextMeshPro scoreText3D;

    [Header("Runtime State")]
    [SerializeField] private int currentScore = 1000;
    public int CurrentScore => currentScore;

    [SerializeField] private int cluesAwardedCount = 0;
    [SerializeField] private bool codeBonusAwarded = false;
    [SerializeField] private int wrongAttempts = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        currentScore = baseScore;
    }

    private void Start()
    {
        UpdateDisplay();
    }

    /// <summary>
    /// Awards the +100 point bonus for discovering a unique clue.
    /// </summary>
    public void AwardClueBonus()
    {
        cluesAwardedCount++;
        currentScore += clueBonus;
        UpdateDisplay();
        Debug.Log($"[ScoreManager] Clue bonus awarded (+{clueBonus}). Current score: {currentScore}");
    }

    /// <summary>
    /// Awards the +200 point bonus for entering the correct code into the keypad.
    /// </summary>
    public void AwardCodeBonus()
    {
        if (codeBonusAwarded) return;
        codeBonusAwarded = true;
        currentScore += correctCodeBonus;
        UpdateDisplay();
        Debug.Log($"[ScoreManager] Correct code bonus awarded (+{correctCodeBonus}). Current score: {currentScore}");
    }

    /// <summary>
    /// Records an incorrect keypad submission and applies penalty.
    /// </summary>
    public void RecordWrongAttempt()
    {
        wrongAttempts++;
        currentScore = Mathf.Max(0, currentScore - penaltyPerWrongAttempt);
        UpdateDisplay();
        Debug.Log($"[ScoreManager] Wrong attempt penalty (-{penaltyPerWrongAttempt}). Current score: {currentScore}");
    }

    /// <summary>
    /// Computes and returns the final score upon escaping the room, adding the time speed bonus.
    /// </summary>
    public int CalculateFinalScore()
    {
        float remainingSecs = 0f;
        if (TimerManager.Instance != null)
        {
            remainingSecs = Mathf.Max(0f, TimerManager.Instance.timeRemaining);
        }

        int timeBonus = Mathf.FloorToInt(remainingSecs * timeBonusMultiplier);
        int finalScore = Mathf.Max(0, currentScore + timeBonus);

        currentScore = finalScore;
        UpdateDisplay();

        Debug.Log($"[ScoreManager] Final Score Calculated: {currentScore} (Base + Bonuses + Time Bonus: {timeBonus} pts)");
        return finalScore;
    }

    /// <summary>
    /// Updates all assigned score text elements on the HUD and in world space.
    /// </summary>
    public void UpdateDisplay()
    {
        string text = $"SCORE: {currentScore}";
        if (scoreTextUGUI != null) scoreTextUGUI.text = text;
        if (scoreText3D != null) scoreText3D.text = text;
    }
}
