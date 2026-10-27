using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// VRDesktopTester provides a complete desktop keyboard and mouse testing system (Phase 2),
/// allowing the escape room to be fully played and completed without a VR headset.
/// 
/// Controls:
/// - WASD: Walk (or stand up when seated)
/// - Mouse: Look around
/// - Left Click: Interact / Sit / Press Keypad / Pick up Key / Read Clue / Unlock Door
/// - E: Interact / Inspect Clue / Stand Up
/// - Right Click: Drop held object
/// - ESC: Pause game / Toggle mouse cursor lock
/// - R: Restart game
/// - Number keys (0-9): Direct keypad entry
/// - ENTER: Submit keypad code
/// </summary>
public class VRDesktopTester : MonoBehaviour
{
    [Header("Locomotion Settings")]
    [Tooltip("Always force desktop controls (WASD + Mouse)")]
    public bool forceDesktopControls = true;
    [Tooltip("Walking speed in meters per second")]
    public float moveSpeed = 2.4f;
    [Tooltip("Sprint speed in meters per second")]
    public float sprintSpeed = 4.0f;
    [Tooltip("Mouse look sensitivity")]
    public float lookSensitivity = 1.5f;

    [Header("Interaction Raycast")]
    public float interactionDistance = 4.5f;
    public LayerMask interactionLayers = ~0;

    [Header("Pickup Anchor")]
    public Transform holdPoint;
    public float holdSmoothSpeed = 16f;

    [Header("Runtime State")]
    public bool isCursorLocked = true;
    public Rigidbody heldObjectRb = null;
    public Transform heldObject = null;

    // Chair state
    public ChairController currentChair = null;

    // Active reading clue overlay
    public ClueInteraction activeClue = null;

    private Camera cam;
    private float pitch = 0f;
    private float yaw = 0f;
    private CharacterController charController;
    private Vector3 currentVelocity = Vector3.zero;
    private float verticalVelocity = 0f;

    // Hover prompt
    private string currentHoverPrompt = "";
    private bool isHoveringInteractable = false;

    private void Awake()
    {
        cam = GetComponentInChildren<Camera>();
        if (cam == null) cam = Camera.main;

        charController = GetComponent<CharacterController>();
        if (charController != null)
        {
            charController.skinWidth = 0.02f;
            charController.minMoveDistance = 0f;
            charController.stepOffset = 0.35f;

            // Ensure player doesn't spawn embedded inside floor collider
            if (transform.position.y < 0.05f)
            {
                transform.position = new Vector3(transform.position.x, 0.05f, transform.position.z);
            }
        }

        if (holdPoint == null && cam != null)
        {
            GameObject hp = new GameObject("Desktop_HoldPoint");
            hp.transform.SetParent(cam.transform);
            hp.transform.localPosition = new Vector3(0.25f, -0.20f, 0.65f);
            hp.transform.localRotation = Quaternion.identity;
            holdPoint = hp.transform;
        }

        yaw = transform.eulerAngles.y;
        if (cam != null) pitch = cam.transform.localEulerAngles.x;
    }

    private void Start()
    {
        if (!forceDesktopControls && UnityEngine.XR.XRSettings.isDeviceActive && !Application.isEditor)
        {
            Debug.Log("[VRDesktopTester] VR Headset active in standalone. Desktop controls disabled.");
            enabled = false;
            return;
        }

        SetCursorLock(true);
        Debug.Log("[VRDesktopTester] Desktop controls active: WASD to Move, Shift to Sprint, Mouse to Look, Click to Interact.");
    }

    private void Update()
    {
        // Restart game with R key
        if (Input.GetKeyDown(KeyCode.R) && GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
            return;
        }

        // Handle Escape key
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (activeClue != null)
            {
                CloseClueOverlay();
                return;
            }

            SetCursorLock(false);
            return;
        }

