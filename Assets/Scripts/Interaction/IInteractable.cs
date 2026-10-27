using UnityEngine;

/// <summary>
/// Interaction interface providing a unified abstraction for all interactable objects in the escape room.
/// Both VR interactors (direct touch, ray, grab) and Desktop input (mouse click, 'E' key) invoke this interface,
/// guaranteeing consistent gameplay logic and avoiding duplicate interaction code.
/// </summary>
public interface IInteractable
{
    /// <summary>
    /// User-friendly hover prompt describing what interaction will occur (e.g. "Read Celestial Star Journal").
    /// </summary>
    string InteractionPrompt { get; }

    /// <summary>
    /// Executes the primary gameplay interaction on this object.
    /// </summary>
    /// <param name="user">The GameObject that triggered the interaction (e.g. Player Rig or Controller).</param>
    void Interact(GameObject user);
}
