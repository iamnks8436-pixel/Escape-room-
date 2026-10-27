using UnityEngine;

/// <summary>
/// EscapeZone sits in the sunlit garden outside the open doorway (Storyboard Panel 8).
/// When the player steps through the threshold into this trigger, it triggers GameManager.WinGame().
/// </summary>
public class EscapeZone : MonoBehaviour
{
    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;

        // Check if player entered the zone
        if (other.CompareTag("Player") || other.name.ToLower().Contains("player") || other.name.ToLower().Contains("camera") || other.name.ToLower().Contains("origin"))
        {
            hasTriggered = true;
            if (GameManager.Instance != null)
            {
                GameManager.Instance.WinGame();
            }
        }
    }
}
