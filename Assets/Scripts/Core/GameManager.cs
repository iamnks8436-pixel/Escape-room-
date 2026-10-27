using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// GameManager is the central authority for overall game state, progression, and win/loss conditions
/// in "VR Escape Room — The Lost Relic".
/// 
/// States:
/// - Intro: Game start introduction and instructions
/// - Playing: 5-minute countdown running, clues and puzzles interactive
/// - Paused: Gameplay paused
/// - Won: Player entered the escape zone after unlocking the exit door
/// - Lost: Countdown timer reached 00:00 before escaping
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState
    {
        Intro,
        Playing,
        Paused,
        Won,
        Lost
    }

    [Header("Game State")]
    [SerializeField] private GameState currentState = GameState.Intro;
    public GameState CurrentState => currentState;

    [Header("Start Configuration")]
    [Tooltip("If true, automatically starts gameplay after the intro delay")]
    [SerializeField] private bool autoStartAfterIntro = true;
    [SerializeField] private float autoStartDelay = 2.5f;

    [Header("Clue Tracking")]
    [SerializeField] private int totalClues = 3;
    private int cluesDiscoveredCount = 0;
    public int CluesDiscoveredCount => cluesDiscoveredCount;
    public int TotalClues => totalClues;

    private readonly HashSet<string> discoveredClueIds = new HashSet<string>();

    [Header("Developer / Debug Mode")]
    [Tooltip("Enable keyboard shortcut overrides for testing (F1-F5). Disabled by default.")]
    [SerializeField] private bool developerMode = false;
    public bool DeveloperMode => developerMode;

    [Header("Optional References")]
    [Tooltip("Optional parent object of interactables to disable on game over")]
    [SerializeField] private GameObject interactablesParent;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        SetState(GameState.Intro);

        if (autoStartAfterIntro)
        {
            Invoke(nameof(StartGame), autoStartDelay);
        }
    }

    private void Update()
    {
        // Handle developer mode debug shortcuts
        if (developerMode)
        {
            HandleDeveloperShortcuts();
        }

        // Handle Escape key to Pause/Resume if playing
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentState == GameState.Playing)
            {
                PauseGame();
            }
            else if (currentState == GameState.Paused)
            {
                ResumeGame();
            }
        }
    }

    /// <summary>
    /// Begins active gameplay: starts the 5-minute countdown clock and brings up the HUD.
    /// </summary>
    public void StartGame()
    {
        if (currentState != GameState.Intro && currentState != GameState.Paused) return;

        SetState(GameState.Playing);

        if (TimerManager.Instance != null)
        {
            TimerManager.Instance.StartTimer();
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowHUD();
            UIManager.Instance.UpdateCluesDisplay(cluesDiscoveredCount, totalClues);
        }

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayAmbient();
        }

        Debug.Log("[GameManager] Escape Room Challenge begun! Timer is running (05:00).");
    }

    /// <summary>
    /// Pauses gameplay and opens the pause UI.
    /// </summary>
    public void PauseGame()
    {
        if (currentState != GameState.Playing) return;

        SetState(GameState.Paused);

        if (TimerManager.Instance != null)
        {
            TimerManager.Instance.StopTimer();
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowPauseScreen();
        }

        Debug.Log("[GameManager] Game Paused.");
    }

    /// <summary>
    /// Resumes active gameplay from pause.
    /// </summary>
    public void ResumeGame()
    {
        if (currentState != GameState.Paused) return;

        SetState(GameState.Playing);

        if (TimerManager.Instance != null)
        {
            TimerManager.Instance.StartTimer();
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowHUD();
        }

        Debug.Log("[GameManager] Game Resumed.");
    }

    /// <summary>
    /// Records the discovery of a clue (Panel 2, 3, or 4).
    /// Prevents repeated rewards for the same clue.
    /// </summary>
    public bool RegisterClueDiscovered(string clueId, string clueTitle, string digitHint)
    {
        if (string.IsNullOrEmpty(clueId)) return false;

        if (discoveredClueIds.Contains(clueId))
        {
            // Already discovered — no duplicate points
            return false;
        }

        discoveredClueIds.Add(clueId);
        cluesDiscoveredCount++;

        // Award +100 bonus in ScoreManager
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AwardClueBonus();
        }

        // Update HUD
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateCluesDisplay(cluesDiscoveredCount, totalClues);
        }

        Debug.Log($"[GameManager] Clue discovered ({cluesDiscoveredCount}/{totalClues}): {clueTitle} -> {digitHint}");

        // If all 3 clues found, present the complete code reveal banner
        if (cluesDiscoveredCount >= totalClues)
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowAllCluesFound("5 - 2 - 7");
            }

            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySecretDiscovered();
            }
        }

        return true;
    }

    /// <summary>
    /// Called when the player correctly enters 527 on the keypad.
    /// </summary>
    public void OnCodeAccepted()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AwardCodeBonus();
        }
        Debug.Log("[GameManager] Code 527 accepted! Chest unlocking.");
    }

    /// <summary>
    /// Triggered when the player steps into the EscapeZone outside the doorway.
    /// </summary>
    public void WinGame()
    {
        if (currentState != GameState.Playing) return;

        SetState(GameState.Won);

        // 1. Stop timer
        float timeTaken = 0f;
        if (TimerManager.Instance != null)
        {
            TimerManager.Instance.StopTimer();
            timeTaken = TimerManager.Instance.ElapsedTime;
        }

        // 2. Compute final score (including time bonus)
        int finalScore = 1000;
        if (ScoreManager.Instance != null)
        {
            finalScore = ScoreManager.Instance.CalculateFinalScore();
        }

        // 3. Play victory audio
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayVictory();
        }

        // 4. Show Win UI
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowWinScreen(finalScore, timeTaken);
        }

        Debug.Log($"[GameManager] VICTORY! Player escaped in {timeTaken:F1}s with Score: {finalScore}");
    }

    /// <summary>
    /// Triggered when the countdown timer reaches 00:00.
    /// </summary>
    public void LoseGame()
    {
        if (currentState != GameState.Playing) return;

        SetState(GameState.Lost);

        // Stop timer
        if (TimerManager.Instance != null)
        {
            TimerManager.Instance.StopTimer();
        }

        // Disable interactables
        if (interactablesParent != null)
        {
            interactablesParent.SetActive(false);
        }

        // Play defeat sound
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayDefeat();
        }

        // Show Lose UI
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowLoseScreen();
        }

        Debug.Log("[GameManager] TIME'S UP! Game Over. The player is trapped.");
    }

    /// <summary>
    /// Alias for LoseGame() for backwards compatibility.
    /// </summary>
    public void GameOver()
    {
        LoseGame();
    }

    /// <summary>
    /// Fully resets all state and reloads the active scene cleanly.
    /// </summary>
    public void RestartGame()
    {
        Debug.Log("[GameManager] Restarting Escape Room challenge...");
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void SetState(GameState newState)
    {
        currentState = newState;

        if (newState == GameState.Intro && UIManager.Instance != null)
        {
            UIManager.Instance.ShowIntro();
        }
    }

    /// <summary>
    /// Developer mode shortcuts for rapid testing:
    /// F1: Reveal all clues
    /// F2: Auto-enter code 527 into Keypad
    /// F3: Unlock box & reveal brass key
    /// F4: Unlock and open exit door
    /// F5: Trigger WinGame immediately
    /// </summary>
    private void HandleDeveloperShortcuts()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            Debug.Log("[DeveloperMode] F1: Revealing all clues...");
            RegisterClueDiscovered("Clue_1_Book_Of_Stars", "Celestial Star Journal", "FIRST DIGIT = 5");
            RegisterClueDiscovered("Clue_2_Parchment_Note", "Diary Scrap", "SECOND DIGIT = 2");
            RegisterClueDiscovered("Clue_3_Wall_Carving", "Chiseled Wall Inscription", "THIRD DIGIT = 7");
        }
        else if (Input.GetKeyDown(KeyCode.F2))
        {
            Debug.Log("[DeveloperMode] F2: Setting keypad to 527...");
            KeypadController keypad = Object.FindFirstObjectByType<KeypadController>();
            if (keypad != null)
            {
                keypad.ClearInput();
                keypad.OnButtonPressed("5");
                keypad.OnButtonPressed("2");
                keypad.OnButtonPressed("7");
            }
        }
        else if (Input.GetKeyDown(KeyCode.F3))
        {
            Debug.Log("[DeveloperMode] F3: Opening puzzle box & unlocking key...");
            BoxController box = Object.FindFirstObjectByType<BoxController>();
            if (box != null) box.UnlockBox();
        }
        else if (Input.GetKeyDown(KeyCode.F4))
        {
            Debug.Log("[DeveloperMode] F4: Unlocking exit door...");
            DoorController door = Object.FindFirstObjectByType<DoorController>();
            if (door != null) door.UnlockDoor(null);
        }
        else if (Input.GetKeyDown(KeyCode.F5))
        {
            Debug.Log("[DeveloperMode] F5: Forcing win game...");
            WinGame();
        }
    }
}
