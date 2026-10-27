using System.Collections;
using UnityEngine;

/// <summary>
/// KeypadButton represents a single interactive button cap on the keypad housing.
/// Supports VR hand poke, Ray Interactor selection, and Desktop mouse clicks.
/// Provides visual depression travel and tactile audio click.
/// </summary>
public class KeypadButton : MonoBehaviour, IInteractable
{
    [Header("Button Identity")]
    [Tooltip("The digit or command sent to the KeypadController (e.g. '5', 'C', 'ENTER')")]
    public string keyCharacter = "0";

    [Header("Controller Link")]
    public KeypadController keypadController;

    [Header("Visual Depression Settings")]
    public Renderer buttonRenderer;
    public Color normalColor = new Color(0.85f, 0.85f, 0.85f);
    public Color pressColor = new Color(1.0f, 0.80f, 0.25f);
    public float pressDepth = 0.008f;
    public float pressDuration = 0.12f;

    private Vector3 originalLocalPos;
    private bool isPressed = false;

    public string InteractionPrompt => $"[Left Click] Press Button '{keyCharacter}'";

    private void Awake()
    {
        originalLocalPos = transform.localPosition;

        if (buttonRenderer == null)
        {
            buttonRenderer = GetComponent<Renderer>();
        }

        if (buttonRenderer != null && buttonRenderer.material != null)
        {
            buttonRenderer.material.color = normalColor;
        }

        if (keypadController == null)
        {
            keypadController = GetComponentInParent<KeypadController>();
        }
    }

    /// <summary>
    /// Implementation of IInteractable interface.
    /// </summary>
    public void Interact(GameObject user)
    {
        Press();
    }

    /// <summary>
    /// Executes the physical depression animation and notifies KeypadController.
    /// </summary>
    public void Press()
    {
        if (isPressed) return;
        StartCoroutine(PressFeedbackRoutine());

        if (keypadController != null)
        {
            keypadController.OnButtonPressed(keyCharacter);
        }
    }

    private IEnumerator PressFeedbackRoutine()
    {
        isPressed = true;

        // Visual press down
        transform.localPosition = originalLocalPos - new Vector3(0f, pressDepth, 0f);
        if (buttonRenderer != null && buttonRenderer.material != null)
        {
            buttonRenderer.material.color = pressColor;
        }

        yield return new WaitForSeconds(pressDuration);

        // Visual return
        transform.localPosition = originalLocalPos;
        if (buttonRenderer != null && buttonRenderer.material != null)
        {
            buttonRenderer.material.color = normalColor;
        }

        isPressed = false;
    }

    // --- Desktop Mouse Click Fallback ---
    private void OnMouseDown()
    {
        Press();
    }

    // --- VR Hand / Collider Touch Fallback ---
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.name.ToLower().Contains("hand") || other.name.ToLower().Contains("controller") || other.name.ToLower().Contains("interactor"))
        {
            Press();
        }
    }
}
