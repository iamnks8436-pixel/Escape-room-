using System.Collections;
using UnityEngine;

/// <summary>
/// ChairController allows the player to sit down at the study desk and stand back up
/// (Storyboard Panel 3).
/// Smoothly transitions the camera height from standing (1.6m) to seated eye level (1.05m)
/// facing the desk props and diary note.
/// </summary>
public class ChairController : MonoBehaviour, IInteractable
{
    [Header("Seat Alignment")]
    [Tooltip("Point where player base will be positioned when seated")]
    public Transform seatPoint;
    public float seatedCameraHeight = 1.05f;
    public float standingCameraHeight = 1.60f;

    [Header("State")]
    public bool isOccupied = false;

    public string InteractionPrompt => isOccupied ? "[Space / W / E] Stand Up" : "[Left Click / E] Sit Down at Desk";

    private Coroutine transitionRoutine;

    private void Awake()
    {
        if (seatPoint == null)
        {
            seatPoint = transform;
        }
    }

    /// <summary>
    /// Implementation of IInteractable interface.
    /// </summary>
    public void Interact(GameObject user)
    {
        VRDesktopTester tester = user.GetComponent<VRDesktopTester>() ?? Object.FindFirstObjectByType<VRDesktopTester>();
        if (tester != null)
        {
            ToggleSit(tester);
        }
    }

    public void ToggleSit(VRDesktopTester player)
    {
        if (isOccupied)
        {
            StandUp(player);
        }
        else
        {
            SitDown(player);
        }
    }

    public void SitDown(VRDesktopTester player)
    {
        if (isOccupied || player == null) return;
        isOccupied = true;

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayChairMove();
        }

        if (transitionRoutine != null) StopCoroutine(transitionRoutine);
        transitionRoutine = StartCoroutine(MovePlayerToSeat(player, true));

        Debug.Log("[ChairController] Player seated at study desk.");
    }

    public void StandUp(VRDesktopTester player)
    {
        if (!isOccupied || player == null) return;
        isOccupied = false;

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayChairMove();
        }

        if (transitionRoutine != null) StopCoroutine(transitionRoutine);
        transitionRoutine = StartCoroutine(MovePlayerToSeat(player, false));

        Debug.Log("[ChairController] Player stood up from chair.");
    }

    private IEnumerator MovePlayerToSeat(VRDesktopTester player, bool sitting)
    {
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        Transform camOffset = player.transform.Find("Camera_Offset");

        Vector3 startPos = player.transform.position;
        Vector3 targetPos;
        Quaternion startRot = player.transform.rotation;
        Quaternion targetRot;

        float startCamY = (camOffset != null) ? camOffset.localPosition.y : standingCameraHeight;
        float targetCamY = sitting ? seatedCameraHeight : standingCameraHeight;

        if (sitting)
        {
            // Position player firmly on the seat facing the desk
            targetPos = new Vector3(seatPoint.position.x, 0.0f, seatPoint.position.z + 0.02f);
            targetRot = Quaternion.Euler(0f, 0f, 0f); // Facing table (+Z)
        }
        else
        {
            // Step back safely behind the chair
            targetPos = new Vector3(seatPoint.position.x, 0.0f, seatPoint.position.z - 0.65f);
            targetRot = startRot;
        }

        float elapsed = 0f;
        float duration = 0.35f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            player.transform.position = Vector3.Lerp(startPos, targetPos, t);
            if (sitting) player.transform.rotation = Quaternion.Slerp(startRot, targetRot, t);

            if (camOffset != null)
            {
                camOffset.localPosition = new Vector3(0f, Mathf.Lerp(startCamY, targetCamY, t), 0f);
            }

            yield return null;
        }

        player.transform.position = targetPos;
        if (camOffset != null) camOffset.localPosition = new Vector3(0f, targetCamY, 0f);

        // Re-enable CharacterController when standing
        if (cc != null) cc.enabled = !sitting;

        // Angle gaze towards desk props when sitting down, reset when standing
        if (sitting) player.SetLookPitch(16f);
        else player.SetLookPitch(0f);
    }
}
