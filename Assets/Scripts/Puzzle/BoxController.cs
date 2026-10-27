using System.Collections;
using UnityEngine;

/// <summary>
/// BoxController manages the locked wooden puzzle chest on the study table (Storyboard Panel 6).
/// Starts locked with the antique brass key resting inside.
/// When unlocked by the KeypadController:
/// - Plays an unlock latch sound and hinge creak
/// - Smoothly pivots lid open ~105 degrees via Quaternion Slerp
/// - Emits golden burst particles and interior warm light
/// - Enables the brass key for VR grab or desktop pickup
/// </summary>
public class BoxController : MonoBehaviour
{
    [Header("Lid Pivot Animation")]
    [Tooltip("Transform of the lid that pivots backwards")]
    public Transform lidTransform;
    [Tooltip("Target rotation angle around the local X axis (default: -105 degrees)")]
    public float openAngleX = -105f;
    [Tooltip("Duration of the lid opening animation in seconds")]
    public float openDuration = 1.2f;

    [Header("Contents")]
    [Tooltip("The brass key GameObject resting inside the chest")]
    public GameObject keyInside;

    [Header("Feedback Effects")]
    [Tooltip("Golden particle emission when unsealed")]
    public ParticleSystem unlockParticles;
    [Tooltip("Interior warm light revealed upon opening")]
    public Light interiorLight;

    [Header("Runtime State")]
    public bool isLocked = true;
    public bool isOpen = false;

    private Quaternion closedRotation;
    private Quaternion openRotation;

    private void Awake()
    {
        if (lidTransform != null)
        {
            closedRotation = lidTransform.localRotation;
            openRotation = closedRotation * Quaternion.Euler(openAngleX, 0f, 0f);
        }

        // Initially disable grabbing of the key until the box has unsealed
        if (keyInside != null)
        {
            Collider keyCol = keyInside.GetComponent<Collider>();
            if (keyCol != null) keyCol.enabled = false;
        }

        if (interiorLight != null)
        {
            interiorLight.enabled = false;
        }
    }

    /// <summary>
    /// Unlocks the chest, opens the lid smoothly, and reveals the brass key.
    /// Called when the player enters 527 on the keypad.
    /// </summary>
    public void UnlockBox()
    {
        if (!isLocked) return;

        isLocked = false;
        isOpen = true;

        // Play unlock audio
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayBoxUnlock();
            SoundManager.Instance.PlayBoxOpen();
        }

        // Play particle effect
        if (unlockParticles != null)
        {
            unlockParticles.Play();
        }

        if (interiorLight != null)
        {
            interiorLight.enabled = true;
        }

        // Animate lid opening smoothly
        if (lidTransform != null)
        {
            StartCoroutine(AnimateLidOpen());
        }

        // Enable key interaction
        if (keyInside != null)
        {
            Collider keyCol = keyInside.GetComponent<Collider>();
            if (keyCol != null) keyCol.enabled = true;
        }

        Debug.Log("[BoxController] Wooden chest unlocked! Antique brass key is now accessible.");
    }

    private IEnumerator AnimateLidOpen()
    {
        float elapsed = 0f;
        while (elapsed < openDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / openDuration);
            lidTransform.localRotation = Quaternion.Slerp(closedRotation, openRotation, t);
            yield return null;
        }
        lidTransform.localRotation = openRotation;
    }
}