        // Auto re-lock cursor if clicking in Game View or pressing any movement/action key
        if (!isCursorLocked)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) ||
                Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) ||
                Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D) ||
                Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space))
            {
                SetCursorLock(true);
            }
        }

        // Close clue overlay with E, Space, Escape, or Left-Click
        if (activeClue != null)
        {
            if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
            {
                CloseClueOverlay();
                return;
            }
        }

        // Mouse look when cursor is locked or when right mouse button is held
        if (isCursorLocked || Input.GetMouseButton(1))
        {
            HandleMouseLook();
        }

        // Movement runs continuously regardless of cursor lock state
        HandleMovement();
        UpdateHoverState();
        CheckProximityDoorUnlock();
        HandleInteraction();
        UpdateHeldObject();
    }

    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * lookSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * lookSensitivity;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -85f, 85f);

        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        if (cam != null)
        {
            cam.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }
    }

    public void SetLookPitch(float newPitch)
    {
        pitch = Mathf.Clamp(newPitch, -85f, 85f);
        if (cam != null)
        {
            cam.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }
    }

    private void HandleMovement()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // Direct key fallbacks so movement is 100% reliable even if axis inputs lag or deadzone
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) h -= 1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) h += 1f;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) v -= 1f;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) v += 1f;

        h = Mathf.Clamp(h, -1f, 1f);
        v = Mathf.Clamp(v, -1f, 1f);

        // If sitting on chair and pressing move or stand keys, stand up!
        if (currentChair != null && currentChair.isOccupied)
        {
            if (Mathf.Abs(h) > 0.1f || Mathf.Abs(v) > 0.1f || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.W))
            {
                currentChair.StandUp(this);
                currentChair = null;
            }
            return;
        }

        bool isSprinting = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        float currentSpeed = isSprinting ? sprintSpeed : moveSpeed;

        Vector3 moveDir = (transform.forward * v + transform.right * h);
        if (moveDir.sqrMagnitude > 1f) moveDir.Normalize();

        Vector3 targetVelocity = moveDir * currentSpeed;
        currentVelocity = Vector3.MoveTowards(currentVelocity, targetVelocity, 25f * Time.deltaTime);

        Vector3 displacement = currentVelocity * Time.deltaTime;

        if (charController != null && charController.enabled)
        {
            if (charController.isGrounded)
            {
                verticalVelocity = -0.5f;
            }
            else
            {
                verticalVelocity -= 9.81f * Time.deltaTime;
            }
            displacement.y = verticalVelocity * Time.deltaTime;

            Vector3 prePos = transform.position;
            charController.Move(displacement);

            // Obstruction/capsule-snag safety: if horizontal movement was requested but zeroed by collision mesh snag, translate directly
            if (moveDir.sqrMagnitude > 0.05f)
            {
                Vector3 horizontalDelta = new Vector3(transform.position.x - prePos.x, 0f, transform.position.z - prePos.z);
                if (horizontalDelta.sqrMagnitude < 0.000001f && displacement.sqrMagnitude > 0.00001f)
                {
                    transform.position += new Vector3(displacement.x, 0f, displacement.z);
                }
            }
        }
        else
        {
            transform.position += displacement;
        }

        // Auto-start game on movement during Intro
        if ((Mathf.Abs(h) > 0.1f || Mathf.Abs(v) > 0.1f) && GameManager.Instance != null && GameManager.Instance.CurrentState == GameManager.GameState.Intro)
        {
            GameManager.Instance.StartGame();
        }
    }

    private void UpdateHoverState()
    {
        if (cam == null) return;

        isHoveringInteractable = false;
        currentHoverPrompt = "";

        bool isHoldingKey = (heldObject != null && heldObject.GetComponent<KeyController>() != null);

        if (heldObject != null)
        {
            if (isHoldingKey)
            {
                currentHoverPrompt = "Holding: Brass Key  |  Click South Door to Unlock & Escape!";
            }
            else
            {
                currentHoverPrompt = $"Holding: {heldObject.name}  |  [Right-Click] Drop";
            }
        }

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, interactionDistance, interactionLayers))
        {
            // Check for IInteractable
            IInteractable interactable = hit.collider.GetComponent<IInteractable>() ?? hit.collider.GetComponentInParent<IInteractable>();
            if (interactable != null)
            {
                string prompt = interactable.InteractionPrompt;
                if (!string.IsNullOrEmpty(prompt))
                {
                    isHoveringInteractable = true;
                    currentHoverPrompt = prompt;
                    return;
                }
            }

            // UI Button
            Button btn = hit.collider.GetComponent<Button>() ?? hit.collider.GetComponentInParent<Button>();
            if (btn != null)
            {
                isHoveringInteractable = true;
                currentHoverPrompt = "[Left Click] Click Button";
                return;
            }

            // General Grabbable
            if (hit.rigidbody != null && !hit.rigidbody.isKinematic)
            {
                isHoveringInteractable = true;
                currentHoverPrompt = $"[Left Click] Pick up {hit.collider.name}";
                return;
            }
        }
    }

    /// <summary>
    /// Automatic unlock when player approaches the exit door while holding the brass key.
    /// </summary>
    private void CheckProximityDoorUnlock()
    {
        if (heldObject == null) return;
        KeyController key = heldObject.GetComponent<KeyController>();
        if (key == null || key.hasBeenUsed) return;

        DoorController door = Object.FindFirstObjectByType<DoorController>();
        if (door != null && door.isLocked)
        {
            float dist = Vector3.Distance(transform.position, door.transform.position);
            if (dist < 2.0f)
            {
                door.UnlockDoor(key);
                heldObject = null;
                heldObjectRb = null;
            }
        }
    }

    private void HandleInteraction()
    {
        if (cam == null) return;

        // Space or Enter during Intro immediately starts game
        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)) && GameManager.Instance != null && GameManager.Instance.CurrentState == GameManager.GameState.Intro)
        {
            GameManager.Instance.StartGame();
            return;
        }

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        RaycastHit hit;
        bool hasHit = Physics.Raycast(ray, out hit, interactionDistance, interactionLayers);

        // Left Click to Interact
        if (Input.GetMouseButtonDown(0))
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameManager.GameState.Intro)
            {
                GameManager.Instance.StartGame();
                return;
            }

            if (hasHit)
            {
                // 1. Chair
                ChairController chair = hit.collider.GetComponent<ChairController>() ?? hit.collider.GetComponentInParent<ChairController>();
                if (chair != null)
                {
                    if (chair.isOccupied)
                    {
                        chair.StandUp(this);
                        currentChair = null;
                    }
                    else
                    {
                        chair.SitDown(this);
                        currentChair = chair;
                    }
                    return;
                }

                // 2. Door with key
                DoorController door = hit.collider.GetComponent<DoorController>() ?? hit.collider.GetComponentInParent<DoorController>();
                if (door != null && door.isLocked)
                {
                    if (heldObject != null)
                    {
                        KeyController heldKey = heldObject.GetComponent<KeyController>();
                        if (heldKey != null)
                        {
                            door.UnlockDoor(heldKey);
                            heldObject = null;
                            heldObjectRb = null;
                            return;
                        }
                    }
                }

                // 3. Clue
                ClueInteraction clue = hit.collider.GetComponent<ClueInteraction>() ?? hit.collider.GetComponentInParent<ClueInteraction>();
                if (clue != null)
                {
                    OpenClueOverlay(clue);
                    return;
                }

                // 4. Keypad Button
                KeypadButton kpBtn = hit.collider.GetComponent<KeypadButton>() ?? hit.collider.GetComponentInParent<KeypadButton>();
                if (kpBtn != null)
                {
                    kpBtn.Press();
                    return;
                }

                // 5. Brass Key
                KeyController key = hit.collider.GetComponent<KeyController>() ?? hit.collider.GetComponentInParent<KeyController>();
                if (key != null && !key.hasBeenUsed)
                {
                    GrabObject(key.gameObject);
                    key.OnGrabbed();
                    return;
                }

                // 6. UI Button
                Button btn = hit.collider.GetComponent<Button>() ?? hit.collider.GetComponentInParent<Button>();
                if (btn != null)
                {
                    btn.onClick.Invoke();
                    return;
                }

                // 7. General Grabbable
                if (heldObject == null && hit.rigidbody != null && !hit.rigidbody.isKinematic)
                {
                    GrabObject(hit.collider.gameObject);
                    return;
                }
            }

            // Drop non-key object if clicking empty space
            if (heldObject != null && heldObject.GetComponent<KeyController>() == null)
            {
                DropHeldObject();
                return;
            }
        }

        // Right Click to Drop
        if (Input.GetMouseButtonDown(1) && heldObject != null)
        {
            DropHeldObject();
        }

        // E key to Read hovered clue directly
        if (Input.GetKeyDown(KeyCode.E) && hasHit)
        {
            ClueInteraction clue = hit.collider.GetComponent<ClueInteraction>() ?? hit.collider.GetComponentInParent<ClueInteraction>();
            if (clue != null)
            {
                OpenClueOverlay(clue);
            }
        }
    }

    private void OpenClueOverlay(ClueInteraction clue)
    {
        activeClue = clue;
        clue.InspectClue();
        if (SoundManager.Instance != null) SoundManager.Instance.PlayButtonClick();
    }

    public void CloseClueOverlay()
    {
        if (activeClue != null)
        {
            activeClue.StopInspectClue();
            activeClue = null;
        }
        if (UIManager.Instance != null)
        {
            UIManager.Instance.HideClueInspection();
            UIManager.Instance.HideAllCluesFound();
        }
    }

    public void GrabObject(GameObject obj)
    {
        heldObject = obj.transform;
        heldObjectRb = obj.GetComponent<Rigidbody>();

        if (heldObjectRb != null)
        {
            heldObjectRb.isKinematic = true;
            heldObjectRb.useGravity = false;
        }

        heldObject.SetParent(holdPoint);
        heldObject.localPosition = Vector3.zero;
        heldObject.localRotation = Quaternion.identity;

        Debug.Log($"[VRDesktopTester] Grabbed {obj.name}");
    }

    public void DropHeldObject()
    {
        if (heldObject == null) return;

        heldObject.SetParent(null);
        if (heldObjectRb != null)
        {
            heldObjectRb.isKinematic = false;
            heldObjectRb.useGravity = true;
            heldObjectRb.linearVelocity = (cam != null ? cam.transform.forward : transform.forward) * 1.5f;
        }

        KeyController key = heldObject.GetComponent<KeyController>();
        if (key != null) key.OnReleased();

        heldObject = null;
        heldObjectRb = null;
    }

    private void UpdateHeldObject()
    {
        if (heldObject != null && holdPoint != null)
        {
            heldObject.position = Vector3.Lerp(heldObject.position, holdPoint.position, Time.deltaTime * holdSmoothSpeed);
            heldObject.rotation = Quaternion.Slerp(heldObject.rotation, holdPoint.rotation, Time.deltaTime * holdSmoothSpeed);
        }
    }

    public void SetCursorLock(bool locked)
    {
        isCursorLocked = locked;
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }

    // --- Dynamic On-Screen Reticle & Context Prompts ---
    private void OnGUI()
    {
        if (!isCursorLocked) return;

        // Subtle crosshair dot in center of screen
        float centerX = Screen.width / 2f;
        float centerY = Screen.height / 2f;
        float dotSize = isHoveringInteractable ? 8f : 4f;
        Color dotColor = isHoveringInteractable ? new Color(1f, 0.9f, 0.3f, 0.9f) : new Color(1f, 1f, 1f, 0.5f);

        GUI.color = dotColor;
        GUI.DrawTexture(new Rect(centerX - dotSize / 2f, centerY - dotSize / 2f, dotSize, dotSize), Texture2D.whiteTexture);

        // Interaction prompt below crosshair
        if (!string.IsNullOrEmpty(currentHoverPrompt))
        {
            GUIStyle promptStyle = new GUIStyle(GUI.skin.label);
            promptStyle.alignment = TextAnchor.MiddleCenter;
            promptStyle.fontSize = 15;
            promptStyle.fontStyle = FontStyle.Bold;
            promptStyle.normal.textColor = Color.white;

            // Background pill
            Vector2 size = promptStyle.CalcSize(new GUIContent(currentHoverPrompt));
            float padding = 16f;
            Rect bgRect = new Rect(centerX - (size.x + padding) / 2f, centerY + 28f, size.x + padding, size.y + 8f);

            GUI.color = new Color(0.05f, 0.05f, 0.08f, 0.85f);
            GUI.DrawTexture(bgRect, Texture2D.whiteTexture);

            GUI.color = new Color(1f, 0.9f, 0.4f, 1f);
            GUI.Label(bgRect, currentHoverPrompt, promptStyle);
        }
    }
}
