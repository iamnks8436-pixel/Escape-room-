using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// KeypadController manages the combination lock on the wooden puzzle chest (Storyboard Panel 5).
/// Validates player input against the secret combination: 527.
/// 
/// Requirements (Phase 9):
/// - Configurable passcode: correctCombination = "527"
/// - 3x4 layout (1-9, 0, Clear, Enter)
/// - Digital LCD display starts with "_ _ _"
/// - Incorrect submission displays "INCORRECT CODE" in red and clears
/// - Correct submission displays "CODE ACCEPTED" then "OPEN" in green and unlocks BoxController
/// - Supports Desktop number keys, ENTER, and CLEAR/Backspace
/// </summary>
public class KeypadController : MonoBehaviour
{
    [Header("Combination Settings")]
    [Tooltip("The secret passcode required to open the chest")]
    [SerializeField] private string correctCombination = "527";
    public string CorrectCombination => correctCombination;

    [SerializeField] private int maxCodeLength = 3;

    [Header("Box Target")]
    [Tooltip("Reference to the locked chest to open when combination matches")]
    public BoxController targetBox;

    [Header("Display References")]
    public TextMeshPro screenText3D;
    public TextMeshProUGUI screenTextUGUI;

    [Header("Display Colors")]
    public Color defaultDisplayColor = Color.cyan;
    public Color successDisplayColor = Color.green;
    public Color errorDisplayColor = Color.red;

    [Header("Runtime State")]
    public string currentInput = "";
    public bool isUnlocked = false;
    private bool isProcessing = false;

    private void Start()
    {
        UpdateDisplay("_ _ _", defaultDisplayColor);
    }

    private void Update()
    {
        if (isUnlocked || isProcessing) return;

        // Support direct keyboard input on Desktop
        for (int i = 0; i <= 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0 + i) || Input.GetKeyDown(KeyCode.Keypad0 + i))
            {
                OnButtonPressed(i.ToString());
                return;
            }
        }

        if (Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.Backspace))
        {
            ClearInput();
        }
        else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            OnButtonPressed("ENTER");
        }
    }

    /// <summary>
    /// Called when any keypad button is pressed (via VR poke, Ray, or Mouse click).
    /// </summary>
    public void OnButtonPressed(string digit)
    {
        if (isUnlocked || isProcessing) return;

        // Play tactile button click
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayButtonClick();
        }

        string upper = digit.ToUpper();

        // Handle Clear button
        if (upper == "C" || upper == "CLR" || upper == "CLEAR")
        {
            ClearInput();
            return;
        }

        // Handle Enter / Confirm button
        if (upper == "E" || upper == "ENT" || upper == "ENTER")
        {
            if (currentInput.Length == maxCodeLength)
            {
                StartCoroutine(CheckCodeRoutine());
            }
            return;
        }

        // Append numeric digit
        if (currentInput.Length < maxCodeLength && char.IsDigit(digit[0]))
        {
            currentInput += digit;
            RenderCurrentInput();

            // When 3 digits are entered, auto-evaluate or wait for Enter
            if (currentInput.Length == maxCodeLength)
            {
                StartCoroutine(CheckCodeRoutine());
            }
        }
    }

    /// <summary>
    /// Renders entered digits with placeholder underscores (e.g. "5 _ _").
    /// </summary>
    private void RenderCurrentInput()
    {
        string display = "";
        for (int i = 0; i < maxCodeLength; i++)
        {
            if (i < currentInput.Length)
            {
                display += currentInput[i] + " ";
            }
            else
            {
                display += "_ ";
            }
        }
        UpdateDisplay(display.TrimEnd(), defaultDisplayColor);
    }

    private IEnumerator CheckCodeRoutine()
    {
        isProcessing = true;
        yield return new WaitForSeconds(0.25f);

        if (currentInput == correctCombination)
        {
            // --- SUCCESS ---
            isUnlocked = true;
            UpdateDisplay("CODE ACCEPTED", successDisplayColor);

            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayCorrectCode();
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnCodeAccepted();
            }

            yield return new WaitForSeconds(0.6f);
            UpdateDisplay("OPEN", successDisplayColor);

            if (targetBox != null)
            {
                targetBox.UnlockBox();
            }

            Debug.Log("[KeypadController] Correct passcode (527) accepted! Chest opening.");
        }
        else
        {
            // --- INCORRECT ---
            UpdateDisplay("INCORRECT CODE", errorDisplayColor);

            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayWrongCode();
            }

            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.RecordWrongAttempt();
            }

            Debug.Log($"[KeypadController] Incorrect code '{currentInput}' entered. Try again!");

            yield return new WaitForSeconds(1.2f);
            ClearInput();
            isProcessing = false;
        }
    }

    public void ClearInput()
    {
        currentInput = "";
        UpdateDisplay("_ _ _", defaultDisplayColor);
    }

    public void UpdateDisplay(string text, Color color)
    {
        if (screenText3D != null)
        {
            screenText3D.text = text;
            screenText3D.color = color;
        }

        if (screenTextUGUI != null)
        {
            screenTextUGUI.text = text;
            screenTextUGUI.color = color;
        }
    }
}
