using UnityEngine;
using TMPro;

/// <summary>
/// TimerManager manages the 5-minute (300 seconds) countdown clock for the escape room.
/// Decreases each second during active gameplay and notifies GameManager when time runs out.
/// Format: MM:SS (e.g. 05:00, 04:32). Stops on Win or Loss, preventing negative time.
/// </summary>
public class TimerManager : MonoBehaviour
{
    public static TimerManager Instance { get; private set; }

    [Header("Timer Configuration")]
    [Tooltip("Total countdown duration in seconds (Default: 300s = 5 minutes)")]
    [SerializeField] private float totalDuration = 300f;
    public float TotalDuration => totalDuration;

    [Header("UI References (Optional)")]
    [Tooltip("TextMeshPro UGUI component on the HUD canvas")]
    public TextMeshProUGUI timerTextUGUI;
    [Tooltip("3D TextMeshPro component mounted in world space")]
    public TextMeshPro timerText3D;

    [Header("Runtime State")]
    public float timeRemaining;
    public bool isRunning = false;

    /// <summary>
    /// Total seconds elapsed since active gameplay started.
    /// </summary>
    public float ElapsedTime => Mathf.Max(0f, totalDuration - timeRemaining);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        timeRemaining = totalDuration;
    }

    private void Start()
    {
        UpdateDisplay();
    }

    private void Update()
    {
        if (!isRunning) return;

        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            isRunning = false;
            UpdateDisplay();

            // Notify GameManager that time is up
            if (GameManager.Instance != null)
            {
                GameManager.Instance.LoseGame();
            }
        }
        else
        {
            UpdateDisplay();
        }
    }

    /// <summary>
    /// Starts or resumes the countdown timer.
    /// </summary>
    public void StartTimer()
    {
        isRunning = true;
    }

    /// <summary>
    /// Pauses or stops the timer (on pause, victory, or game over).
    /// </summary>
    public void StopTimer()
    {
        isRunning = false;
    }

    /// <summary>
    /// Resets the timer back to its initial full duration (300s).
    /// </summary>
    public void ResetTimer()
    {
        timeRemaining = totalDuration;
        isRunning = false;
        UpdateDisplay();
    }

    /// <summary>
    /// Returns the formatted time string (e.g. "05:00", "03:42").
    /// </summary>
    public string GetFormattedTime()
    {
        int minutes = Mathf.FloorToInt(timeRemaining / 60f);
        int seconds = Mathf.FloorToInt(timeRemaining % 60f);
        return $"{minutes:00}:{seconds:00}";
    }

    /// <summary>
    /// Updates all assigned timer text components and applies urgent red color when under 60 seconds.
    /// </summary>
    private void UpdateDisplay()
    {
        string formatted = GetFormattedTime();
        Color displayColor = (timeRemaining <= 60f && timeRemaining > 0f) ? Color.red : Color.white;

        if (timerTextUGUI != null)
        {
            timerTextUGUI.text = $"TIME: {formatted}";
            timerTextUGUI.color = displayColor;
        }

        if (timerText3D != null)
        {
            timerText3D.text = $"TIME: {formatted}";
            timerText3D.color = displayColor;
        }
    }
}
