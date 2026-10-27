using UnityEngine;

/// <summary>
/// Trigger sensor placed directly on the door keyhole slot.
/// When the brass key enters the trigger volume, unlocks the door via DoorController.
/// </summary>
public class DoorKeyholeTrigger : MonoBehaviour
{
    public DoorController doorController;

    private void OnTriggerEnter(Collider other)
    {
        if (doorController != null && doorController.isLocked)
        {
            KeyController key = other.GetComponent<KeyController>() ?? other.GetComponentInParent<KeyController>();
            if (key != null && !key.hasBeenUsed)
            {
                doorController.UnlockDoor(key);
            }
        }
    }
}
