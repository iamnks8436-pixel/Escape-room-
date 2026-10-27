using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UIManager manages all VR-compatible world-space and screen user interfaces
/// in "VR Escape Room — The Lost Relic" (Phase 16).
/// 
/// Panels:
/// - Intro Screen ("VR ESCAPE ROOM CHALLENGE")
/// - In-Game HUD ("TIME: 05:00", "CLUES: 0/3", "SCORE: 1000")
/// - Clue Inspection Overlay (Title, Body, Digit Reveal Banner, [CONTINUE])
/// - All Clues Found Banner ("THE COMPLETE CODE IS: 5 - 2 - 7")
/// - Toast Notification (e.g. "BRASS KEY ACQUIRED")
/// - Victory Screen (Final Time, Final Score, [RESTART])
/// - Defeat Screen ("TIME'S UP!", [RESTART])
/// - Pause Screen ("GAME PAUSED", [RESUME], [RESTART])
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI Panels")]
    public GameObject introPanel;
    public GameObject hudPanel;
    public GameObject clueInspectPanel;
    public GameObject allCluesPanel;
    public GameObject toastPanel;
    public GameObject winPanel;
    public GameObject losePanel;
    public GameObject pausePanel;

    [Header("Intro Elements")]
    public TextMeshProUGUI introTitleText;
    public TextMeshProUGUI introSubtitleText;
    public Button startButton;

    [Header("HUD Elements")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI cluesText;
    public TextMeshProUGUI scoreText;

    [Header("Clue Inspection Elements")]
    public TextMeshProUGUI clueTitleText;
    public TextMeshProUGUI clueBodyText;
    public TextMeshProUGUI clueDigitText;
    public Button clueContinueButton;

    [Header("All Clues Found Elements")]
    public TextMeshProUGUI allCluesCodeText;
    public Button allCluesContinueButton;

    [Header("Toast Notification Elements")]
    public TextMeshProUGUI toastMessageText;

    [Header("Win Elements")]
    public TextMeshProUGUI winTitleText;
    public TextMeshProUGUI winScoreText;
    public TextMeshProUGUI winTimeText;
    public Button winRestartButton;

    [Header("Lose Elements")]
    public TextMeshProUGUI loseTitleText;
    public TextMeshProUGUI loseSubtitleText;
    public Button loseRestartButton;

    [Header("Pause Elements")]
    public Button pauseResumeButton;
    public Button pauseRestartButton;

    private Coroutine toastCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Hook up button click listeners
        if (startButton != null) startButton.onClick.AddListener(OnStartButtonClicked);
        if (clueContinueButton != null) clueContinueButton.onClick.AddListener(HideClueInspection);
        if (allCluesContinueButton != null) allCluesContinueButton.onClick.AddListener(HideAllCluesFound);
        if (winRestartButton != null) winRestartButton.onClick.AddListener(OnRestartButtonClicked);
        if (loseRestartButton != null) loseRestartButton.onClick.AddListener(OnRestartButtonClicked);
        if (pauseResumeButton != null) pauseResumeButton.onClick.AddListener(OnResumeButtonClicked);
        if (pauseRestartButton != null) pauseRestartButton.onClick.AddListener(OnRestartButtonClicked);
    }

    /// <summary>
    /// Displays the opening intro screen.
    /// </summary>
    public void ShowIntro()
    {
        SetPanelActive(introPanel, true);
        SetPanelActive(hudPanel, false);
        SetPanelActive(clueInspectPanel, false);
        SetPanelActive(allCluesPanel, false);
        SetPanelActive(toastPanel, false);
        SetPanelActive(winPanel, false);
        SetPanelActive(losePanel, false);
        SetPanelActive(pausePanel, false);

        if (introTitleText != null)
        {
            introTitleText.text = "VR ESCAPE ROOM CHALLENGE";
        }

        if (introSubtitleText != null)
        {
            introSubtitleText.text = "Find the combination and escape before time runs out.\n\nExplore -> Discover clues -> Solve code -> Open relic box -> Obtain key -> Unlock door -> Escape";
        }
    }

    /// <summary>
    /// Shows the active in-game HUD (countdown timer, clues count, score).
    /// </summary>
    public void ShowHUD()
    {
        SetPanelActive(introPanel, false);
        SetPanelActive(hudPanel, true);
        SetPanelActive(winPanel, false);
        SetPanelActive(losePanel, false);
        SetPanelActive(pausePanel, false);
    }

    /// <summary>
    /// Updates the clue discovery counter on the HUD (e.g. "CLUES: 1/3").
    /// </summary>
    public void UpdateCluesDisplay(int current, int total)
    {
        if (cluesText != null)
        {
            cluesText.text = $"CLUES: {current}/{total}";
        }
    }

    /// <summary>
    /// Displays the clue reading overlay panel.
    /// </summary>
    public void ShowClueInspection(string title, string body, string digitHint)
    {
        SetPanelActive(clueInspectPanel, true);

        if (clueTitleText != null) clueTitleText.text = title;
        if (clueBodyText != null) clueBodyText.text = body;
        if (clueDigitText != null) clueDigitText.text = digitHint;
    }

    public void HideClueInspection()
    {
        SetPanelActive(clueInspectPanel, false);
        if (SoundManager.Instance != null) SoundManager.Instance.PlayButtonClick();
    }

    /// <summary>
    /// Displays the popup when all 3 clues have been collected.
    /// </summary>
    public void ShowAllCluesFound(string completeCode)
    {
        SetPanelActive(allCluesPanel, true);

        if (allCluesCodeText != null)
        {
            allCluesCodeText.text = $"ALL CLUES FOUND\n\nTHE COMPLETE CODE IS:\n\n{completeCode}";
        }
    }

    public void HideAllCluesFound()
    {
        SetPanelActive(allCluesPanel, false);
        if (SoundManager.Instance != null) SoundManager.Instance.PlayButtonClick();
    }

    /// <summary>
    /// Shows a temporary banner notification (e.g. "BRASS KEY ACQUIRED").
    /// </summary>
    public void ShowToast(string message, float duration = 3.0f)
    {
        if (toastPanel == null || toastMessageText == null) return;

        if (toastCoroutine != null) StopCoroutine(toastCoroutine);
        toastCoroutine = StartCoroutine(ToastRoutine(message, duration));
    }

    private IEnumerator ToastRoutine(string message, float duration)
    {
        toastMessageText.text = message;
        SetPanelActive(toastPanel, true);
        yield return new WaitForSeconds(duration);
        SetPanelActive(toastPanel, false);
    }

    /// <summary>
    /// Displays the victory screen upon stepping through the exit door into freedom.
    /// </summary>
    public void ShowWinScreen(int finalScore, float timeTaken)
    {
        SetPanelActive(introPanel, false);
        SetPanelActive(hudPanel, false);
        SetPanelActive(clueInspectPanel, false);
        SetPanelActive(allCluesPanel, false);
        SetPanelActive(winPanel, true);
        SetPanelActive(losePanel, false);
        SetPanelActive(pausePanel, false);

        int minutes = Mathf.FloorToInt(timeTaken / 60f);
        int seconds = Mathf.FloorToInt(timeTaken % 60f);

        if (winTitleText != null) winTitleText.text = "VICTORY!\nESCAPED!";
        if (winTimeText != null) winTimeText.text = $"Final Time: {minutes:00}:{seconds:00}";
        if (winScoreText != null) winScoreText.text = $"Score: {finalScore}";

        // Reposition canvas to face the escaping player in the exterior garden
        Canvas canvas = GetComponentInParent<Canvas>() ?? Object.FindFirstObjectByType<Canvas>();
        if (canvas != null)
        {
            canvas.transform.position = new Vector3(0f, 1.8f, -6.5f);
            canvas.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        }
    }

    /// <summary>
    /// Displays the defeat screen when the timer reaches 00:00.
    /// </summary>
    public void ShowLoseScreen()
    {
        SetPanelActive(introPanel, false);
        SetPanelActive(hudPanel, false);
        SetPanelActive(clueInspectPanel, false);
        SetPanelActive(allCluesPanel, false);
        SetPanelActive(winPanel, false);
        SetPanelActive(losePanel, true);
        SetPanelActive(pausePanel, false);

        if (loseTitleText != null) loseTitleText.text = "TIME'S UP!\nYOU ARE TRAPPED.";
        if (loseSubtitleText != null) loseSubtitleText.text = "You failed to escape the chamber in time.";
    }

    /// <summary>
    /// Displays the pause screen.
    /// </summary>
    public void ShowPauseScreen()
    {
        SetPanelActive(pausePanel, true);
    }

    // --- Button Callbacks ---
    public void OnStartButtonClicked()
    {
        if (SoundManager.Instance != null) SoundManager.Instance.PlayButtonClick();
        if (GameManager.Instance != null) GameManager.Instance.StartGame();
    }

    public void OnResumeButtonClicked()
    {
        if (SoundManager.Instance != null) SoundManager.Instance.PlayButtonClick();
        if (GameManager.Instance != null) GameManager.Instance.ResumeGame();
    }

    public void OnRestartButtonClicked()
    {
        if (SoundManager.Instance != null) SoundManager.Instance.PlayButtonClick();
        if (GameManager.Instance != null) GameManager.Instance.RestartGame();
    }

    private void SetPanelActive(GameObject panel, bool active)
    {
        if (panel != null)
        {
            panel.SetActive(active);
        }
    }
}
