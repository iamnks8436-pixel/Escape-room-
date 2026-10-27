using System;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// XRInteractableAdapter connects Unity XR Interaction Toolkit interactables (XRGrabInteractable,
/// XRSimpleInteractable) with escape room gameplay logic.
/// Uses reflection to seamlessly attach XRI 3.x or 2.x components at runtime while maintaining
/// 100% clean compilation and full desktop fallback compatibility.
/// </summary>
public class XRInteractableAdapter : MonoBehaviour
{
    public enum InteractableType
    {
        Grab,       // Keys, Journals, Parchment notes
        Simple,     // Buttons, Levers
        Socket      // Keyholes, Pedestals
    }

    [Header("Interaction Mode")]
    public InteractableType interactionType = InteractableType.Grab;

    [Header("XR Events")]
    public UnityEvent onSelected = new UnityEvent();
    public UnityEvent onDeselected = new UnityEvent();
    public UnityEvent onHoverEntered = new UnityEvent();
    public UnityEvent onHoverExited = new UnityEvent();

    private void Awake()
    {
        AttachXRComponents();
    }

    /// <summary>
    /// Attaches XRI interactable components dynamically based on available package versions.
    /// </summary>
    private void AttachXRComponents()
    {
        Type targetType = null;

        if (interactionType == InteractableType.Grab)
        {
            // XRI 3.x (Unity 6)
            targetType = Type.GetType("UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable, Unity.XR.Interaction.Toolkit")
                      ?? Type.GetType("UnityEngine.XR.Interaction.Toolkit.XRGrabInteractable, Unity.XR.Interaction.Toolkit");
        }
        else if (interactionType == InteractableType.Simple)
        {
            // XRI 3.x (Unity 6)
            targetType = Type.GetType("UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable, Unity.XR.Interaction.Toolkit")
                      ?? Type.GetType("UnityEngine.XR.Interaction.Toolkit.XRSimpleInteractable, Unity.XR.Interaction.Toolkit");
        }

        if (targetType != null)
        {
            Component existing = GetComponent(targetType);
            if (existing == null)
            {
                existing = gameObject.AddComponent(targetType);
                Debug.Log($"[XRInteractableAdapter] Attached {targetType.Name} to {gameObject.name}");
            }
        }
    }

    public void TriggerSelect() => onSelected?.Invoke();
    public void TriggerDeselect() => onDeselected?.Invoke();
}
