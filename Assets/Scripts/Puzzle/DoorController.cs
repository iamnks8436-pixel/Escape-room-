using System.Collections;
using UnityEngine;

/// <summary>
/// DoorController manages the heavy exit door of the stone chamber (Storyboard Panels 7 & 8).
/// Starts locked. When the brass key is inserted into the keyhole:
/// - Snaps key into position and disables its physics
/// - Plays heavy deadbolt unlock sound and swinging door creak
/// - Swings open ~95 degrees smoothly via Quaternion Slerp
/// - Floods the doorway with bright golden sunlight (exteriorLight)
/// - Enables the EscapeZone trigger leading to freedom
/// </summary>
public class DoorController : MonoBehaviour, IInteractable
{
    [Header("Door Components")]
    [Tooltip("The swinging door panel Transform (pivots on hinge)")]
    public Transform doorPanel;
    [Tooltip("Socket where key will snap into place")]
    public Transform keyholeTransform;
    [Tooltip("Trigger collider in doorway detecting player escape")]
    public Collider escapeTrigger;

    [Header("Exterior Escape Lighting")]
    [Tooltip("Bright exterior sunlight that shines through open doorway")]
    public Light exteriorLight;
    public float targetLightIntensity = 6.0f;

    [Header("Animation Settings")]
    [Tooltip("Target rotation around Y axis when door swings open")]
    public float openAngleY = 95f;
    public float openDuration = 1.5f;

    [Header("Runtime State")]
    public bool isLocked = true;
    public bool isOpen = false;

    private Quaternion closedRotation;
    private Quaternion openRotation;

    public string InteractionPrompt
    {
        get
        {
            if (isOpen) return "";
            VRDesktopTester tester = Object.FindFirstObjectByType<VRDesktopTester>();
            bool isHoldingKey = (tester != null && tester.heldObject != null && tester.heldObject.GetComponent<KeyController>() != null);
            return isHoldingKey ? "★ [Left Click / E] Insert Key into Lock & Escape! ★" : "Locked Exit Door (Requires Brass Key from chest)";
        }
    }

    private void Awake()
    {
        if (doorPanel != null)
        {
            closedRotation = doorPanel.localRotation;
            openRotation = closedRotation * Quaternion.Euler(0f, openAngleY, 0f);
        }

        if (exteriorLight != null)
        {
            exteriorLight.intensity = 0.2f;
        }

        if (escapeTrigger != null)
        {
            escapeTrigger.enabled = false;
        }
    }

    /// <summary>
    /// Implementation of IInteractable interface.
    /// </summary>
    public void Interact(GameObject user)
    {
        if (!isLocked) return;

        VRDesktopTester tester = user.GetComponent<VRDesktopTester>() ?? Object.FindFirstObjectByType<VRDesktopTester>();
        if (tester != null && tester.heldObject != null)
        {
            KeyController key = tester.heldObject.GetComponent<KeyController>();
            if (key != null && !key.hasBeenUsed)
            {
                tester.DropHeldObject();
                UnlockDoor(key);
            }
        }
    }

    /// <summary>
    /// Unlocks the door using the brass key, opens door, and turns on exterior sunlight.
    /// </summary>
    public void UnlockDoor(KeyController key)
    {
        if (!isLocked) return;

        isLocked = false;
        isOpen = true;

        if (key != null)
        {
            key.ConsumeKey(keyholeTransform);
        }

        // Play unlock and door opening sound
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayDoorUnlock();
            SoundManager.Instance.PlayDoorOpen();
        }

        // Animate door opening
        if (doorPanel != null)
        {
            StartCoroutine(AnimateDoorOpen());
        }

        // Enable escape zone
        if (escapeTrigger != null)
        {
            escapeTrigger.enabled = true;
        }

        Debug.Log("[DoorController] Final door unlocked! Bright light reveals the path to freedom.");
    }

    private IEnumerator AnimateDoorOpen()
    {
        float elapsed = 0f;
        float startIntensity = (exteriorLight != null) ? exteriorLight.intensity : 0f;

        while (elapsed < openDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / openDuration);

            if (doorPanel != null)
            {
                doorPanel.localRotation = Quaternion.Slerp(closedRotation, openRotation, t);
            }

            if (exteriorLight != null)
            {
                exteriorLight.intensity = Mathf.Lerp(startIntensity, targetLightIntensity, t);
            }

            yield return null;
        }

        if (doorPanel != null) doorPanel.localRotation = openRotation;
        if (exteriorLight != null) exteriorLight.intensity = targetLightIntensity;
    }
}
