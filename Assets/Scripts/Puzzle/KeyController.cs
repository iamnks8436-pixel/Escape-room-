using UnityEngine;

/// <summary>
/// KeyController manages the antique brass key found inside the unsealed chest (Storyboard Panels 6 & 7).
/// 
/// Requirements (Phase 11):
/// - Starts non-interactable inside the chest until unsealed
/// - Golden point light glow
/// - Interacted via XR Grab in VR or Left Click/E in Desktop mode
/// - Upon pickup: displays "BRASS KEY ACQUIRED" banner and plays chime
/// - Unlocks the final exit door when brought to the keyhole trigger
/// </summary>
public class KeyController : MonoBehaviour, IInteractable
{
    [Header("Key Identity")]
    public string keyId = "EscapeRoomBrassKey";

    [Header("Visual Effects")]
    public Light glowLight;
    public Color glowColor = new Color(1.0f, 0.85f, 0.35f);

    [Header("Runtime State")]
    public bool isGrabbed = false;
    public bool hasBeenUsed = false;

    private Rigidbody rb;
    private Collider col;

    public string InteractionPrompt => hasBeenUsed ? "" : "[Left Click / E] Pick up Brass Key";

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        if (glowLight != null)
        {
            glowLight.color = glowColor;
            glowLight.range = 0.6f;
            glowLight.intensity = 1.5f;
        }
    }

    /// <summary>
    /// Implementation of IInteractable interface.
    /// </summary>
    public void Interact(GameObject user)
    {
        if (hasBeenUsed) return;

        VRDesktopTester tester = user.GetComponent<VRDesktopTester>() ?? Object.FindFirstObjectByType<VRDesktopTester>();
        if (tester != null)
        {
            tester.GrabObject(gameObject);
        }
        OnGrabbed();
    }

    /// <summary>
    /// Invoked when VR Grab or Desktop controller picks up the key.
    /// </summary>
    public void OnGrabbed()
    {
        if (hasBeenUsed) return;
        isGrabbed = true;

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayKeyPickup();
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowToast("BRASS KEY ACQUIRED");
        }

        Debug.Log("[KeyController] Brass Key acquired by player.");
    }

    public void OnReleased()
    {
        isGrabbed = false;
    }

    /// <summary>
    /// Snaps the key into the door lock socket and disables further interaction.
    /// </summary>
    public void ConsumeKey(Transform lockTransform)
    {
        hasBeenUsed = true;
        isGrabbed = false;

        if (lockTransform != null)
        {
            transform.SetParent(lockTransform);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        if (col != null)
        {
            col.enabled = false;
        }

        Debug.Log("[KeyController] Key inserted into door lock successfully.");
    }
}
